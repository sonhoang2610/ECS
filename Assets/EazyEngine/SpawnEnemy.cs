using System.Collections;
using System.Collections.Generic;
using EazyEngine.ECS.Components;
using EazyEngine.ECS.Ultis;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public class SpawnEnemy : MonoBehaviour
{
    private EntityManager manager;
    public GameObject prefab;

    public bool tradition = false;
    // Start is called before the first frame update
    void Start()
    {
        manager = World.Active.EntityManager;
        for (int i = 0; i < 500; ++i)
        {
           GameObject pObjet = Instantiate(prefab);
           if (!tradition)
           {
               Entity e = pObjet.GetComponent<GameObjectEntity>().Entity;
               manager.AddComponent<CopyTransformToGameObject>(e);
               manager.AddComponent<LocalToWorld>(e);
               manager.AddComponent<Rotation>(e);
               manager.AddComponentData(e,new EzMovement()
               {
                   Speed = 1
               });
               manager.AddComponentData(e,new HasTargetMove()
               {
                   target =  new float3(0,-2,0)
               });
               manager.AddComponentData<Translation>(e,new Translation(){Value =  new float3(0,1,0)});
           }
  
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
