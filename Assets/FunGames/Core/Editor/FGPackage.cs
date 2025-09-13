using System.Collections.Generic;
using FunGames.Core.Modules;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor
{
    public abstract class FGPackage
    {
        public abstract FGModuleInfo ModuleInfo { get; }
        public abstract string JsonName { get; }
        public abstract string PackageName { get; }
        public abstract string ModuleFolder { get; }
        public abstract string[] AssetFolders { get; }
        public abstract string SettingsAsset { get; }
        public abstract string SettingsAssetName { get; }
        public abstract string[] ExternalAssets { get; }
        public abstract string DestinationPath { get; }
        public abstract string DeploymentFolder { get; }
        public abstract ExportPackageOptions ExportOptions { get; }
        public abstract List<GameObject> Prefabs { get; }
        public abstract string JsonFile { get; }

        public abstract void AddPrefabs();
        
        public abstract void CreateSettingsAsset();

        public abstract void UpdateSettings();
    }
}