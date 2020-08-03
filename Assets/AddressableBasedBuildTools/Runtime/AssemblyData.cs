using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssemblyData : ScriptableObject
{
    /// <summary>
    /// 主 Dll
    /// </summary>
    public TextAsset mainDll;

    /// <summary>
    /// 引用 Dll
    /// </summary>
    public TextAsset[] refDlls;
}
