using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;
using System.Net;
using System.Linq;

public class UploadEntryPhase : APipePhase
{
    public HostType HostType { get; set; }
    public string Host { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string RemoteBuildPath { get; set; }
    public string RemoteLoadPath { get; set; }
    public Func<PipeContext, bool> OnWillProcess { get; set; }

    private IUploadClient client;

    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        if (OnWillProcess != null && !OnWillProcess(context))
            return true;

        if (client == null)
        {
            var ipAddress = await Dns.GetHostAddressesAsync(Host);
            var ipv4Addr = ipAddress.FirstOrDefault((v) => v.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            client = GetClientByHost(HostType.Ftp);
            client.Host = ipv4Addr.ToString();
            client.UserName = UserName;
            client.Password = Password;

            await client.Initialize();
        }

        List<UploadGroupInfo> needToUpload = new List<UploadGroupInfo>();
        needToUpload.Add(new UploadGroupInfo()
        {
            localPath = RemoteBuildPath,
            remotePath = RemoteLoadPath,
            files = Directory.GetFiles(RemoteBuildPath, "*.*", SearchOption.AllDirectories)
        });

        bool success = true;
        try
        {
            await UploadGroups(client, needToUpload);
        }
        catch (System.Exception e)
        {
            success = false;
            Debug.LogException(e);
        }
        finally
        {
            await client.Release();
            client = null;
        }

        return success;
    }

    public static IUploadClient GetClientByHost(HostType type)
    {
        if (type == HostType.Ftp)
            return new FtpUploadClient();
        else if (type == HostType.Http)
            return new HttpUploadClient();
        else
            throw new NotSupportedException("不支持上传::" + type);
    }

    public static async Task UploadGroups(IUploadClient uploader, List<UploadGroupInfo> uploadGroups)
    {
        int allFileCount = 0;
        uploadGroups.ForEach(v => allFileCount += v.files.Length);

        int uploadedCount = 0;
        for (int i = 0; i < uploadGroups.Count; i++)
        {
            var groupBuildPath = uploadGroups[i].localPath;
            var groupLoadPath = uploadGroups[i].remotePath;
            var files = uploadGroups[i].files;

            string basePath = groupBuildPath.Replace('\\', '/');
            foreach (var file in files)
            {
                var newFile = file.Replace('\\', '/');
                if (!newFile.StartsWith(basePath))
                {
                    Debug.LogFormat("{0} not at path::{1}", newFile, basePath);
                    continue;
                }

                var relPath = newFile.Substring(basePath.Length);
                var remoteFile = (groupLoadPath + relPath).Replace('\\', '/');

                try
                {
                    bool success = await uploader.UploadFile(newFile, remoteFile);
                    uploadedCount++;
                    EditorUtility.DisplayProgressBar("上传文件", newFile, (float)uploadedCount / allFileCount);

                    if (success)
                        Debug.LogFormat("上传成功，{0}", remoteFile);
                    else
                        Debug.LogErrorFormat("上传失败，{0}", newFile);
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("上传失败，{0},{1}", remoteFile, e.Message);
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
