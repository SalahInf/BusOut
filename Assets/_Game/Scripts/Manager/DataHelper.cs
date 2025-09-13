using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using FunGames.Analytics;
using UnityEngine;



public class DataHelper : MonoBehaviour
{
    public static DataHelper instance;
    public SoundManager SoundManager;
    public List<Car> cars;
    public List<Passenger> passengers;
    public List<Garage> garages;
    public List<Car> CarsInConvy = new List<Car>();
    public UIManager uiManager;
    public Material[] materialsCars;
    public Material[] materialsStickman;
    public Material materialHidenCar;
    public Color[] colorsBlinkOutline;
    public GameObject[] particlesFinishCar;
    public LevelHolder levelHolder;
    public PassengersHolder passengersHolder;
    public SplineComputer spline;
    public LevelSpowner levelSpowner;
    public Level currentLevel;
    public HapticsManager hapticsManager;
    public LevelSecunseTutorial levelSequnseTutorial;
    public float CarSpeed = 60f;
    public float CarConvySpeed = 60f;
    public float CarSlowSpeed = 10;
    [Range(0, 1)] public float smoothTurn = 0.1f;
    [Range(0, 1)] public float smoothAcceleration = 0.1f;
    [Range(0, 3)] public float scaleShakWhenParking = 0.1f;
    [Range(0, 1)] public float timeShakWhenParking = 0.1f;
    [Range(0, 5)] public float scalePassenger = 0.1f;
    [Range(0, 1)] public float timePassengerToGetInCar = 0.1f;
    [Range(0, 1)] public float timeShakePassenger = 0.1f;
    [Range(0, 3)] public float scalePassengerSeat = 0.1f;
    public int passengersInRows = 7;
    public int passengersInCols = 7;
    public int passengersInRowsMax = 7;
    public float scaleCarMultiplayer = 1f;
    public bool lose;
    public bool win;
    public bool pause;
    public bool isLevelSpowner = false;
    public bool GetFromFrontLigne;
    public bool isTurtorial;
    public int currentLevelIndex = 0;
    public int currentLevelTXT = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    public void Init()
    {
        lose = false;
        win = false;
        FGAnalytics.NewProgressionEvent(LevelStatus.Start, "level" + currentLevelTXT);
        Debug.Log ("Print level : " +currentLevelTXT);

    }
    public void Lose()
    {
        if (lose || win) return;
        FGAnalytics.NewProgressionEvent(LevelStatus.Fail, "level" + currentLevelTXT);
         Debug.Log("Print Lose : " + currentLevelTXT);
        levelHolder.isAutoFill = false;
        lose = true;
        StartCoroutine(StartLosWait());
    }

    public void Win()
    {
        if (lose || win) return;

        FGAnalytics.NewProgressionEvent(LevelStatus.Complete, "level" + currentLevelTXT);
        Debug.Log("Print Win : " + currentLevelTXT);
        levelHolder.isAutoFill = false;
        win = true;
        uiManager.ShowWin(true);
    }

    IEnumerator StartLosWait()
    {
        yield return new WaitForSeconds(0.8f);
        currentLevel.StopAllCars();
        uiManager.ShowLose(true);
    }
}
