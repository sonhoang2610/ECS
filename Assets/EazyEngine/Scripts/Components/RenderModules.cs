using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace EazyEngine.ECS.Components
{
    
    [Serializable]
    public struct EzRenderSprite : ISharedComponentData,IEquatable<EzRenderSprite>
    {
        public Sprite sprite;
        public Material mat;
        public bool Equals(EzRenderSprite other)
        {
            return
                sprite == other.sprite &&
                mat == other.mat;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            if (!ReferenceEquals(sprite, null)) hash ^= sprite.GetHashCode();
            if (!ReferenceEquals(mat, null)) hash ^= mat.GetHashCode();
            return hash;
        }
    }
    
}
