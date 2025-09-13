using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class RunTimeSaver
{
#if UNITY_EDITOR
    public static void CreateRuntimeDataAsset(List<CarData> carsData, List<PassengersData> passengersData, List<GaragesData> garagesData, Vector3 pos/*int carId, int carColorId, Vector3 posCar, Vector3 direction*/)
    {
        LevelsData data = ScriptableObject.CreateInstance<LevelsData>();

        data.carsData = carsData;
        data.passengersData = passengersData;
        data.garageData = garagesData;
        data.levelHolderPos = pos;

        // Choose or create the folder
        string folderPath = "Assets/_Game/Scripts/LevelScriptables";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // Create a unique file name
        string assetPath = $"{folderPath}/Level__{System.DateTime.Now:ss}.asset";

        // Save the asset
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
