using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace  EazyEngine.ECS.Components
{
    [Serializable]
    public struct EzSingleLayer
    {
        public int layer;
    }
    [Serializable]
    public struct EzUnit : IComponentData
    {
        public LayerMask layerTarget;
    };
    [Serializable]
  
    public struct EzTarget : IComponentData
    {
        public EzSingleLayer layer;
    };
    public struct EzHasTarget : IComponentData
    {
        public Entity target;
    };

    public struct EzMoveToTarget : IComponentData
    {
    };
    public struct EzRotateToTarget : IComponentData
    {
    };
    public struct EzTargeted : IBufferElementData
    {
        public Entity targetBy;
    };
}
