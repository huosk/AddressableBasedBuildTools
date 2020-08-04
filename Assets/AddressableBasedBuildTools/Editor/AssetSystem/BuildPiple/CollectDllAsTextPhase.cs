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
        var input = DllToBuild.GetAssembliesToExport();
        if (input == null || input.Count == 0)
            return true;

        var assemblies = input.Distinct();

        foreach (var assembly in assemblies)
        {
            string outputDir = context.buildSetting.BuildFolder;
            var obj = SaveDllAsText(assembly, outputDir);
            if (obj != null)
            {
                /*
                 * 这里采用 ScriptableObject 来存储 Dll 及其引用数据，这样做的优势是简化加载时的依赖处理，
                 * 当加载 AssemblyData 的时候，Addressable 会自动加载依赖；
                 * 
                 * 但是采用 ScriptableObject 在跨工程时，需要特别注意，脚本不能直接在外部拷贝，必须通过 
                 * UnityPackage 或者 PackageManager 的方式加载到工程当中，否则无法解析对象。
                 */
                var ad = ScriptableObject.CreateInstance<AssemblyData>();
                string adFile = outputDir + "/" + Path.GetFileNameWithoutExtension(assembly.Location) + "_info.asset";
                AssetDatabase.CreateAsset(ad, adFile);
                ad = AssetDatabase.LoadAssetAtPath<AssemblyData>(adFile);
                ad.mainDll = obj;
                EditorUtility.SetDirty(ad);

                context.assets.Add(new AssetEntry()
                {
                    assetPath = AssetDatabase.GetAssetPath(obj),
                    type = AssetType.Text
                });

                context.assets.Add(new AssetEntry()
                {
                    assetPath = AssetDatabase.GetAssetPath(ad),
                    type = AssetType.Custom
                });
            }
        }

        await Task.FromResult(true);

        return true;
    }

    private static TextAsset SaveDllAsText(Assembly assembly, string outputDir)
    {
        if (assembly == null)
            return null;

        if (string.IsNullOrEmpty(assembly.Location))
            return null;

        string file = Path.GetFileName(assembly.Location);
        string newFile = Path.ChangeExtension(file, "bytes");

        string copyDir = outputDir;
        string destFile = copyDir + "/" + newFile;

        return SaveFileAsText(assembly.Location, destFile);
    }

    private static TextAsset SaveFileAsText(string dllFile, string destFile)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(dllFile);
            File.WriteAllBytes(destFile, bytes);
            AssetDatabase.ImportAsset(destFile, ImportAssetOptions.ForceUpdate);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        var obj = AssetDatabase.LoadAssetAtPath<TextAsset>(destFile);
        return obj;
    }
}

public class DllToBuild
{
    public static List<Assembly> GetAssembliesToExport()
    {
        return GetAssemblies<ExportAssemblyAttribute>();
    }

    private static IEnumerable<MethodInfo> GetMethodsWithAttribute<T>()
        where T : System.Attribute
    {
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var methods = assemblies.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .SelectMany(v => v.GetExportedTypes())
            .SelectMany(t =>
            {
                var ms = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                return ms.Where(m => Attribute.IsDefined(m, typeof(T)));
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

        return methods;
    }

    private static List<Assembly> GetAssemblies<T>()
        where T : System.Attribute
    {
        var methods = GetMethodsWithAttribute<T>();
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