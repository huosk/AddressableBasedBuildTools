using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class SaveManifestPhase : IPiplePhase
{
    public string OutputPath { get; set; }

    public async Task<bool> Process(List<AssetEntry> assets)
    {
        if (assets == null)
            throw new System.ArgumentNullException("assets");

        AssetManifestFile manifestFile = new AssetManifestFile();
        manifestFile.assets = assets;

        string previewCatalogFile = Path.Combine(OutputPath, "manifest.json");
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        File.WriteAllText(previewCatalogFile, JsonConvert.SerializeObject(manifestFile, Formatting.Indented));

        await Task.FromResult(true);
        return true;
    }
}
