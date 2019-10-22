using System.Collections;
using System.Collections.Generic;
using EazyEngine.ECS.Components;
using EazyEngine.ECS.Ultis;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;


namespace EazyEngine.ECS.System
{
    public class EzFindTargetJobSystem : JobComponentSystem
    {
        private struct EntityWithPosition
        {
            public Entity entity;
            public int layer;
            public float3 position;
        }
     
        [ExcludeComponent(typeof(EzHasTarget))]
        [BurstCompile]
        private struct FindTargetJob : IJobForEachWithEntity<Translation,EzUnit>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
            public NativeArray<Entity> closestTargetEntityArray;
 
            public void Execute(Entity entity, int index, ref Translation c0,ref  EzUnit pUnit)
            {
                float3 unitPos = c0.Value;
                float3 closetTargetPos = float3.zero;
                Entity closetETarget = Entity.Null;
                for (int i = 0; i < targetArray.Length; ++i)
                {
                    if(!EzLayers.LayerInLayerMask(targetArray[i].layer,pUnit.layerTarget)) continue;
                    if (closetETarget == Entity.Null)
                    {
                        closetETarget = targetArray[i].entity;
                        closetTargetPos = targetArray[i].position;
                    }
                    else
                    {
                        if (math.distance(unitPos, targetArray[i].position) < math.distance(closetTargetPos, unitPos))
                        {
                            closetETarget = targetArray[i].entity;
                            closetTargetPos = targetArray[i].position;
                        }
                    }
                }
            

                closestTargetEntityArray[index] = closetETarget;
            }
        }
        [ExcludeComponent(typeof(EzHasTarget))]
        [RequireComponentTag(typeof(EzUnit))]
        private struct AddComponentJob : IJobForEachWithEntity<Translation>
        {
            [DeallocateOnJobCompletion][ReadOnly]public NativeArray<Entity> closestTargetEntityArray;
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public void Execute(Entity entity, int index, ref Translation c0)
            {
                if (closestTargetEntityArray[index] != Entity.Null)
                {
                    entityCommandBuffer.AddComponent<EzHasTarget>(index,entity,new EzHasTarget()
                    {
                        target = closestTargetEntityArray[index]
                    });
                }
            }
        }

        protected EndSimulationEntityCommandBufferSystem commandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityQuery targetQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    typeof(EzTarget),
                    ComponentType.ReadOnly<Translation>()
                },
            });
            var pArrayEntity = targetQuery.ToEntityArray(Allocator.TempJob);
            var pArrayTranslation = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var pArrayEztarget = targetQuery.ToComponentDataArray<EzTarget>(Allocator.TempJob);
            EntityQuery pUnitNontarget = GetEntityQuery(typeof(EzUnit),typeof(Translation), ComponentType.Exclude<EzHasTarget>());
            var pClosedFindEntity = new NativeArray<Entity>(pUnitNontarget.CalculateEntityCount(), Allocator.TempJob);

            NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(pArrayEntity.Length,Allocator.TempJob);
            for (int i = 0; i < pArrayEntity.Length; ++i)
            {
                targetArray[i] = new EntityWithPosition()
                {
                    entity = pArrayEntity[i],
                    layer =  pArrayEztarget[i].layer.layer,
                    position = pArrayTranslation[i].Value
                };
            }
            pArrayEntity.Dispose();
            pArrayTranslation.Dispose();
            pArrayEztarget.Dispose();
            FindTargetJob jobFind = new FindTargetJob()
            {    
                targetArray =  targetArray,
                closestTargetEntityArray =  pClosedFindEntity
            };
            var pAddComponentJob = new AddComponentJob()
            {
                entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                closestTargetEntityArray = pClosedFindEntity,
            };
            JobHandle pJob = jobFind.Schedule(this, inputDeps);
            pJob = pAddComponentJob.Schedule(this, pJob);
            commandBufferSystem.AddJobHandleForProducer(pJob);
            return pJob;
        }
    }
    public class AddBehaviorWithTargetSystem : JobComponentSystem
    {
        private struct AddBehaviorJobWithTarget : IJobChunk
        {      
            [ReadOnly] public ArchetypeChunkComponentType<EzRotateToTarget> rotateToTarget;
            [ReadOnly] public ArchetypeChunkComponentType<EzMoveToTarget> moveToTarget;
            [ReadOnly] public ArchetypeChunkComponentType<HasTargetMove> targetMove;
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Entity> entities;
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                bool hasMove = chunk.Has(moveToTarget);
                bool hasTargetMove= chunk.Has(targetMove);
                bool hasRotation = chunk.Has(rotateToTarget);

                int pCount = chunk.Count;
                if (hasMove && !hasTargetMove)
                {
                    for (int i = 0; i < pCount; ++i)
                    {
                        entityCommandBuffer.AddComponent<HasTargetMove>(chunkIndex, entities[firstEntityIndex +i], new HasTargetMove());
                    }
               
                }

                if (hasRotation)
                {
                   // entityCommandBuffer.AddComponent<HasTargetMove>();
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem commandBufferSystem;
        private EntityQuery m_Group;
        protected override void OnCreate()
        {
            base.OnCreate();
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Group =  GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    typeof(EzHasTarget)                    
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<EzMoveToTarget>(),
                    ComponentType.ReadOnly<EzRotateToTarget>(), 
                    ComponentType.ReadOnly<HasTargetMove>(), 
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var move = GetArchetypeChunkComponentType<EzMoveToTarget>(true);
            var rotation = GetArchetypeChunkComponentType<EzRotateToTarget>(true);
            var targetMove = GetArchetypeChunkComponentType<HasTargetMove>(true);
            var pEntities = m_Group.ToEntityArray(Allocator.TempJob);
            var pJobAdd = new AddBehaviorJobWithTarget()
            {
                entities = pEntities,
                targetMove = targetMove,
                moveToTarget = move,
                rotateToTarget =  rotation,
                entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
           var pJobHandle  = pJobAdd.Schedule(m_Group, inputDeps);
            return pJobHandle;
        }
    }
    public class UpdateTargetPosToMove : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<HasTargetMove>().WithAll<EzMoveToTarget>().ForEach((Entity unit, ref EzHasTarget pTarget) =>
            {
                var pTrans =  World.Active.EntityManager.GetComponentData<Translation>(pTarget.target);
                World.Active.EntityManager.SetComponentData(unit,new HasTargetMove()
                {
                    target = pTrans.Value
                });
            });
        }
    }

/*    public class MoveToTargetSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<EzMoveToTarget>().ForEach((Entity unit,ref Translation pTrans, ref EzHasTarget pTarget , ref EzMovement pMove) =>
            {
                var pTransTarget =  World.Active.EntityManager.GetComponentData<Translation>(pTarget.target);
                float3 dir = math.normalize(pTransTarget.Value - pTrans.Value);
                pTrans.Value += dir * pMove.Speed * Time.deltaTime;
                /*World.Active.EntityManager.SetComponentData(unit,new HasTargetMove()
                {
                    target = pTrans.Value
                });#1#
            });
        }
    }*/
/*    public class CheckCompleteMoveToTarget : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            throw new NotImplementedException();
        }
    }*/
}
