using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class CollectDependencyPhase : IPiplePhase
{
    public bool Recursive { get; set; }

    public CollectDependencyPhase()
    {
        Recursive = true;
    }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        if (assets == null)
            throw new System.ArgumentNullException("assets");

        AssetEntry[] origins = assets.ToArray();

        for (int i = 0; i < origins.Length; i++)
        {
            var entry = origins[i];
            if (entry == null)
                continue;

            var assetPath = entry.assetPath;
            var deps = AssetDatabase.GetDependencies(assetPath, Recursive);
            for (int j = 0; j < deps.Length; j++)
            {
                var depAssetPath = deps[j];
                if (depAssetPath.Equals(assetPath))
                    continue;

                if (assets.Any(v => v.assetPath.Equals(depAssetPath)))
                    continue;

                var depObj = AssetDatabase.LoadAssetAtPath(depAssetPath, typeof(Object));
                if (depObj == null)
                    continue;

                assets.Add(new AssetEntry()
                {
                    assetPath = depAssetPath,
                    type = PipleUtility.GetAssetType(depObj)
                });
            }
        }

        await Task.FromResult(true);
        return true;
    }
}
