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
using Random = UnityEngine.Random;

public class SpawnEnemy : MonoBehaviour
{
    private EntityManager manager;
    public GameObject prefab;
    public GameObject prefabTarget;
    public Vector2 sizeZone;
    public bool tradition = false;
    // Start is called before the first frame update
    void Start()
    {
        manager = World.Active.EntityManager;
        Entity pPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        for (int i = 0; i < 1; ++i)
        {
          // GameObject pObjet = Instantiate(prefab);
           if (!tradition)
           {
               Entity e = manager.Instantiate(pPrefab);
            //   manager.AddComponent<CopyTransformToGameObject>(e);
               manager.AddComponent<LocalToWorld>(e);
               manager.AddComponent<Rotation>(e);
               manager.AddComponentData(e,new EzMovement()
               {
                   Speed = 1
               });
               manager.SetComponentData(e,new Translation(){Value =  new float3(Random.Range(-sizeZone.x/2,sizeZone.x/2),Random.Range(-sizeZone.y/2,sizeZone.y/2),0)});
           }
  
        }

        SpawnTarget();
    }

    public void SpawnTarget()
    {
        Entity pPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        for (int i = 0; i < 1; ++i)
        {
            if (!tradition)
            {
                Entity e = manager.Instantiate(pPrefab);
                manager.AddComponent<CopyTransformToGameObject>(e);
                manager.AddComponent<LocalToWorld>(e);
                manager.AddComponentData(e, new EzTarget()
                {
                    layer = new EzSingleLayer()
                    {
                        layer = 0
                    }
                });
                manager.SetComponentData<Translation>(e,
                    new Translation()
                    {
                        Value = new float3(Random.Range(-sizeZone.x / 2, sizeZone.x / 2),
                            Random.Range(-sizeZone.y / 2, sizeZone.y / 2), 0)
                    });
            }
        }

        //Invoke(nameof(SpawnTarget),1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
