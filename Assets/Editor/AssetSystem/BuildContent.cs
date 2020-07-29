using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuildContent
{
    static string assetBuildFolder = "Assets/Prefabs";

    static string buildProfileName = "RemoteBuildPath";
    static string loadProfileName = "RemoteLoadPath";

    static string previewSaveFolder = "ServerData/Previews";

    [MenuItem("Tools/Build Asset System/更新资源")]
    static async void UpdateContent()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        if (setting == null)
            return;

        // 创建组模板
        var groupTemplete = PipleUtility.GetTemplete(setting);
        var groupTempleteBundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        string groupBuildPath = groupTempleteBundleSchema.BuildPath.GetValue(setting.profileSettings, setting.activeProfileId);

        BuildPiple piple = new BuildPiple();
        piple.AddPhase(new CollectBuildEntryPhase() { TargetPath = assetBuildFolder });
        piple.AddPhase(new CollectDependencyPhase() { Recursive = false });
        piple.AddPhase(new CreateGroupPhase()
        {
            Setting = setting,
            GroupTemplete = groupTemplete
        });

        piple.AddPhase(new UpdateContentPhase() { Setting = setting });
        piple.AddPhase(new GeneratePreviewPhase() { OutputPath = Path.Combine(groupBuildPath, "preview").Replace('\\', '/') });
        piple.AddPhase(new SaveManifestPhase() { OutputPath = groupBuildPath });

        try
        {
            List<AssetEntry> assets = new List<AssetEntry>();
            await piple.ProcessPiple(assets);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        Debug.Log("资源更新完成");
    }

    [MenuItem("Tools/Build Asset System/新版本打包")]
    static async void BuildNewContent()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        if (setting == null)
            return;

        if (!EditorUtility.DisplayDialog("警告", "是否确定打新版本包？", "是", "否"))
        {
            return;
        }

        // 创建组模板
        var groupTemplete = PipleUtility.GetTemplete(setting);
        var groupTempleteBundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        string groupBuildPath = groupTempleteBundleSchema.BuildPath.GetValue(setting.profileSettings, setting.activeProfileId);

        BuildPiple piple = new BuildPiple();
        piple.AddPhase(new CollectBuildEntryPhase() { TargetPath = assetBuildFolder });
        piple.AddPhase(new CollectDependencyPhase() { Recursive = false });
        piple.AddPhase(new CreateGroupPhase()
        {
            Setting = setting,
            GroupTemplete = groupTemplete
        });

        piple.AddPhase(new BuildContentPhase() { Setting = setting });
        piple.AddPhase(new GeneratePreviewPhase() { OutputPath = Path.Combine(groupBuildPath, "preview").Replace('\\', '/') });
        piple.AddPhase(new SaveManifestPhase() { OutputPath = groupBuildPath });

        try
        {
            List<AssetEntry> assets = new List<AssetEntry>();
            await piple.ProcessPiple(assets);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        Debug.Log("资源更新完成");
    }
}

public class AssetManifestFile
{
    public List<AssetEntry> assets = new List<AssetEntry>();
}

public enum AssetType
{
    Unknown = 0,
    GameObject,
    Material,
    Texture,
    AudioClip,
    AnimationClip,
}

public class AssetEntry
{
    public AssetType type;
    public string assetPath;
    public string preview;
    public string[] tags;
}