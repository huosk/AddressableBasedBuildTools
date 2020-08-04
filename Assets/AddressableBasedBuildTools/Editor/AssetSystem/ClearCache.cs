using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

public class ClearCache
{
    const string Menu_OpenCacheFolder = "Tools/清除缓存/浏览缓存目录";
    const string Menu_ClearAll = "Tools/清除缓存/清除所有";
    const string Menu_ClearCache = "Tools/清除缓存/清除资源包";
    const string Menu_ClearCatalog = "Tools/清除缓存/清除 Catalog";

    [MenuItem(Menu_OpenCacheFolder, priority = 1900)]
    static void OpenCacheFolder()
    {
        string cacheFolder = Application.persistentDataPath + "/";
        if (!Directory.Exists(cacheFolder))
            Directory.CreateDirectory(cacheFolder);

        EditorUtility.RevealInFinder(cacheFolder);
    }

    [MenuItem(Menu_ClearAll, priority = 2000)]
    static async void ClearAllCache()
    {
        try
        {
            ClearBundleCacheInternal();
            await ClearCatalogInternal();
            UnityEngine.Debug.Log("缓存清理完毕");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    [MenuItem(Menu_ClearCache, priority = 2001)]
    static void ClearBundleCache()
    {
        ClearBundleCacheInternal();
        UnityEngine.Debug.Log("资源包缓存清理完毕");
    }

    [MenuItem(Menu_ClearCatalog, priority = 2001)]
    static async void ClearCatalog()
    {
        try
        {
            var exitCode = await ClearCatalogInternal();
            UnityEngine.Debug.LogFormat("Catalog 清理完毕.{0}", exitCode);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    internal static void ClearBundleCacheInternal()
    {
        Caching.ClearCache();
    }

    internal static async Task<int> ClearCatalogInternal()
    {
        string catalogCacheDir = (Application.persistentDataPath + "/com.unity.addressables").Replace('/', '\\');
        if (!Directory.Exists(catalogCacheDir))
            return 0;

        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.CreateNoWindow = true;

        p.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            UnityEngine.Debug.LogError(e.Data);
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        p.StandardInput.WriteLine("rd /s /q \"" + catalogCacheDir + "\"");
        p.StandardInput.Close();

        int exitCode = await Task.Run(() =>
        {
            p.WaitForExit();
            return p.ExitCode;
        });

        return exitCode;
    }
}
