using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public class Spowner : MonoBehaviour
{

#if UNITY_EDITOR

    [SerializeField] Camera _cam;

    public List<CarData> carsData = new List<CarData>();
    public List<PassengersData> passengersData = new List<PassengersData>();
    public List<GaragesData> garageData = new List<GaragesData>();
    int currentCar => _selector.currentCar;
    [SerializeField] MenuEditor _menuEditor;
    [SerializeField] Selector _selector;
    [SerializeField] Transform _carsHolder;
    [SerializeField] Car[] _cars;
    [SerializeField] Garage _garage;
    [SerializeField] Passenger _passenger;
    [SerializeField] Transform _passengerHolder;

    [SerializeField] Parking _parkingLot;
    [SerializeField] Level _levelHolder;
    [SerializeField] LayerMask _clicable;
    RaycastHit hit;
    [SerializeField]
    int spownedStickMan = 0;
    [Range(0, 10), SerializeField] float spacing;
    [SerializeField] private int passengersPerRow = 4;
    public List<int> colorSpownedCount = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
    public bool isColoring = false;
    [SerializeField] LevelsData levelsData;


    void Update()
    {
        HoverMap();
    }

    void HoverMap()
    {
        bool hited = (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit, 10000, _clicable));
        Vector3 point = hit.point;
        _carsHolder.position = point;


        if (hit.collider == null)
            return;

        if (!isColoring)
        {
            _selector.gameObject.SetActive(hited);
            if (Input.GetMouseButtonDown(0))
            {
                CheckPoint();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DeleteCar();
        }
    }

    public void AttchCars()
    {
        foreach (var car in levelsData.carsData)
        {
            Car c = Instantiate(_cars[car.carId], _levelHolder.carsHolder);
            c.SetColors(car.carColorId);
            c.carId = car.carId;
            c.transform.localPosition = car.posCar;
            c.transform.localScale = car.localScale;
            c.transform.forward = car.direction;
            _levelHolder.cars.Add(c);
        }

        passengersData = levelsData.passengersData;
        //SpownPassengers
       
        foreach (var garagesData in levelsData.garageData)
        {
            Garage garage = Instantiate(_garage, _levelHolder.carsHolder);
            garage.transform.localPosition = garagesData.pos;
            garage.transform.forward = garagesData.dir;
            garage.transform.localScale = garagesData.localScale;
            _levelHolder.garages.Add(garage);
            _menuEditor.tunelFiller.AddDropDownValue(_levelHolder.garages.IndexOf(garage));
        }
    }

    void DeleteCar()
    {
        if (hit.collider.tag == "Car")
        {
            Car c = hit.collider.GetComponent<Car>();
            if (isColoring)
            {
                c.SetColors(_selector.currentMaterial);
            }
            else
            {
                _levelHolder.cars.Remove(c);
                _menuEditor.UpdateText(colorSpownedCount, c.carColorId);
                Destroy(c.gameObject);
            }
        }


        if (hit.collider.tag == "Garage")
        {
            Garage g = hit.collider.GetComponent<Garage>();
            _menuEditor.tunelFiller.RemoveDropDown(_levelHolder.garages.IndexOf(g));
            _levelHolder.garages.Remove(g);
            Destroy(g.gameObject);
        }
    }
    void CheckPoint()
    {

        if (hit.collider.tag == "Ground")
        {
            SpownObjects();
        }
        else
        {

        }

    }

    void SpownObjects()
    {
        if (_selector._currentSelectotr == 0)
        {
            Car c = Instantiate(_cars[currentCar], hit.point, Quaternion.Euler(0, _selector.targetAngle, 0), _levelHolder.carsHolder);
            c.SetColors(_selector.currentMaterial);
            _levelHolder.cars.Add(c);
            c.transform.localScale = Vector3.one * DataHelper.instance.scaleCarMultiplayer;
            _menuEditor.UpdateText(colorSpownedCount, c.carColorId);
            c.carId = currentCar;
            c.carPos = transform.localPosition;
        }
        else if (_selector._currentSelectotr == 1)
        {
            Garage g = Instantiate(_garage, hit.point + (Vector3.up * 3.5f), Quaternion.Euler(0, _selector.targetAngle, 0), _levelHolder.carsHolder);
            _levelHolder.garages.Add(g);
            g.transform.localScale = Vector3.one * DataHelper.instance.scaleCarMultiplayer;
            _menuEditor.tunelFiller.AddDropDownValue(_levelHolder.garages.IndexOf(g));

            //_menuEditor.UpdateText(ColorSpownedCount, c.carColorId);
            //c.carId = currentCar;

        }
    }

    bool CheckColorSpownedStickMan(int ColorIndex)
    {
        List<(int, int)> indexColor = _levelHolder.GetColorIndex();
        if (colorSpownedCount[ColorIndex] < indexColor[ColorIndex].Item2)
        {
            colorSpownedCount[indexColor[ColorIndex].Item1]++;
            return true;
        }
        return false;
    }
    public void SpownStickMan(int ColorIndex)
    {
        if (!CheckColorSpownedStickMan(ColorIndex))
            return;

        int row = spownedStickMan / passengersPerRow;
        int col = spownedStickMan % passengersPerRow;
        Vector3 localPos = new Vector3(col * spacing, 0f, -row * spacing);

        Passenger p = Instantiate(_passenger, _passengerHolder);
        _levelHolder.passengers.Add(p);
        p.passengerColorId = ColorIndex;
        p.transform.localPosition = localPos;

        p.SetColorId(ColorIndex);
        p.transform.localRotation = Quaternion.LookRotation(Vector3.forward); // Face forward from start
        spownedStickMan++;
        _menuEditor.UpdateText(colorSpownedCount, ColorIndex);


    }

    public void SaveLevel()
    {
        foreach (var item in _levelHolder.passengers)
        {
            PassengersData _passengersData = new PassengersData();
            _passengersData.passengerColorId = item.passengerColorId;
            _passengersData.direction = item.transform.forward;
            this.passengersData.Add(_passengersData);
        }

        foreach (var item in _levelHolder.cars)
        {
            if (!item.garageCar)
            {
                CarData _carsData = new CarData();
                _carsData.carId = item.carId;
                _carsData.carColorId = item.carColorId;
                _carsData.posCar = item.transform.localPosition;
                _carsData.direction = item.transform.forward;
                _carsData.localScale = item.transform.localScale;
                this.carsData.Add(_carsData);
            }
        }

        foreach (var item in _levelHolder.garages)
        {
            GaragesData _garageData = new GaragesData();
            _garageData.carIndex = item.carIndex;
            _garageData.colorIndex = item.colorIndex;
            _garageData.pos = item.transform.localPosition;
            _garageData.dir = item.transform.forward;
            _garageData.localScale = item.transform.localScale;
            this.garageData.Add(_garageData);
        }



        RunTimeSaver.CreateRuntimeDataAsset(carsData, passengersData, garageData, _levelHolder.centerPoint.transform.localPosition);
    }

    public void CreatePassengerGroups()
    {
        // Destroy old stickmen
        //foreach (var passenger in _levelHolder.passengers)
        //{
        //    Destroy(passenger.gameObject);
        //}
        //_levelHolder.passengers.Clear();
        //NewpassengersGroups.Clear();

        //spownedStickMan = 0;

        //// Spawn stickmen (dummy objects) for total car capacity
        ///
        _levelHolder.SetPassengersColors(_levelHolder.passengers);

        foreach (var item in _levelHolder.colorGroups)
        {
            for (int i = 0; i < item.groupSize; i++)
            {
                SpownStickMan(item.colorId);
            }
        }

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hit.point, 0.1f);
    }

    public void ClearPassengers()
    {
        foreach (var item in _levelHolder.passengers)
        {
            Destroy(item.gameObject);
        }
        colorSpownedCount = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        _levelHolder.passengers.Clear();
        spownedStickMan = 0;
        _menuEditor.UpdateText();
    }

    public void ColorChnager(bool state)
    {
        isColoring = state;
        _selector.gameObject.SetActive(!state);
    }
#endif
}

[Serializable]
public struct CarData
{
    public int carId;
    public int carColorId;
    public Vector3 posCar;
    public Vector3 localScale;
    public Vector3 direction;
    public bool isCarHiden;
}

[Serializable]
public struct PassengersData
{
    public int passnegrId;
    public int passengerColorId;
    public Vector3 direction;
}

[Serializable]
public struct GaragesData
{
    public int garageId;
    public List<int> carIndex;
    public List<int> colorIndex;
    public Vector3 pos;
    public Vector3 dir;
    public Vector3 localScale;
}
