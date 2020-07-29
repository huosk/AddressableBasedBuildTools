using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.IO;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

public class UpdateContentPhase : IPiplePhase
{
    public AddressableAssetSettings Setting { get; set; }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        string stateAssetPath = Setting.ConfigFolder;
        stateAssetPath = Path.Combine(stateAssetPath, PlatformMappingService.GetPlatform().ToString());
        var content_state_path = Path.Combine(stateAssetPath, "addressables_content_state.bin");
        var result = ContentUpdateScript.BuildContentUpdate(Setting, content_state_path);
        bool buildSuccess = result != null && string.IsNullOrEmpty(result.Error);

        await Task.FromResult(true);
        return buildSuccess;
    }
}
