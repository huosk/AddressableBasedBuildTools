using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class SaveManifestPhase : APipePhase
{
    public string OutputPath { get; set; }

    public override async Task<bool> Process(PipeContext context)
    {
        if (context == null)
            throw new System.ArgumentNullException("context");

        List<AssetEntry> assets = context.assets;
        if (assets == null)
            return false;

        AssetManifestFile manifestFile = new AssetManifestFile();
        manifestFile.assets = assets;

        string previewCatalogFile = Path.Combine(OutputPath, "manifest.json");
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        try
        {
            File.WriteAllText(previewCatalogFile, JsonConvert.SerializeObject(manifestFile, Formatting.Indented));
            context.manifestFile = previewCatalogFile;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        await Task.FromResult(true);
        return true;
    }
}
