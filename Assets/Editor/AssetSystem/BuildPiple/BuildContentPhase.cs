using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;

public class BuildContentPhase : IPiplePhase
{
    public AddressableAssetSettings Setting { get; set; }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        int activeBuilder = 0;
        for (int i = 0; i < Setting.DataBuilders.Count; i++)
        {
            var m = Setting.GetDataBuilder(i);
            if (m.CanBuildData<AddressablesPlayerBuildResult>())
            {
                activeBuilder = i;
                break;
            }
        }

        Setting.ActivePlayerDataBuilderIndex = activeBuilder;
        AddressableAssetSettings.BuildPlayerContent();

        await Task.FromResult(true);
        return true;
    }
}
