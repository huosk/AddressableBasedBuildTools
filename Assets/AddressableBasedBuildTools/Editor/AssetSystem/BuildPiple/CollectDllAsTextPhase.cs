using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CollectDllAsTextPhase : APipePhase
{
    public override async Task<bool> Process(PipeContext context)
    {
        var input = DllToBuild.GetAssemblies();
        if (input == null || input.Count == 0)
            return true;

        var assemblies = input.Distinct();

        foreach (var assembly in assemblies)
        {
            if (assembly == null)
                continue;

            if (string.IsNullOrEmpty(assembly.Location))
                continue;

            string file = Path.GetFileName(assembly.Location);
            string newFile = Path.ChangeExtension(file, "bytes");

            string copyDir = context.buildSetting.BuildFolder;
            string destFile = copyDir + "/" + newFile;

            try
            {
                byte[] bytes = File.ReadAllBytes(assembly.Location);
                File.WriteAllBytes(destFile, bytes);
                AssetDatabase.ImportAsset(destFile, ImportAssetOptions.ForceUpdate);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            var obj = AssetDatabase.LoadAssetAtPath<TextAsset>(destFile);
            if (obj != null)
            {
                context.assets.Add(new AssetEntry()
                {
                    assetPath = AssetDatabase.GetAssetPath(obj),
                    type = AssetType.Text
                });
            }
        }

        await Task.FromResult(true);

        return true;
    }
}

public class DllToBuild
{
    public static List<Assembly> GetAssemblies()
    {
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var methods = assemblies.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .SelectMany(v => v.GetExportedTypes())
            .SelectMany(t =>
            {
                var ms = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                return ms.Where(m => Attribute.IsDefined(m, typeof(ExportAssemblyAttribute)));
            })
            .Where(m =>
            {
                Type tp = typeof(IEnumerable<Assembly>);
                bool b = !m.IsAbstract && !m.IsConstructor && (!m.IsGenericMethod || !m.ContainsGenericParameters);
                bool noArguments = m.GetParameters().Length == 0;
                return b && noArguments && tp.IsAssignableFrom(m.ReturnType);
            })
            .Distinct()
            .ToList();

        List<Assembly> list = new List<Assembly>();

        foreach (var method in methods)
        {
            var result = method.Invoke(null, null) as IEnumerable<Assembly>;
            if (result == null)
                continue;

            list.AddRange(result);
        }

        return list.Distinct().Where(v => !string.IsNullOrEmpty(v.Location)).ToList();
    }
}