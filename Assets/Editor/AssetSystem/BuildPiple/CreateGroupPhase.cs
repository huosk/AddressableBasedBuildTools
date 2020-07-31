using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
using System.IO;

public class CreateGroupPhase : APipePhase
{
    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        if (context.setting == null)
            throw new System.ArgumentNullException("context.setting");

        if (context.groupTemplete == null)
            throw new System.ArgumentNullException("context.groupTemplete");

        List<AssetEntry> assets = context.assets;
        if (assets == null)
            return false;

        for (int i = 0; i < assets.Count; i++)
        {
            var entry = assets[i];
            if (entry == null)
                continue;

            string assetPath = entry.assetPath;
            CreateGroupForAsset(assetPath, context.setting, context.groupTemplete);
        }

        await Task.FromResult(true);
        return true;
    }

    /// <summary>
    /// 为指定的资源创建相应的组
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="setting"></param>
    /// <param name="groupTemplete"></param>
    /// <returns></returns>
    static AddressableAssetEntry CreateGroupForAsset(string assetPath, AddressableAssetSettings setting, AddressableAssetGroupTemplate groupTemplete)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = setting.FindAssetEntry(guid);
        if (entry != null)
        {
            var group = entry.parentGroup;
            groupTemplete.ApplyToAddressableAssetGroup(group);
        }
        else
        {
            entry = CreateEntryToNewGroup(setting, groupTemplete, assetPath);
        }
        return entry;
    }

    /// <summary>
    /// 创新新的资源条目，并将添加到新的组中
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="groupTemplete"></param>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    static AddressableAssetEntry CreateEntryToNewGroup(AddressableAssetSettings setting, AddressableAssetGroupTemplate groupTemplete, string assetPath)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = setting.FindAssetEntry(guid);
        if (entry != null)
            return entry;

        var newGroup = setting.CreateGroup(assetPath, false, false, true, null, groupTemplete.GetTypes());
        groupTemplete.ApplyToAddressableAssetGroup(newGroup);

        var entryToAdd = setting.CreateOrMoveEntry(guid, newGroup, false, false);

        newGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entryToAdd, false, true);
        setting.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entryToAdd, true, false);

        return entryToAdd;
    }
}
