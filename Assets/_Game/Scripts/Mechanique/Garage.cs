using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Garage : MonoBehaviour
{
    DataHelper _dataHelper => DataHelper.instance;
    public List<Car> allCarsOut = new List<Car>();
    public List<int> carIndex = new List<int>();
    public List<int> colorIndex = new List<int>();
    public bool isBusy;
    public int currentAvailableCars = 0;

    [SerializeField] Transform _targetPos;
    [SerializeField] Transform _spownPos;
    [SerializeField] TextMeshProUGUI _text;

    private void OnEnable()
    {
        //Car.OnCarMove += UseCar;
    }
    private void OnDisable()
    {
        //Car.OnCarMove -= UseCar;
    }
    public void Init()
    {
        currentAvailableCars = carIndex.Count;
        UseCar();
    }
    public void UseCar(bool isCleard = false)
    {
        if (carIndex.Count > 0)
        {
            if (!isCleard)
            {
                StartCoroutine(UseCarInum());
            }
            else
            {
                Car car = Instantiate(_dataHelper.cars[carIndex[0]], _spownPos.position, Quaternion.identity);
                car.currentGarage = this;
                car.SetColors(colorIndex[0]);
                car.transform.forward = _targetPos.right;
                allCarsOut.Add(car);
                carIndex.RemoveAt(0);
                colorIndex.RemoveAt(0);
                UpdateTextCounter();
                isBusy = true;

                car.isMoving = true;
                car.garageCar = true;
                car.transform.DOMove(_targetPos.position, isCleard ? 0.05f : 0.3f).OnComplete(() =>
                {
                    car.isMoving = false;
                    isBusy = false;
                });
            }

        }
    }

    IEnumerator UseCarInum()
    {
        yield return new WaitForSeconds(0.2f);
        if (carIndex.Count <= 0)
            yield break;

        Car car = Instantiate(_dataHelper.cars[carIndex[0]], _spownPos.position, Quaternion.identity);
        car.currentGarage = this;
        car.SetColors(colorIndex[0]);
        car.transform.forward = _targetPos.right;
        allCarsOut.Add(car);
        carIndex.RemoveAt(0);
        colorIndex.RemoveAt(0);
        UpdateTextCounter();
        isBusy = true;

        car.isMoving = true;
        car.garageCar = true;
        car.transform.DOMove(_targetPos.position, 0.3f).OnComplete(() =>
        {
            car.isMoving = false;
            isBusy = false;
        });

    }



    public void UpdateTextCounter()
    {
        _text.text = (carIndex.Count).ToString();
    }
    public void FillGarage((int, int) value)
    {
        carIndex.Add(value.Item1);
        colorIndex.Add(value.Item2);
        Car c = Instantiate(_dataHelper.cars[value.Item1]);
        c.transform.position = _spownPos.position;
        c.transform.parent = _dataHelper.currentLevel.carsHolder;
        c.garageCar = true;
        c.transform.forward = _targetPos.right;
        c.carPos = c.transform.localPosition;
        c.SetColors(value.Item2);
        c.carId = value.Item1;
        _dataHelper.currentLevel.cars.Add(c);
        UpdateTextCounter();

    }
}
