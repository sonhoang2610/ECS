using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace EazyEngine.ECS.Components
{
    public struct EzMovement : IComponentData
    {
        public float Speed;
    }
    public struct HasTargetMove : IComponentData
    {
        public float3 target;
    }
    public struct HasTargetRotation : IComponentData
    {
        public float target;
    }
}