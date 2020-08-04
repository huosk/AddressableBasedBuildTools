using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PreviewBuilder
{

    public static async Task<PreviewData[]> GetPreviews(string[] assetPaths)
    {
        List<PreviewData> previews = new List<PreviewData>();

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string assetPath = assetPaths[i];
            if (string.IsNullOrEmpty(assetPath))
                continue;

            PreviewData previewData = await GetPreview(assetPath);
            if (previewData == null)
                continue;

            previews.Add(previewData);
        }

        return previews.ToArray();
    }

    public static async Task<PreviewData> GetPreview(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return default;

        Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        if (obj == null)
            return default;

        //Selection.activeObject = obj;
        Texture2D tex = AssetPreview.GetAssetPreview(obj);
        int tryCount = 0;
        while ((tex == null || AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID())) && tryCount < 5)
        {
            tryCount++;
            await Task.Delay(100);
            tex = AssetPreview.GetAssetPreview(obj);
        }

        return new PreviewData()
        {
            texture = tex,
            assetPath = assetPath
        };
    }

    public static async Task SavePreviewTexture(string assetPath, string previewPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            throw new System.ArgumentNullException("assetPath");

        PreviewData previewData = await GetPreview(assetPath);
        if (previewData == null)
            return;

        if (previewData.texture == null)
            return;

        string dir = Path.GetDirectoryName(previewPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(previewPath, previewData.texture.EncodeToPNG());
    }
}

public class PreviewData
{
    public Texture2D texture;
    public string assetPath;
}