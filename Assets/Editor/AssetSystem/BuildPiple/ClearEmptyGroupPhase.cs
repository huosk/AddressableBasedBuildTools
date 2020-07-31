using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

/// <summary>
/// 清除空组
/// </summary>
public class ClearEmptyGroupPhase : APipePhase
{
    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new ArgumentNullException("context");

        var setting = context.setting;

        var currentGroups = setting.groups.ToArray();
        for (int i = 0; i < currentGroups.Length; i++)
        {
            var g = currentGroups[i];
            if (g.Default)
                continue;

            var bundleSchema = g.GetSchema<BundledAssetGroupSchema>();
            if (bundleSchema == null)
                continue;

            if (g.entries.Count == 0)
                setting.RemoveGroup(g);
        }

        await Task.FromResult(true);
        return true;
    }
}