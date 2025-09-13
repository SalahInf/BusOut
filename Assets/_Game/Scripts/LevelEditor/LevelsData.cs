using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class LevelsData : ScriptableObject
{
    public List<CarData> carsData;
    public List<PassengersData> passengersData;
    public List<GaragesData> garageData;
    public int passengersPerRows;
    public int passengersPerCols;
    public Vector3 levelHolderPos;
    public bool isTutorialLevel;
}

