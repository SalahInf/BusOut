using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Car : MonoBehaviour
{
    #region === References ===
    DataHelper _dataHelper => DataHelper.instance;
    LevelHolder _levelHolder => DataHelper.instance.levelHolder;
    SplineComputer spline => DataHelper.instance.spline;
    PassengersHolder _PassengerHolder => DataHelper.instance.passengersHolder;
    float _carSpeed => _dataHelper.CarSpeed;
    float _convySpeed => _dataHelper.CarConvySpeed;
    public List<Passenger> passengrsInCar = new List<Passenger>();
    #endregion

    #region === Car Appearance & Configuration ===
    public int carPassengers = 0;
    public Garage currentGarage;
    public MeshRenderer carMesh;
    public MeshRenderer topCarMesh;
    public Collider _collider;
    public List<Transform> carSeats;
    public int carColorId;
    public int carId;
    public float carPercenet;
    public Vector3 carPos;
    #endregion

    #region === Car State ===
    public int carCurrentPassengers = 0;
    public bool isCarHiden = false;
    public bool isMoving = false;
    public bool garageCar = false;
    public bool isRoadEmpty = true;
    public bool isParkedInConvey = false;
    public bool isParking = false;
    public bool ReadyToInput = true;
    public bool isCarFull = false;
    public bool isOnconvy = false;
    public bool isReadyToPickUp = false;
    public bool canPickup;
    public int currentPark = -1;
    public bool onpath;
    public bool canGoToConvyDirectly = false;
    public bool isAutoFinished = false;
    public bool isParkecdInParking = false;
    bool isCarMoved = false;
    bool isRoadFree = false;
    bool isCrashed = false;
    int freeCarSeat;
    Vector3 startPos;
    RaycastHit hit;
    [SerializeField] bool onSpline = false;
    [SerializeField] bool _isSpownLevel = false;

    Coroutine _moveCoroutine;
    // this is used only for tutorial
    public GameObject carPointerTutorial;
    #endregion

    #region === Spline & Movement Settings ===
    [Header("Spline Settings")]
    public SplineFollower follower;
    [SerializeField] LayerMask _clicable;
    [SerializeField] LayerMask _Walls;

    [Header("Car Settings")]
    [SerializeField] GameObject _crachParticle;
    [SerializeField] GameObject _seatsHolder;
    [SerializeField] Transform _posPoint;
    [SerializeField] Transform[] _pathCheckers;
    [SerializeField] Rigidbody _rb;
    [SerializeField] float _parkingScale;
    [HideInInspector] public float speedVelocity;
    bool _IsreadyParkedinPark = false;
    #region === Unity Methods ===
    private void Start()
    {

        if (_dataHelper.isLevelSpowner)
            return;

        _rb = GetComponent<Rigidbody>();
        if (_isSpownLevel) return;

        follower.onNode += ChangeLane;
        carCurrentPassengers = 0;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SplineTrigger") && isMoving)
        {
            if (onSpline || isCarFull || !isRoadEmpty) return;
            StartCoroutine(AssignePath());
        }

        if (!_levelHolder.isAutoFill)
            if (other.CompareTag("Car"))
            {
                if (!other.GetComponent<Car>().onSpline)
                {
                    if (!isMoving || onSpline || isCrashed || isCarMoved || isParkedInConvey) return;
                    _dataHelper.SoundManager.StopSound(2);
                    _dataHelper.SoundManager.PlaySound(5);
                    GameObject g = Instantiate(_crachParticle, _posPoint.position, Quaternion.identity);
                    Destroy(g, 0.6f);
                    isCrashed = true;
                    _rb.isKinematic = true;
                    DOTween.Kill(this);
                    other.transform.DOShakePosition(0.2f, 0.6f, 1).SetEase(Ease.InBounce).SetId(this);
                    other.transform.DOShakeScale(0.08f, 0.2f, 1).SetEase(Ease.InBounce).SetId(this);
                    transform.DOMove(startPos, 0.3f).OnComplete(() =>
                    {
                        isMoving = false;
                        isCrashed = false;
                    }).SetId(this);
                }
            }
    }
    private void OnEnable()
    {
        LevelHolder.OnHideOf += ShowHidenColor;

    }
    private void OnDisable()
    {
        LevelHolder.OnHideOf -= ShowHidenColor;
    }
    #endregion

    #region === Public Methods ===

    public void SetColors(int matIndex)
    {
        if (isCarHiden)
        {
            carMesh.material = topCarMesh.material = _dataHelper.materialHidenCar;
            topCarMesh.transform.GetChild(1).gameObject.SetActive(true);
            topCarMesh.transform.GetChild(0).gameObject.SetActive(false);

        }
        else
        {
            carMesh.material = topCarMesh.material = _dataHelper.materialsCars[matIndex];
            topCarMesh.transform.GetChild(1).gameObject.SetActive(false);
            carColorId = matIndex;
        }
    }

    public void MoveCar()
    {
        if (isMoving || _dataHelper.lose || _dataHelper.win) return;

        startPos = transform.position;
        _dataHelper.SoundManager.PlaySound(2);
        isRoadEmpty = _levelHolder.isAutoFill ? true : IsPathFree();

        isMoving = true;

        if (isRoadEmpty)
        {
            if (DataHelper.instance.isTurtorial)
            {
                if (this.carColorId == 0)
                {
                    DataHelper.instance.levelSequnseTutorial.ActivatePointerBleu(false, 1);
                }
                else
                {
                    DataHelper.instance.levelSequnseTutorial.ActivatePointerYelow(false, 1);
                }
            }

            if (!_levelHolder.isAutoFill)
                canGoToConvyDirectly = _PassengerHolder.CheckLineIfHasColor(this);

            if (canGoToConvyDirectly == false)
            {
                _levelHolder.AddCarToParking(this);
            }

            if (garageCar && !_levelHolder.isAutoFill)
            {
                currentGarage.UseCar();
            }
        }
        //if (currentPark == -10)
        //{
        //    canGoToConvyDirectly = true;
        //}

        StartCoroutine(MoveForword());
    }

    public void MoveFromParking()
    {
        if (currentPark < 0)
            return;
        isParkecdInParking = false;
        _IsreadyParkedinPark = true;
        isParking = false;
        MoveSmooth(_carSpeed);
        _dataHelper.SoundManager.PlaySound(2);

        _levelHolder.parkcardIndex[currentPark] = null;
        currentPark = -10;

    }
    public bool TryPickupPassenger(Passenger passenger)
    {

        if (carSeats == null || _dataHelper.lose || _dataHelper.win) return false;


        //if (!isAutoFinished)
        if (!canPickup || passenger.passengerColorId != carColorId) return false;

        if (carCurrentPassengers == carPassengers)
        {
            isCarFull = true;
            return false;
        }

        passengrsInCar.Add(passenger);
        passenger = passengrsInCar[^1];
        passenger.gameObject.SetActive(true);
        passenger.MoveToSeat(carSeats[freeCarSeat], transform, _dataHelper.timePassengerToGetInCar);
        freeCarSeat++;
        carCurrentPassengers++;

        if (carCurrentPassengers == carPassengers)
        {
            isCarFull = true;
            _levelHolder.CheckAutoFill();
            float startY = transform.localPosition.y;
            topCarMesh.transform.DOScale(1, 0.3f).SetEase(Ease.InBack).OnComplete(() => _seatsHolder.gameObject.SetActive(false));
            transform.DOLocalMoveY(startY + 8f, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                transform.DOLocalMoveY(startY, 0.1f).SetEase(Ease.OutCubic);
                GameObject p = Instantiate(_dataHelper.particlesFinishCar[carColorId], transform.position + Vector3.up * 4, Quaternion.identity);
                Destroy(p, 0.6f);
                foreach (var item in passengrsInCar)
                {
                    Destroy(item.gameObject, 0.1f);
                }
                Destroy(_seatsHolder.gameObject, 0.5f);
            });
            if (garageCar)
                currentGarage.currentAvailableCars--;
            _dataHelper.hapticsManager.PlayHaptic(2);
            _dataHelper.SoundManager.PlaySound(3);
        }

        return true;
    }

    public bool IsPathFree()
    {
        foreach (var item in _pathCheckers)
        {
            isRoadFree = !Physics.Raycast(item.position, transform.forward, out hit, 30f, _clicable);
            Car car = hit.collider?.GetComponent<Car>();
            if (car != null)
            {
                isRoadFree = car.isMoving;
                break;
            }

            if (!isRoadFree && !car.onpath) break;
        }
        return isRoadFree;
    }
    public void LoseCase()
    {
        _dataHelper.Lose();
        StopCoroutine(_moveCoroutine);
        follower.followSpeed = 0;
    }

    #endregion

    #region === Private Methods ===

    void ShowHidenColor()
    {
        if (isCarHiden)
        {
            if (IsPathFree())
            {
                isCarHiden = false;
                carMesh.material = topCarMesh.material = _dataHelper.materialsCars[carColorId];
                topCarMesh.transform.GetChild(0).gameObject.SetActive(true);
                topCarMesh.transform.GetChild(1).gameObject.SetActive(false);
                topCarMesh.transform.localScale = Vector3.zero;
                topCarMesh.transform.DOScale(1, .1f);
                transform.DOShakeScale(0.1f, 0.5f).SetEase(Ease.OutBack);
            }
        }
    }
    void ChangeLane(List<SplineTracer.NodeConnection> nodes)
    {
        if (!isParking)
            StartCoroutine(LaneTurne(nodes));
    }

    void AddListenrTriger(SplineComputer spline, int trigerIndex)
    {
        TriggerGroup triggerGroup = spline.triggerGroups[0];
        SplineTrigger trigger = triggerGroup.triggers[trigerIndex];

        UnityAction<SplineUser> callback = null;
        callback = ((SplineUser user) =>
        {
            if (user != follower) return;

            if (trigerIndex == 0)
            {
                if (!isParkedInConvey)
                {
                    ParkInParkingSpot(trigger);
                }
                else
                {
                    MoveSmooth(_convySpeed);
                    isParking = false;
                    onSpline = false;
                    ReadyToInput = false;
                    canPickup = true;
                    _PassengerHolder.CheckCarsForPickup(_dataHelper.CarsInConvy);
                }
            }
            else if (trigerIndex == 1)
            {
                GoBackToParkingSpot();
            }
            else if (trigerIndex == 2)
            {

                //_dataHelper.CarsInConvy.Add(this);
            }
            else if (trigerIndex == 3)
            {
                OutFromTriger();
            }

            trigger.onCross.RemoveListener(callback);
        });
        trigger.onCross.AddListener(callback);

        void ParkInParkingSpot(SplineTrigger trigger)
        {

            if (_dataHelper.isTurtorial)
            {
                _dataHelper.levelSequnseTutorial.Sequence(0.5f);
            }

            if (_levelHolder.isAutoFill)
            {
                MoveFromParking();
            }

            if (!_IsreadyParkedinPark)
            {
                _levelHolder.CheckOnlyOneSpotLeft();
            }

            isParkecdInParking = true;
            follower.followSpeed = 0;
            ReadyToInput = true;
            Vector3 initialScale = new Vector3(1.2f, 1.2f, 1.1f);
            follower.SetPercent(trigger.position);
            _levelHolder.carsMoving.Remove(this);

            transform.DOScale(_dataHelper.scaleShakWhenParking, _dataHelper.timeShakWhenParking / 2).SetEase(Ease.InBounce).OnComplete(() =>
            {
                follower.SetPercent(trigger.position);
                transform.DOScale(initialScale, _dataHelper.timeShakWhenParking / 2).SetEase(Ease.InCubic);
            });

            _levelHolder.CheckLose();
        }

        void GoBackToParkingSpot()
        {
            _dataHelper.CarsInConvy.Remove(this);
            if (!isCarFull)
            {
                for (int i = 0; i < _levelHolder.parkcardIndex.Count; i++)
                {
                    if (this == _levelHolder.parkcardIndex[i])
                    {
                        return;
                    }
                }
                MoveSmooth(_carSpeed);

                int Checker = _levelHolder.AddCarToParking(this, true);
                if (Checker >= 0)
                {
                    _levelHolder.AddCarToParking(this);

                    if (currentPark >= 0)
                        canPickup = false;
                }
                else if (Checker == -1)
                {
                    Invoke("LoseCase", 0.5f);
                }
            }
        }
    }

    void OutFromTriger()
    {
        follower.spline = this.spline;
        follower.spline = null;
        follower.follow = false;
        isParkedInConvey = false;
        follower.wrapMode = SplineFollower.Wrap.Loop;

        StartCoroutine(MoveForword());
    }
    bool CheckFrontLine(int colorId)
    {
        //int frontRowCount = Mathf.Min(_PassengerHolder.passengerQueue.Count, _PassengerHolder.passengersPerRow);
        for (int i = 0; i < _PassengerHolder.passengersPerRow; i++)
        {
            if (_PassengerHolder.passengersGroups[i][0].passengerColorId == colorId)
                return true;
        }
        return false;
    }
    void MoveSmooth(float targetspeed)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(SmoothSpeed(targetspeed));
    }
    #endregion

    #region === Coroutines ===
    IEnumerator MoveForword()
    {
        _rb.isKinematic = false;

        while (!onSpline && !isCrashed)
        {
            _rb.linearVelocity = transform.forward.normalized * _carSpeed;
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator AssignePath()
    {
        follower.spline = spline;
        onpath = true;
        SplineSample startSample = new SplineSample();
        spline.Project(_pathCheckers[2].position, ref startSample);

        float startPercent = (float)startSample.percent;
        float targetPercent = 1f; // default
        if (currentPark >= 0)
        {
            SplineSample targetSample = new SplineSample();
            spline.Project(_levelHolder.nodesParkings[currentPark].position, ref targetSample);
            targetPercent = (float)targetSample.percent;
        }
        float forwardDist = (targetPercent - startPercent + 1f) % 1f;
        float backwardDist = (startPercent - targetPercent + 1f) % 1f;
        //_levelHolder.carsMoving.Add(this);
        float start = (float)startSample.percent;
        if (topCarMesh != null)
            topCarMesh.transform.DOScale(0, 0.2f).SetEase(Ease.InBounce).OnComplete(() => topCarMesh.transform.GetChild(0).gameObject.SetActive(false));

        follower.follow = false;
        isParking = false;
        isCarMoved = true;
        onSpline = true;
        follower.followSpeed = _carSpeed;
        transform.DOScale(1.1f, 0.3f).SetEase(Ease.InOutQuad);
        Vector3 dir;
        float dis = targetPercent - (float)startSample.percent;
        if (forwardDist <= backwardDist)
        {
            dir = startSample.forward;
        }
        else
        {
            dir = startSample.forward * -1;
        }

        _rb.isKinematic = true;
        float duration = _dataHelper.smoothTurn;
        float elapsed = 0f;
        Vector3 initialPos = transform.position;
        Quaternion initialRot = transform.rotation;
        yield return new WaitForEndOfFrame();
        Vector3 targetPos = startSample.position;
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(initialPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(initialRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (forwardDist <= backwardDist)
        {
            follower.direction = Spline.Direction.Forward;
        }
        else
        {
            follower.direction = Spline.Direction.Backward;
        }
        if (_levelHolder.isAutoFill && !garageCar)
        {
            currentPark = -10;
            canGoToConvyDirectly = true;
        }


        transform.position = startSample.position;
        follower.SetPercent(startSample.percent);
        follower.follow = true;

    }

    IEnumerator LaneTurne(List<SplineTracer.NodeConnection> nodes)
    {
        if (nodes.Count == 0) yield break;

        SplineTracer.NodeConnection nodecnx = nodes[nodes.Count - 1];
        Node.Connection[] cns = nodecnx.node.GetConnections();

        foreach (var item in cns)
        {
            if (canGoToConvyDirectly)
            {
                if ((item.spline == _levelHolder.convyEntrence[0] || item.spline == _levelHolder.convyEntrence[1]) && item.spline != follower.spline && onSpline)
                {
                    canGoToConvyDirectly = false;
                    follower.spline = item.spline;
                    follower.direction = Spline.Direction.Forward;
                    follower.wrapMode = SplineFollower.Wrap.Default;
                    MoveSmooth(_convySpeed);
                    yield return new WaitForEndOfFrame();
                    follower.SetPercent(0);
                    break;
                }
            }
            else
            {

                if (currentPark >= 0 && item.spline != follower.spline && item.spline != _levelHolder.convyEntrence[0] && item.spline != _levelHolder.convyEntrence[1])
                {
                    if (item.spline == _levelHolder.parkings[currentPark])
                    {
                        isParking = true;
                        follower.spline = item.spline;
                        canGoToConvyDirectly = false;
                        follower.direction = Spline.Direction.Forward;
                        isParkedInConvey = false;
                        AddListenrTriger(item.spline, 0);
                        follower.wrapMode = SplineFollower.Wrap.Default;
                        yield return new WaitForEndOfFrame();
                        follower.spline = item.spline;
                        follower.SetPercent(0);
                        break;
                    }
                }
                else if (item.spline == _levelHolder.conveySpline && item.spline != follower.spline && currentPark < 0)
                {

                    isParking = true;
                    if (currentPark >= 0)
                        _levelHolder.parkings[currentPark] = null;
                    currentPark = -10;
                    follower.spline = item.spline;
                    follower.direction = Spline.Direction.Forward;
                    follower.wrapMode = SplineFollower.Wrap.Default;
                    AddListenrTriger(item.spline, 0);
                    AddListenrTriger(item.spline, 1);
                    AddListenrTriger(item.spline, 2);
                    AddListenrTriger(item.spline, 3);
                    SplineSample sample = new SplineSample();
                    yield return new WaitForEndOfFrame();
                    _levelHolder.conveySpline.Project(transform.position, ref sample);
                    isParkedInConvey = true;
                    follower.SetPercent(sample.percent);
                    //MoveSmooth(_convySpeed);
                    _dataHelper.CarsInConvy.Add(this);
                    follower.followSpeed = _convySpeed;


                    break;
                }
                else if (item.spline == _levelHolder.outSpline && item.spline != follower.spline && isCarFull)
                {
                    if (_dataHelper.isTurtorial)
                    {
                        _dataHelper.levelSequnseTutorial.Sequence(0.5f);
                    }


                    _levelHolder.CheckOnlyOneSpotLeft();
                    MoveSmooth(_carSpeed);
                    isParking = true;
                    isParkedInConvey = false;
                    //_dataHelper.currentLevel.cars.Remove(this);
                    _levelHolder.carsMoving.Remove(this);
                    follower.spline = item.spline;
                    follower.direction = Spline.Direction.Forward;
                    follower.wrapMode = SplineFollower.Wrap.Default;
                    yield return new WaitForEndOfFrame();
                    follower.follow = true;
                    follower.SetPercent(0);
                    Destroy(gameObject, 1.5f);
                    break;
                }
            }
        }
    }

    IEnumerator SmoothSpeed(float targetSpeed)
    {
        float duration = _dataHelper.smoothAcceleration;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float speed = Mathf.Lerp(follower.followSpeed, targetSpeed, t);
            follower.followSpeed = speed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        follower.followSpeed = targetSpeed;
    }

    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < _pathCheckers.Length; i++)
    //    {

    //        Gizmos.DrawRay(_pathCheckers[i].position, transform.forward * 30f);
    //    }
    //}

    #endregion
}


