using EazyEngine.ECS.Components;
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
    public class MovementToPosSystem : JobComponentSystem
    {
        private EntityQuery m_Group;
        private EndSimulationEntityCommandBufferSystem ecb;
        protected override void OnCreate()
        {
            base.OnCreate();
            ecb = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Group = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    typeof(Translation),
                    typeof(HasTargetMove),  
                    typeof(EzMovement),
                }
            });
        }

        [BurstCompile]
        private struct MoveJob : IJobForEachWithEntity<Translation,HasTargetMove,EzMovement>
        {
            public float deltaTime;
            public NativeArray<Entity> completeJob;
            public void Execute(Entity entity, int index, ref Translation c0,[Unity.Collections.ReadOnly]ref HasTargetMove pTarget,ref EzMovement pMoveData)
            {
                var dir = math.normalize(pTarget.target - c0.Value);
                var deltaMove =   pMoveData.Speed * deltaTime;
                if (deltaMove > math.distance(pTarget.target, c0.Value))
                {
                    c0.Value = pTarget.target;
                    completeJob[index] = pTarget.eventHandle;
                }
                else
                {
                    c0.Value += dir*deltaMove;
                }
            }
        }
        [RequireComponentTag(typeof(EzMovement),typeof(Translation))]
        private struct HandleEventJob : IJobForEachWithEntity<HasTargetMove>
        {
            public EntityCommandBuffer.Concurrent ecb;
            [DeallocateOnJobCompletion] public NativeArray<Entity> completeJob;
            public void Execute(Entity entity, int index, ref HasTargetMove c0)
            {
                if ( completeJob[index] != Entity.Null)
                {
                    ecb.DestroyEntity(index,completeJob[index]);
                }
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
             var pArrrayHastargetMove = new NativeArray<Entity>(m_Group.CalculateEntityCount(),Allocator.TempJob);
            var pJob = new MoveJob()
            {
                completeJob = pArrrayHastargetMove,
                deltaTime =  Time.deltaTime
            };
            var pJobEventHandle = new HandleEventJob()
            {
                completeJob =  pArrrayHastargetMove,
                ecb = ecb.CreateCommandBuffer().ToConcurrent()
            };
            var pJobHandle = pJob.Schedule(this, inputDeps);
            pJobHandle = pJobEventHandle.Schedule(this, pJobHandle);
            ecb.AddJobHandleForProducer(pJobHandle);
            return pJobHandle;
        }
    }
    public class RotationSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct RotationJob : IJobForEachWithEntity<Rotation>
        {
            public float radianSpeed ;
            public void Execute(Entity entity, int index, ref Rotation c0)
            {
                c0.Value = quaternion.Euler(0, 0, math.PI*radianSpeed);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var pJob = new RotationJob()
            {
                radianSpeed =Time.realtimeSinceStartup
            };
            return pJob.Schedule(this,inputDeps);
        }
    }
}
