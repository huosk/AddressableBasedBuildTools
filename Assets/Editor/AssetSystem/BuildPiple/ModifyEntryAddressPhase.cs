using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using System.IO;

public class ModifyEntryAddressPhase : APipePhase
{
    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        if (context.assets == null)
            return true;

        for (int i = 0; i < context.assets.Count; i++)
        {
            var entry = context.assets[i];
            if (entry == null)
                continue;

            var assetPath = entry.assetPath;
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                continue;

            var assetEntry = context.setting.FindAssetEntry(guid);
            if (assetEntry == null)
                continue;

            // 修改 entry 的地址
            assetEntry.address = GetEntryAddress(assetPath, context);
        }

        await Task.FromResult(true);
        return true;
    }

    private static string GetEntryAddress(string assetPath, PipeContext context)
    {
        var type = context.buildSetting.AssetKeyBuildType;
        if (type == EntryAddressType.UseNameNoExtension)
            return Path.GetFileNameWithoutExtension(assetPath);
        else if (type == EntryAddressType.UseNameWithExtension)
            return Path.GetFileName(assetPath);
        else if (type == EntryAddressType.RelativeToBuildFolder)
        {
            string buildFolder = context.buildSetting.BuildFolder.Replace('\\', '/').Trim('/');
            string nAssetPath = assetPath.Replace('\\', '/');
            if (nAssetPath.StartsWith(buildFolder))
                return nAssetPath.Substring(buildFolder.Length).Trim('/', '\\');
        }

        return assetPath;
    }
}
