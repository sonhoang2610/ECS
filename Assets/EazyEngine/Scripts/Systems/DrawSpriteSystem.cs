using System.Collections;
using System.Collections.Generic;
using EazyEngine.ECS.Components;
using EazyEngine.ECS.Ultis;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;



namespace EazyEngine.ECS.System
{
    public class DrawSpriteSystem : ComponentSystem
    {
        public EntityManager manager;
        protected override void OnCreate()
        {
            base.OnCreate();
            manager = World.Active.EntityManager;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<EzRenderSprite>().WithNone<RenderMesh>().ForEach<EzRenderSprite>((Entity e, EzRenderSprite e1) =>
            {
                var pMat = e1.mat;
                pMat.mainTexture = e1.sprite.texture;
                manager.AddSharedComponentData(e,new RenderMesh()
                {
                    layer =  0,
                    material = pMat,
                    mesh = RenderUltis.CreateMesh(1,1)
                });
            });
        }
    }
}

