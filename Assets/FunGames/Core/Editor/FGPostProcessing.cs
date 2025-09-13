#if UNITY_EDITOR
using System.Collections.Generic;
using FunGames.Core.Editor;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class FGPostProcessing : IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 1; }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        SetUpAndroidPermissions(report.summary.platform, string.Empty);
        UpdateAllSettings();
    }

    public static void SetUpAndroidPermissions(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.Android)
        {
#if UNITY_ANDROID
            AndroidManifestParser.Instance.AddAllPermissions();
#endif
        }
    }
    
    private void UpdateAllSettings()
    {
        List<FGPackage> packages = ProjectUtils.GetEnumerableOfType<FGPackage>();
        foreach (FGPackage package in packages)
        {
            package.UpdateSettings();
        }
    }
}
#endif