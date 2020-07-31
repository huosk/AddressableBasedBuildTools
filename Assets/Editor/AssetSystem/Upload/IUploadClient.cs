using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public interface IUploadClient
{
    HostType Type { get; }
    string Host { get; set; }
    string UserName { get; set; }
    string Password { get; set; }
    Task Initialize();
    Task<bool> UploadFile(string localFile, string remoteFile);
    Task Release();
}

public enum HostType
{
    Unknown,
    Ftp,
    Http,
}

public struct UploadGroupInfo
{
    public string localPath;
    public string remotePath;
    public string[] files;
}