using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;

public class CreateGroupPhase : IPiplePhase
{
    public AddressableAssetSettings Setting { get; set; }
    public AddressableAssetGroupTemplate GroupTemplete { get; set; }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        if (assets == null)
            throw new System.ArgumentNullException("assets");

        for (int i = 0; i < assets.Count; i++)
        {
            var entry = assets[i];
            if (entry == null)
                continue;

            string assetPath = entry.assetPath;
            CreateGroupForAsset(assetPath, Setting, GroupTemplete);
        }

        await Task.FromResult(true);
        return true;
    }

    static void CreateGroupForAsset(string assetPath, AddressableAssetSettings setting, AddressableAssetGroupTemplate groupTemplete)
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
            CreateEntryToNewGroup(setting, groupTemplete, assetPath);
        }
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
