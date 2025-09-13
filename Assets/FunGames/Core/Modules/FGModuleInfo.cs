using System;
using System.Collections.Generic;

namespace FunGames.Core.Modules
{
    [Serializable]
    public class FGModuleInfo
    {
        public string Id = String.Empty;
        public string Name = String.Empty;
        public string Version = String.Empty;
        public string LogColor = "#FFFFFF";
        public string PackageUrl = String.Empty;
        public bool HasIssue = false;
        public bool IsDeprecated = false;
        public List<string> Dependencies = new List<string>();
        
        public void Set(FGModuleInfo moduleInfo)
        {
            Id = moduleInfo.Id;
            Name = moduleInfo.Name;
            Version = moduleInfo.Version;
            LogColor = moduleInfo.LogColor;
            PackageUrl = moduleInfo.PackageUrl;
            HasIssue = moduleInfo.HasIssue;
            IsDeprecated = moduleInfo.IsDeprecated;
            Dependencies = moduleInfo.Dependencies;
        }
    }
}