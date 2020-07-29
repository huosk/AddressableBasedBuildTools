using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildPiple
{
    public bool ContinueWhenError = false;
    public event Action<bool> OnPipleComplete;

    List<IPiplePhase> phases = new List<IPiplePhase>();

    public void AddPhase(IPiplePhase phase)
    {
        if (phase == null)
            return;

        phases.Add(phase);
    }

    public void RemoveAll(IPiplePhase phase)
    {
        if (phase == null)
            return;

        phases.RemoveAll(v => v == phase);
    }

    public void Remove(IPiplePhase phase)
    {
        if (phase == null)
            return;

        phases.Remove(phase);
    }

    public async Task<bool> ProcessPiple(List<AssetEntry> assets)
    {
        if (phases == null)
            return true;

        if (assets == null)
            return true;

        bool success = true;

        for (int i = 0; i < phases.Count; i++)
        {
            var phase = phases[i];
            try
            {
                success &= await phase.Process(assets);
            }
            catch (Exception e)
            {
                success = false;
                Debug.LogException(e);
            }

            if (!success && !ContinueWhenError)
            {
                Debug.LogErrorFormat("Failed::{0}", nameof(phase));
                break;
            }
        }

        OnPipleComplete?.Invoke(success);

        return success;
    }
}