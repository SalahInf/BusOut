using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using DG.Tweening;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;

public class LevelHolder : MonoBehaviour
{
    Camera _cam => Camera.main;
    DataHelper _dataHelper => DataHelper.instance;
    public List<SplineComputer> parkings;
    public SplineComputer conveySpline;
    public SplineComputer outSpline;
    public SplineComputer[] convyEntrence;
    public List<Car> parkcardIndex = new List<Car>();
    [SerializeField] SpriteRenderer[] _parkingsSprites;
    public List<Transform> nodesParkings = new List<Transform>();
    [SerializeField] LayerMask _clicable;
    [SerializeField] GameObject _clickParticle;
    bool autofillChecked = false;
    //[SerializeField] SpriteRenderer[] spotes;
    public List<Car> carsMoving = new List<Car>();
    RaycastHit hit;
    public static event Action OnHideOf;
    public bool isAutoFill = false;

    public void Init()
    {
        //parkIndex.Clear();
        parkcardIndex.Clear();
        autofillChecked = false;
        for (int i = 0; i < parkings.Count; i++)
        {
            //parkIndex.Add(false);
            parkcardIndex.Add(null);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_dataHelper.lose || _dataHelper.win || _dataHelper.pause)
                return;

            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit, 10000, _clicable))
            {
                Car car = hit.collider.GetComponent<Car>();


                if (car.ReadyToInput && !car.isParking && AddCarToParking(car, true) >= 0)
                {

                    if (!car.isMoving)
                    {
                        car.MoveCar();
                        GameObject g = Instantiate(_clickParticle, hit.point, Quaternion.identity);
                        Destroy(g, 0.6f);

                        if (car.isRoadEmpty)
                            OnHideOf?.Invoke();
                    }
                }
                else if (!car.isParking)
                {
                    DOTween.Kill(606);
                    car.transform.DOShakePosition(0.1f, .5f).SetEase(Ease.InBounce).SetId(606);
                    car.transform.DOShakeScale(0.1f, .5f).SetEase(Ease.InBounce).SetId(606);
                }


                if (car.isParking && car.ReadyToInput)
                {
                    car.MoveFromParking();

                    if (_dataHelper.isTurtorial)
                    {
                        _dataHelper.levelSequnseTutorial.ActivatePointerBleu(false, 0);
                    }
                    //SlowDown();
                }
            }
        }
        CkeckSlowDonw();
    }

    //public int IsParkingAvailable()
    //{
    //    //_maxParkingSlots = Mathf.Clamp(_maxParkingSlots, 0, parkings.Count);

    //    for (int i = 0; i < _maxParkingSlots; i++)
    //    {
    //        if (parkcardIndex[i] == null)
    //        {
    //            return i;
    //        }
    //    }
    //    return -1;
    //}

    public int AddCarToParking(Car car, bool ischeking = false)
    {

        for (int i = 0; i < parkcardIndex.Count; i++)
        {
            if (parkcardIndex[i] == car)
            {
                parkcardIndex[i] = null;
            }
            if (parkcardIndex[i] == null)
            {
                if (!ischeking)
                {
                    parkcardIndex[i] = car;
                    car.currentPark = i;
                }
                return i;
            }
        }
        return -1;
    }

    void BlinkSpots(int index)
    {
        _parkingsSprites[index].gameObject.SetActive(true);

        _parkingsSprites[index].DOFade(0f, 0.5f)
           .SetLoops(5, LoopType.Yoyo)
           .SetEase(Ease.InOutSine).OnComplete(() =>
           {
               _parkingsSprites[index].DOFade(1, 0.01f);
               _parkingsSprites[index].gameObject.SetActive(false);
           });
    }

    public void CheckOnlyOneSpotLeft()
    {
        if (isAutoFill || _dataHelper.lose || _dataHelper.win)
            return;

        bool hasExactlyOneNull = parkcardIndex.Count(c => c == null) == 1;
        bool hasExactlyzeronull = parkcardIndex.Count(c => c == null) == 0;

        if (hasExactlyzeronull)
        {
            _dataHelper.uiManager.ShowOneLeftSpotLeftUi(false);
        }

        if (hasExactlyOneNull)
        {
            int nullIndex = parkcardIndex.FindIndex(c => c == null);
            BlinkSpots(nullIndex);
            _dataHelper.uiManager.ShowOneLeftSpotLeftUi(true);
        }
    }

    public void CheckLose()
    {
        for (int i = 0; i < parkcardIndex.Count; i++)
        {
            if (parkcardIndex[i] == null)
                return;
            int passengersCount = Mathf.Min(_dataHelper.passengersHolder.passengersPerRow, _dataHelper.passengersHolder.passengersGroups.Count);

            for (int j = 0; j < passengersCount; j++)
            {
                if (_dataHelper.passengersHolder.passengersGroups[j].Count == 0) continue;

                if (_dataHelper.passengersHolder.passengersGroups[j][0].passengerColorId == parkcardIndex[i].carColorId)
                {
                    return;
                }
            }

        }

        // lose case 
        DataHelper.instance.Lose();
    }

    public float minDistance = 0.1f;
    public float smoothTime = 0.01f;

    void CkeckSlowDonw()
    {
        if (_dataHelper.CarsInConvy == null)
            return;


        foreach (var car in _dataHelper.CarsInConvy)
        {
            bool tooClose = false;

            foreach (var otherCar in _dataHelper.CarsInConvy)
            {
                if (car == otherCar) continue;

                float distance = GetSplineDistance(car, otherCar);

                if (distance > 0 && distance < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            float targetSpeed = tooClose ? 0f : _dataHelper.CarConvySpeed;
            car.follower.followSpeed = Mathf.SmoothDamp(
                car.follower.followSpeed,
                targetSpeed,
                ref car.speedVelocity,
                smoothTime
            );
        }
    }
    float GetSplineDistance(Car carA, Car carB)
    {
        float a = (float)carA.follower.GetPercent();
        float b = (float)carB.follower.GetPercent();

        float d = ((b - a + 1f) % 1f);  // forward distance
        float backD = ((a - b + 1f) % 1f); // backward distance

        if (carA.follower.direction == Spline.Direction.Forward)
            return d;
        else
            return backD;
    }

    public void AutoFill()
    {
        if (isAutoFill)
            return;
        StartCoroutine(AutoClear());
    }

    IEnumerator AutoClear()
    {
        for (int i = 0; i < parkcardIndex.Count; i++)
        {
            Car car = parkcardIndex[i];

            if (car != null && car.isParkecdInParking)
            {
                car.MoveFromParking();
                car.ReadyToInput = false;
                _dataHelper.currentLevel.cars.Remove(car);
                yield return new WaitForSeconds(0.2f);
            }
        }

        isAutoFill = true;
        foreach (var car in _dataHelper.currentLevel.cars)
        {
            if (car != null && !car.onpath && !car.isMoving && !car.isParkecdInParking)
            {
                //car.ReadyToInput = false;
                car.MoveCar();
                car.canGoToConvyDirectly = true;
                yield return new WaitForSeconds(0.3f);
            }
        }


        foreach (var item in _dataHelper.currentLevel.garages)
        {
            int count = item.carIndex.Count;
            for (int i = 0; i < count; i++)
            {
                item.UseCar(true);
            }
            int c = item.allCarsOut.Count;
            for (int i = 0; i < c; i++)
            {
                item.allCarsOut[i].canGoToConvyDirectly = true;
                item.allCarsOut[i].MoveCar();
                yield return new WaitForSeconds(0.3f);
            }
        }
    }


    public void CheckAutoFill()
    {
        if (_dataHelper.currentLevelIndex <= 1 || autofillChecked)
            return;

        int inedex = 0;
        foreach (var car in _dataHelper.currentLevel.cars)
        {
            if (car != null && !car.isCarFull)
            {
                inedex++;
            }
        }

        foreach (var item in _dataHelper.currentLevel.garages)
        {
            inedex += item.currentAvailableCars ;
        }


        if (inedex <= 5)
        {
            _dataHelper.uiManager.ShowAutoFill(true);
            autofillChecked = true;
        }
    }
}
