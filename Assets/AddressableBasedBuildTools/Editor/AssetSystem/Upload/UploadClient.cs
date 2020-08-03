using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.IO;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

public class UploadClient
{
    static HostType HostType = HostType.Ftp;
    static string Host = "127.0.0.1";
    static string UserName = "test";
    static string Password = "aLLxYYHKYC5MRRRJ";

    [MenuItem("Tools/Build Asset System/上传到服务器")]
    static async void UpdateToServer()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        if (setting == null)
            return;

        // 创建组模板
        var groupTemplete = PipleUtility.GetTemplete(setting, false);
        if (groupTemplete == null)
        {
            EditorUtility.DisplayDialog("提示", "上传失败，未找到组模板！", "确定");
            return;
        }

        var groupTempleteBundleSchema = groupTemplete.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        string groupBuildPath = groupTempleteBundleSchema.BuildPath.GetValue(setting.profileSettings, setting.activeProfileId);
        string groupLoadPath = groupTempleteBundleSchema.LoadPath.GetValue(setting.profileSettings, setting.activeProfileId);

        UploadEntryPhase phase = new UploadEntryPhase();
        phase.HostType = HostType;
        phase.Host = Host;
        phase.UserName = UserName;
        phase.Password = Password;
        phase.RemoteBuildPath = groupBuildPath;
        phase.RemoteLoadPath = groupLoadPath;
        phase.OnWillProcess = (c) => EditorUtility.DisplayDialog("提示", "是否上传到服务器？", "是", "取消");

        PipeContext context = new PipeContext();

        try
        {
            await phase.Process(context);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}