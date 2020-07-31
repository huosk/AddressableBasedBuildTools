using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class CollectBuildEntryPhase : APipePhase
{
    public string TargetPath { get; set; }

    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        if (context.assets == null)
            context.assets = new List<AssetEntry>();

        List<string> files = CollectAssetsToBuild(TargetPath);
        for (int i = 0; i < files.Count; i++)
        {
            string assetPath = files[i];
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            var type = PipleUtility.GetAssetType(obj);

            context.assets.Add(new AssetEntry()
            {
                assetPath = assetPath,
                type = type
            });
        }

        await Task.FromResult(true);
        return true;
    }

    static List<string> CollectAssetsToBuild(string folder)
    {
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException("要打包的目录不存在");

        List<string> list = new List<string>();
        string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        string projPath = System.Environment.CurrentDirectory.Replace('\\', '/');

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i].Replace('\\', '/');
            if (file.EndsWith("meta"))
                continue;

            if (file.StartsWith(projPath))
                file = file.Substring(projPath.Length);

            var obj = AssetDatabase.LoadAssetAtPath(file, typeof(UnityEngine.Object));
            if (obj == null)
                continue;

            list.Add(file);
        }

        return list;
    }
}
