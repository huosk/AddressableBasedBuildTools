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

        //var ipAddress = await Dns.GetHostAddressesAsync("localHost");
        //var ipv4 = ipAddress.FirstOrDefault((v) => v.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        IUploadClient uploader = GetClientByHost(HostType);
        uploader.Host = Host;
        uploader.UserName = UserName;
        uploader.Password = Password;

        await uploader.Initialize();

        List<UploadGroupInfo> uploadGroups = CollectFilesToUpload(setting);

        await UploadEntryPhase.UploadGroups(uploader, uploadGroups);

        await uploader.Release();
    }

    private static IUploadClient GetClientByHost(HostType type)
    {
        if (type == HostType.Ftp)
            return new FtpUploadClient();
        else if (type == HostType.Http)
            return new HttpUploadClient();
        else
            throw new NotSupportedException("不支持上传::" + type);
    }

    private static List<UploadGroupInfo> CollectFilesToUpload(AddressableAssetSettings setting)
    {
        List<UploadGroupInfo> uploadGroups = new List<UploadGroupInfo>();

        var activeProfile = setting.activeProfileId;
        var groups = setting.groups;

        HashSet<KeyValuePair<string, string>> groupPathPairs = new HashSet<KeyValuePair<string, string>>();

        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                continue;

            var groupBuildPath = schema.BuildPath.GetValue(setting.profileSettings, activeProfile);
            var groupLoadPath = schema.LoadPath.GetValue(setting.profileSettings, activeProfile);

            groupPathPairs.Add(new KeyValuePair<string, string>(groupBuildPath, groupLoadPath));
        }

        foreach (var kvp in groupPathPairs)
        {
            string groupBuildPath = kvp.Key;
            string groupLoadPath = kvp.Value;

            string[] files = Directory.GetFiles(groupBuildPath, "*.*", SearchOption.AllDirectories);
            uploadGroups.Add(new UploadGroupInfo()
            {
                localPath = groupBuildPath,
                remotePath = groupLoadPath,
                files = files
            });
        }

        return uploadGroups;
    }
}