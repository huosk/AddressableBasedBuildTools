using UnityEngine;
using System.Collections;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor;

public class PipleUtility
{
    public static AssetType GetAssetType(Object obj)
    {
        System.Type tp = obj.GetType();
        if (tp == typeof(GameObject))
            return AssetType.GameObject;
        else if (tp == typeof(Texture2D))
            return AssetType.Texture;
        else if (tp == typeof(AudioClip))
            return AssetType.AudioClip;
        else if (tp == typeof(Material))
            return AssetType.Material;
        else if (tp == typeof(AnimationClip))
            return AssetType.AnimationClip;
        else
            return AssetType.Unknown;
    }

    /// <summary>
    /// 获取组模板
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static AddressableAssetGroupTemplate GetTemplete(AddressableAssetSettings setting)
    {
        string groupTempleteName = @"__generated";
        string groupTempleteDescription = @"group templete only use for auto build";

        System.Func<ScriptableObject, bool> groupTempleteFilter = delegate (ScriptableObject v)
        {
            var gt = v as AddressableAssetGroupTemplate;
            return gt.Name.ToLower().Equals(groupTempleteName);
        };

        var groupTemplete = setting.GroupTemplateObjects.FirstOrDefault(groupTempleteFilter) as AddressableAssetGroupTemplate;

        if (groupTemplete == null)
        {
            if (!setting.CreateAndAddGroupTemplate(groupTempleteName, groupTempleteDescription, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema)))
            {
                Debug.LogError("未设置 Group 模板");
                return null;
            }

            groupTemplete = setting.GroupTemplateObjects.FirstOrDefault(groupTempleteFilter) as AddressableAssetGroupTemplate;
            setting.SetDirty(AddressableAssetSettings.ModificationEvent.GroupTemplateAdded, groupTemplete, true, true);
        }

        var bundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        bundleSchema.BuildPath.SetVariableByName(setting, "RemoteBuildPath");
        bundleSchema.LoadPath.SetVariableByName(setting, "RemoteLoadPath");
        EditorUtility.SetDirty(bundleSchema);

        var updateSchema = groupTemplete.GetSchemaByType(typeof(ContentUpdateGroupSchema)) as ContentUpdateGroupSchema;
        updateSchema.StaticContent = false;
        EditorUtility.SetDirty(updateSchema);

        return groupTemplete;
    }
}
