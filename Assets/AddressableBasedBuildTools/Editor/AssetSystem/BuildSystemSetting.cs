using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;


public class BuildSystemSetting : ScriptableObject
{
    public HostType UploadHostType
    {
        get
        {
            if (m_UploadHost.StartsWith("http") || m_UploadHost.StartsWith("https"))
                return HostType.Http;
            else if (m_UploadHost.StartsWith("ftp"))
                return HostType.Ftp;
            else
                return HostType.Unknown;
        }
    }

    public string UploadHost
    {
        get
        {
            var match = Regex.Match(m_UploadHost, @"^(http://|ftp://|https://)?(.*)");
            if (match.Success)
                return match.Groups[2].Value;
            else
                return string.Empty;
        }
    }

    public string UserName { get { return m_UserName; } }
    public string Password { get { return m_Password; } }
    public string BuildFolder { get { return m_BuildFolder; } }
    public EntryAddressType AssetKeyBuildType { get { return m_EntryAddressType; } }
    public bool ExportDll { get { return m_ExportDll; } }

#pragma warning disable CS0649
    [SerializeField] string m_UploadHost;
    [SerializeField] string m_UserName;
    [SerializeField] string m_Password;
    [SerializeField] string m_BuildFolder;
    [SerializeField] EntryAddressType m_EntryAddressType;
    [SerializeField] bool m_ExportDll;
#pragma warning restore
}

public enum EntryAddressType
{
    UseAssetPath,
    UseNameWithExtension,
    UseNameNoExtension,
    RelativeToBuildFolder
}
