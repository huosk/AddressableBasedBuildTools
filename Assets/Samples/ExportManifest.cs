using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ExportManifest
{
    [ExportAssembly]
    static List<Assembly> GetExports()
    {
        return new List<Assembly>() {
            typeof(SampleModule).Assembly
        };
    }
}