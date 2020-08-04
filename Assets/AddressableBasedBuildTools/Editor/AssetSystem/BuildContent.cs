using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class BuildContent
{
    static string settingFile = "Assets/AddressableBasedBuildTools/Editor/AssetSystem/BuildSetting.asset";

    [MenuItem("Tools/Build Asset System/更新资源")]
    static async void UpdateContent()
    {
        // 保存一下项目，否则有些修改将无法起效
        if (!NotifySaveProject())
            return;

        var systemSetting = GetOrCreateSystemSetting();
        if (string.IsNullOrEmpty(systemSetting.BuildFolder))
        {
            Debug.LogError("未设置打包目录.");
            return;
        }

        var setting = AddressableAssetSettingsDefaultObject.Settings;
        if (setting == null)
            return;

        // 创建组模板
        var groupTemplete = PipleUtility.GetTemplete(setting, true);
        var groupTempleteBundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        string groupBuildPath = groupTempleteBundleSchema.BuildPath.GetValue(setting.profileSettings, setting.activeProfileId);
        string groupLoadPath = groupTempleteBundleSchema.LoadPath.GetValue(setting.profileSettings, setting.activeProfileId);

        PipeContext context = new PipeContext();
        context.buildSetting = systemSetting;
        context.setting = setting;
        context.groupTemplete = groupTemplete;

        // 构建内容更新管线
        BuildPipe pipe = new BuildPipe();
        pipe.AddPhase(new ClearEmptyGroupPhase());
        pipe.AddPhase(new CollectBuildEntryPhase() { TargetPath = systemSetting.BuildFolder });

        if (systemSetting.ExportDll)
            pipe.AddPhase(new CollectDllAsTextPhase());

        pipe.AddPhase(new CollectDependencyPhase());
        pipe.AddPhase(new CreateGroupPhase());
        pipe.AddPhase(new ModifyEntryAddressPhase());

        pipe.AddPhase(new UpdateContentPhase());
        pipe.AddPhase(new GeneratePreviewPhase() { OutputPath = Path.Combine(groupBuildPath, "preview").Replace('\\', '/') });
        pipe.AddPhase(new SaveManifestPhase() { OutputPath = groupBuildPath });
        pipe.AddPhase(new UploadEntryPhase()
        {
            HostType = systemSetting.UploadHostType,
            Host = systemSetting.UploadHost,
            UserName = systemSetting.UserName,
            Password = systemSetting.Password,
            RemoteBuildPath = groupBuildPath,
            RemoteLoadPath = groupLoadPath,
            OnWillProcess = (c) =>
            {
                return EditorUtility.DisplayDialog("提示", "是否上传到服务器？", "是", "取消");
            }
        });

        try
        {
            await pipe.ProcessPiple(context);
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
        if (!NotifySaveProject())
            return;

        var systemSetting = GetOrCreateSystemSetting();

        var setting = AddressableAssetSettingsDefaultObject.Settings;
        if (setting == null)
            return;

        if (!EditorUtility.DisplayDialog("警告", "是否确定打新版本包？", "是", "否"))
        {
            return;
        }

        // 创建组模板
        var groupTemplete = PipleUtility.GetTemplete(setting, true);
        var groupTempleteBundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        string groupBuildPath = groupTempleteBundleSchema.BuildPath.GetValue(setting.profileSettings, setting.activeProfileId);

        PipeContext context = new PipeContext();
        context.buildSetting = systemSetting;
        context.setting = setting;
        context.groupTemplete = groupTemplete;

        BuildPipe pipe = new BuildPipe();
        pipe.AddPhase(new ClearEmptyGroupPhase());
        pipe.AddPhase(new CollectBuildEntryPhase() { TargetPath = systemSetting.BuildFolder });
        pipe.AddPhase(new CollectDependencyPhase() { Recursive = false });
        pipe.AddPhase(new CreateGroupPhase());
        pipe.AddPhase(new ModifyEntryAddressPhase());

        pipe.AddPhase(new BuildContentPhase() { Setting = setting });
        pipe.AddPhase(new GeneratePreviewPhase() { OutputPath = Path.Combine(groupBuildPath, "preview").Replace('\\', '/') });
        pipe.AddPhase(new SaveManifestPhase() { OutputPath = groupBuildPath });

        try
        {
            await pipe.ProcessPiple(context);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        Debug.Log("资源更新完成");
    }

    static bool NotifySaveProject()
    {
        if (EditorUtility.DisplayDialog("提示", "是否保存项目?", "确定", "取消"))
        {
            AssetDatabase.SaveAssets();
            return true;
        }

        return false;
    }

    static BuildSystemSetting GetOrCreateSystemSetting()
    {
        var setting = AssetDatabase.LoadAssetAtPath<BuildSystemSetting>(settingFile);
        if (setting == null)
        {
            string dir = Path.GetDirectoryName(settingFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var newObj = ScriptableObject.CreateInstance<BuildSystemSetting>();
            AssetDatabase.CreateAsset(newObj, settingFile);
            setting = AssetDatabase.LoadAssetAtPath<BuildSystemSetting>(settingFile);
            EditorUtility.SetDirty(setting);
        }

        return setting;
    }
}

[Serializable]
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
    Shader,
    Text,
    Binary,
    Custom,
}

[Serializable]
public class AssetEntry
{
    public AssetType type = AssetType.Unknown;
    public string assetPath = null;
    public string preview = null;
    public string[] tags = null;
}