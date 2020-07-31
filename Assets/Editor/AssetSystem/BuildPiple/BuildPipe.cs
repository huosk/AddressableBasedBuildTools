using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class BuildPipe
{
    public bool ContinueWhenError = false;
    public event Action<bool> OnPipleComplete;

    List<IPipePhase> phases = new List<IPipePhase>();
    Queue<IPipePhase> processed = new Queue<IPipePhase>();

    public void AddPhase(IPipePhase phase)
    {
        if (phase == null)
            return;

        phases.Add(phase);
    }

    public void RemoveAll(IPipePhase phase)
    {
        if (phase == null)
            return;

        phases.RemoveAll(v => v == phase);
    }

    public void Remove(IPipePhase phase)
    {
        if (phase == null)
            return;

        phases.Remove(phase);
    }

    public async Task<bool> ProcessPiple(PipeContext context)
    {
        if (phases == null)
            return true;

        if (context == null)
            throw new ArgumentNullException("context");

        if (context.assets == null)
            return true;

        bool success = true;
        processed.Clear();

        for (int i = 0; i < phases.Count; i++)
        {
            var phase = phases[i];
            if (phase == null)
                continue;

            processed.Enqueue(phase);

            try
            {
                success &= await phase.Process(context);
            }











            catch (Exception e)
            {
                success = false;
                Debug.LogException(e);
            }

            if (!success && !ContinueWhenError)
            {
                Debug.LogErrorFormat("Failed::{0}", phase);
                break;
            }
        }

        while (processed.Count > 0)
        {
            IPipeCompleteCallback callback = processed.Dequeue() as IPipeCompleteCallback;
            if (callback != null)
            {
                callback.OnPipeComplete(success);
            }
        }

        OnPipleComplete?.Invoke(success);

        return success;
    }
}

public class PipeContext
{
    public BuildSystemSetting buildSetting;
    public AddressableAssetSettings setting;
    public AddressableAssetGroupTemplate groupTemplete;
    public List<AssetEntry> assets = new List<AssetEntry>();
    public string manifestFile = null;
}