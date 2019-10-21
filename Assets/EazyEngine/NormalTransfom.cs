using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTransfom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0,0,(float)Math.PI*Time.realtimeSinceStartup*10);
        transform.position += new Vector3(0,0.01f,0);
    }
}
