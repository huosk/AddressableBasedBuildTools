using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

public class GeneratePreviewPhase : APipePhase
{
    public string OutputPath { get; set; }
    
    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        List<AssetEntry> assets = context.assets;
        if (assets == null)
            return false;

        string previewOutputDir = Path.Combine(OutputPath, "preview").Replace('\\', '/');
        List<string> assetPaths = assets.Select(v => v.assetPath).ToList();

        int fullAssetCount = assets.Count();
        bool success = true;

        for (int j = 0; j < assets.Count; j++)
        {
            var entry = assets[j];
            if (entry == null)
                continue;

            string assetPath = entry.assetPath;

            try
            {
                string previewFile = await GeneratePreviewAtPath(OutputPath, entry.assetPath);
                entry.preview = previewFile;
                EditorUtility.DisplayProgressBar("生成预览图", assetPath, (float)j / fullAssetCount);
            }
            catch (System.Exception e)
            {
                success = false;
                Debug.LogException(e);
            }
        }

        EditorUtility.ClearProgressBar();

        return success;
    }

    static async Task<string> GeneratePreviewAtPath(string outputDir, string assetPath)
    {

        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string previewFile = Path.Combine(outputDir, fileName).Replace('\\', '/');
        previewFile = Path.ChangeExtension(previewFile, "png");

        try
        {
            await PreviewBuilder.SavePreviewTexture(assetPath, previewFile);

            // 缩略图有可能生成失败，只有成功才添加到列表
            if (!File.Exists(previewFile))
                previewFile = null;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        return previewFile;
    }
}
