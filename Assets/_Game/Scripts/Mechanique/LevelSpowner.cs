using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using static Dreamteck.Splines.SplineSampleModifier;

public class LevelSpowner : MonoBehaviour
{
    DataHelper _dataHelper => DataHelper.instance;
    [SerializeField] LevelsData[] levelsScriptables;
    [SerializeField] LevelsData levelsScriptableTutorial;
    [SerializeField] Transform _spawnPoint;
    [SerializeField, Range(0, 1)] float multiplayerScale;
    [SerializeField] Level level;
    [SerializeField] TextMeshProUGUI[] _levelCounterText;
    [SerializeField] LevelsData _currentLevel;
    List<PassengersData> passengersData;

    private void Start()
    {
        Init();
        Application.targetFrameRate = 120;
    }

    public void Init()
    {
        StopAllCoroutines();
        _dataHelper.currentLevel.PassengersCount = 0;
        _dataHelper.uiManager.ShowGameplayUI(true);

        bool haskey = PlayerPrefs.HasKey("Level");


        if (!haskey)
        {
            _dataHelper.currentLevelTXT = 1;
            _currentLevel = levelsScriptableTutorial;
            _dataHelper.isTurtorial = true;
            _dataHelper.currentLevelIndex = PlayerPrefs.GetInt("Level", 0);
            _dataHelper.currentLevelTXT = PlayerPrefs.GetInt("LevelTXT", 1);
        }
        else
        {
            _dataHelper.currentLevelIndex = PlayerPrefs.GetInt("Level", 0);
            _dataHelper.currentLevelTXT = PlayerPrefs.GetInt("LevelTXT", 1);
            _currentLevel = levelsScriptables[_dataHelper.currentLevelIndex];
        }



        Invoke(nameof(UpdateCounterUiText), 0.2f);
        foreach (var item in level.cars)
        {
            foreach (var item1 in item.passengrsInCar)
            {
                if (item1 != null)
                    Destroy(item1.gameObject);
            }
            if (item != null)
                Destroy(item.gameObject);
        }

        foreach (var item in _dataHelper.passengersHolder.passengerQueue)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        foreach (var item in level.garages)
        {
            foreach (var cars in item.allCarsOut)
            {
                if (cars != null)
                    Destroy(cars.gameObject);
            }
            item.allCarsOut.Clear();
            if (item != null)
                Destroy(item.gameObject);
        }

        level.cars.Clear();
        _dataHelper.CarsInConvy.Clear();
        level.garages.Clear();
        _dataHelper.passengersHolder.passengerQueue.Clear();
        _dataHelper.levelHolder.Init();
        _dataHelper.Init();

        SpownLevelScriptable();
        _dataHelper.passengersHolder.AddPassengersToRows();
        _dataHelper.passengersHolder.sliderPaasengers.fillAmount = 0;


        if (_dataHelper.isTurtorial)
        {
            _dataHelper.levelSequnseTutorial.Init(level.cars[1], level.cars[0]);
            //_dataHelper.uiManager.ShowTutorialUI(true);
        }
    }


    void UpdateCounterUiText()
    {
        for (int i = 0; i < _levelCounterText.Length; i++)
        {
            _levelCounterText[i].text = $"LEVEL {_dataHelper.currentLevelTXT}";
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextLevel();
        }
    }

    void SpownLevelScriptable()
    {
        //int levelIndex = _dataHelper._currentLevelIndex;
        _dataHelper.passengersInRows = _currentLevel.passengersPerRows;
        _dataHelper.passengersInCols = _currentLevel.passengersPerCols;
        transform.position = _currentLevel.levelHolderPos;
        //SpownCars
        foreach (var carData in _currentLevel.carsData)
        {
            Car car = Instantiate(_dataHelper.cars[carData.carId], level.transform);
            car.isCarHiden = carData.isCarHiden;
            car.transform.localPosition = carData.posCar;
            car.transform.forward = carData.direction;
            car.transform.localScale = carData.localScale;
            car.SetColors(carData.carColorId);
            level.cars.Add(car);
        }

        passengersData = _currentLevel.passengersData;
        //SpownPassengers
        foreach (var passengersData in passengersData)
        {
            Passenger passenger = Instantiate(_dataHelper.passengers[0], _dataHelper.passengersHolder.transform);
            passenger.transform.localPosition = new Vector3(0, 0, -70);
            passenger.passengerColorId = passengersData.passengerColorId;
            passenger.SetColorId(passengersData.passengerColorId);
            passenger.startColor = DataHelper.instance.materialsStickman[passenger.passengerColorId].GetColor("_OutlineColor");
            passenger.gameObject.SetActive(false);
            _dataHelper.currentLevel.PassengersCount++;
            _dataHelper.passengersHolder.passengerQueue.Add(passenger);
        }
        foreach (var garagesData in _currentLevel.garageData)
        {
            Garage garage = Instantiate(_dataHelper.garages[0], level.transform);
            garage.carIndex = new List<int>(garagesData.carIndex);
            garage.colorIndex = new List<int>(garagesData.colorIndex);
            garage.transform.localPosition = garagesData.pos;
            garage.transform.forward = garagesData.dir;
            garage.transform.localScale = garagesData.localScale;
            level.garages.Add(garage);
            garage.Init();

        }


        _dataHelper.passengersHolder.ReAlignQueue();
        _dataHelper.passengersHolder.passengersCount = _dataHelper.passengersHolder.passengerQueue.Count;
        _dataHelper.passengersHolder.UpdatePassengerCounter();
    }
    public void NextLevel()
    {
        if (_dataHelper.currentLevelIndex < levelsScriptables.Length - 1)
        {
            _dataHelper.currentLevelTXT++;
            if (!_currentLevel.isTutorialLevel)
            {
                _dataHelper.currentLevelIndex++;
            }
            else
            {
                _dataHelper.isTurtorial = false;
                Destroy(_dataHelper.levelSequnseTutorial.gameObject);
            }

            PlayerPrefs.SetInt("Level", _dataHelper.currentLevelIndex);
            PlayerPrefs.SetInt("LevelTXT", _dataHelper.currentLevelTXT);
        }
        else
        {
            _dataHelper.currentLevelIndex = 9/* Random.Range(2, levelsScriptables.Length - 1)*/;
            PlayerPrefs.SetInt("Level", _dataHelper.currentLevelIndex);
            PlayerPrefs.SetInt("LevelTXT", _dataHelper.currentLevelTXT);
        }
        Init();

    }
    public void RetryLevel()
    {
        Init();
    }
}
