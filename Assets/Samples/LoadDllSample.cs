using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadDllSample : MonoBehaviour
{
    private IModule loadedModule;

    [ExportAssembly]
    static List<Assembly> ExportList()
    {
        return new List<Assembly>() {
            typeof(LoadDllSample).Assembly,
            typeof(Newtonsoft.Json.JsonConvert).Assembly,
        };
    }

    // Start is called before the first frame update
    async void Start()
    {
        var loadHandle = Addressables.LoadAssetAsync<TextAsset>("com.jucheng.yixian.bytes");

        try
        {
            await loadHandle.Task;

            Debug.Log("加载完毕：：" + loadHandle.Status);

            string saveFile = Application.persistentDataPath + "/com.jucheng.yixian.dll";
            File.WriteAllBytes(saveFile, loadHandle.Result.bytes);

            var assembly = Assembly.LoadFrom(saveFile);
            var moduleType = assembly.GetExportedTypes().FirstOrDefault((v) => typeof(IModule).IsAssignableFrom(v) &&
            Attribute.IsDefined(v, typeof(EntryModuleAttribute)));

            Debug.Log("找到目标模块：：" + moduleType.ToString());

            var moduleInst = System.Activator.CreateInstance(moduleType) as IModule;

            loadedModule = moduleInst;

            await moduleInst.LoadAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (loadedModule != null)
                loadedModule.ReleaseAsync();
        }
    }
}
