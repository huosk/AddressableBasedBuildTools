using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SampleModule : IModule
{
    GameObject cube;

    public void Initialize(object state)
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Cube";
    }

    public void Release()
    {
        if (cube != null)
        {
            Object.Destroy(cube);
        }
    }
}