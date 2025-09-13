using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Reflection;
using FunGames.Core.Editor;


public class IntegrationManagerWindow : EditorWindow
{
    private static Dictionary<string, FGPackage> _packages = new Dictionary<string, FGPackage>();

    [MenuItem("FunGames/Setup Scene for Test CPI")]
    public static void Init()
    {
        GetAllPackages();
        foreach (FGPackage package in _packages.Values)
        {
            package.AddPrefabs();
            package.CreateSettingsAsset();
        }
    }

    private static void GetAllPackages()
    {
        List<FGPackage> packages = GetEnumerableOfType<FGPackage>();
        foreach (FGPackage package in packages)
        {
            if (String.IsNullOrEmpty(package.ModuleInfo.Name)) continue;
            if (!_packages.ContainsKey(package.ModuleInfo.Name)) _packages.Add(package.ModuleInfo.Name, package);
            if (_packages[package.ModuleInfo.Name] == null) _packages[package.ModuleInfo.Name] = package;
        }
    }

    private static List<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class //, IComparable<T>
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        }

        return objects;
    }
}