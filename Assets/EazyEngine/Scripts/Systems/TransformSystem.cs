using EazyEngine.ECS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace EazyEngine.ECS.System
{
    public class MovementToTargetSystem : JobComponentSystem    
    {
        [BurstCompile]
        private struct MoveJob : IJobForEachWithEntity<Translation,HasTargetMove,EzMovement>
        {
            public float deltaTime;
            public void Execute(Entity entity, int index, ref Translation c0,[Unity.Collections.ReadOnly] ref HasTargetMove pTarget,ref EzMovement pMoveData)
            {
                c0.Value += math.normalize(pTarget.target - c0.Value)*pMoveData.Speed*deltaTime;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var pJob = new MoveJob()
            {
                deltaTime =  Time.deltaTime
            };
            return pJob.Schedule(this,inputDeps);
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
