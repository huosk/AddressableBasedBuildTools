using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading.Tasks;

public class HttpUploadClient : IUploadClient
{
    public HostType Type { get { return HostType.Http; } }
    public string Host { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }

    private WebClient client;

    public async Task Initialize()
    {
        client = new WebClient();
        client.Credentials = new NetworkCredential(UserName, Password);
        await Task.FromResult(true);
    }

    public async Task Release()
    {
        client.Dispose();
        await Task.FromResult(true);
    }

    public async Task<bool> UploadFile(string localFile, string remoteFile)
    {
        // TODO 在服务器确定处理接口后，下面的 recvUri 需要指向请求处理地址
        string recvUri = remoteFile;
        await client.UploadFileTaskAsync(recvUri, localFile);
        return true;
    }
}