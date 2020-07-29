using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class UploadEntryPhase : IPiplePhase
{
    public IUploadClient Client { get; set; }
    public string RemoteBuildPath { get; set; }
    public string RemoteLoadPath { get; set; }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        List<UploadGroupInfo> needToUpload = new List<UploadGroupInfo>();

        for (int i = 0; i < assets.Count; i++)
        {
            var entry = assets[i];
            if (entry == null)
                continue;

            List<string> files = new List<string>();
            files.Add(entry.assetPath);
            if (!string.IsNullOrEmpty(entry.preview))
            {
                files.Add(entry.preview);
            }

            var inf = new UploadGroupInfo();
            inf.localPath = RemoteBuildPath;
            inf.remotePath = RemoteLoadPath;
            inf.files = files.ToArray();

            needToUpload.Add(inf);
        }

        bool success = true;
        try
        {
            await UploadGroups(Client, needToUpload);
        }
        catch (System.Exception e)
        {
            success = false;
            Debug.LogException(e);
        }

        return success;
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
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}
