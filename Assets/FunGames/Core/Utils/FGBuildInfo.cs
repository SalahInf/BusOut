using System;
using FunGames.Core.Utils;
using FunGames.Tools.Utils;
using UnityEngine;

public class FGBuildInfo : ScriptableObject
{
    public const string AssetName = "FGBuildInfo.asset";

    [HideInInspector] public bool _testEnvironment;

    private static FGBuildInfo _instance;

    private static object _lock = new object();

    public static FGBuildInfo Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = LoadBuildInfo();
                    }
                }
            }

            return _instance;
        }
    }

    private static FGBuildInfo LoadBuildInfo()
    {
        try
        {
            return Resources.Load<FGBuildInfo>(FGPath.FUNGAMES + "/" + "FGBuildInfo");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return CreateInstance<FGBuildInfo>();
        }
    }

    public bool IsTestEnvironment()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_ANDROID
        if (CurrentPlatform.IsFireOS) return false;
        return _testEnvironment;
#elif UNITY_IOS
        switch (Application.installMode)
        {
            case ApplicationInstallMode.DeveloperBuild:
            case ApplicationInstallMode.Adhoc:
            case ApplicationInstallMode.Editor:
                return true;
        }

        return false;
#endif
    }
}