using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.IO;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using System;

public class UpdateContentPhase : APipePhase
{
    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new ArgumentNullException("context");

        if (context.setting == null)
            throw new System.ArgumentNullException("context.setting");

        string stateAssetPath = context.setting.ConfigFolder;
        stateAssetPath = Path.Combine(stateAssetPath, PlatformMappingService.GetPlatform().ToString());
        var content_state_path = Path.Combine(stateAssetPath, "addressables_content_state.bin");
        var result = ContentUpdateScript.BuildContentUpdate(context.setting, content_state_path);
        bool buildSuccess = result != null && string.IsNullOrEmpty(result.Error);

        await Task.FromResult(true);
        return buildSuccess;
    }
}
