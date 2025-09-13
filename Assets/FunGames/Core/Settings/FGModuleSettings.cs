using FunGames.Core.Modules;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Settings
{
    public abstract class FGModuleSettings : ScriptableObject, IFGModuleSettings
    {
        [HideInInspector] public FGModuleInfo moduleInfo;
        [HideInInspector] public bool logEnabled = true;

        public Color LogColor = Color.white;

        public bool LogEnabled
        {
            get => logEnabled;
            set => logEnabled = value;
        }

        public Color Color => LogColor;

        public FGModuleInfo ModuleInfo
        {
            get => moduleInfo;
            set
            {
                moduleInfo = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }
    }
}