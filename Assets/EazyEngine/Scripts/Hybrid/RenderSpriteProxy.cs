using System;
using System.Collections;
using System.Collections.Generic;
using EazyEngine.ECS.Components;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(EzRenderSprite))]

namespace  EazyEngine.ECS.Hybrid
{

    [Serializable]
    public class RenderSpriteProxy : SharedComponentDataProxy<EzRenderSprite>
    {
   
    }
}
