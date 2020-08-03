using UnityEngine;
using System.Collections;
using FluentFTP;
using System.Threading.Tasks;
using System.Net;
using System;

public class FtpUploadClient : IUploadClient
{
    FtpClient client;

    public HostType Type { get { return HostType.Ftp; } }
    public string Host { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }

    public async Task Initialize()
    {
        client = new FtpClient(Host);
        client.Credentials = new NetworkCredential(UserName, Password);

        try
        {
            client.Connect();
            await Task.FromResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task Release()
    {
        if (client != null)
            await client.DisconnectAsync();
    }

    public async Task<bool> UploadFile(string localFile, string remoteFile)
    {
        string relFile = ToRelativeFile(remoteFile);
        string dir = GetDirName(relFile);

        try
        {
            if (!client.DirectoryExists(dir))
            {
                client.CreateDirectory(dir, true);
            }

            var status = client.UploadFile(localFile, relFile, FtpRemoteExists.Overwrite);
            return status != FtpStatus.Failed;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        await Task.FromResult(true);
        return true;
    }

    // 获取目录
    // ftp://127.0.0.1/JiChuYiXue/Windows/xxx.bin -> ftp://127.0.0.1/JiChuYiXue/Windows
    string GetDirName(string file)
    {
        if (string.IsNullOrEmpty(file))
            throw new ArgumentNullException("file");

        int idx = file.LastIndexOf('/');
        if (idx >= 0)
            return file.Substring(0, idx);
        else
            return string.Empty;
    }

    // 将Uri 转换为相对路径
    // ftp://127.0.0.1/JiChuYiXue/Windows -> /JiChuYiXue/Windows
    string ToRelativeFile(string file)
    {
        var match = System.Text.RegularExpressions.Regex.Match(file, @"^(ftp|http)://[a-zA-Z0-9.]*:?\d*");
        if (match.Success)
        {
            return file.Substring(match.Length);
        }
        else
        {
            return file;
        }
    }
}