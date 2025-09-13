using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PassengersHolder : MonoBehaviour
{
    [SerializeField] Passenger _passengerPrefab;
    DataHelper _dataHelper => DataHelper.instance;
    [Range(0, 10), SerializeField] float spacing;
    [Range(0, 10), SerializeField] float offsetFirstPassenger;
    public List<Passenger> passengersReady;
    [SerializeField] float timeBetweenEveryPassenger;
    public TextMeshProUGUI txtCounterPassengers;
    public Image sliderPaasengers;

    public List<Passenger> passengerQueue = new List<Passenger>();

    public List<List<Passenger>> passengersGroups = new List<List<Passenger>>();
    public int VisibleCount;

    public int passengersPerRow => _dataHelper.passengersInRows;
    public int passengersInCol => _dataHelper.passengersInCols;
    public int passengersCount;


    public void AddPassenger(Passenger passenger)
    {
        passengerQueue.Add(passenger);
        ReAlignQueue();
    }
    public void CheckCarsForPickup(List<Car> cars)
    {

        if (passengerQueue.Count == 0) return;

        if (_dataHelper.levelHolder.isAutoFill)
        {
            StartCoroutine(PickupPassengerAuto(cars));
        }
        else
        {

            StartCoroutine(PickupPassenger(cars));
        }

    }

    public void UpdatePassengerCounter()
    {
        txtCounterPassengers.text = passengersCount.ToString();
        sliderPaasengers.fillAmount = Mathf.InverseLerp(_dataHelper.currentLevel.PassengersCount, 0, passengersCount);
    }

    public void AddPassengersToRows()
    {
        // init Listes
        passengersGroups.Clear();
        for (int i = 0; i < passengersPerRow; i++)
        {
            passengersGroups.Add(new List<Passenger>());
        }

        int indexRow = 0;
        for (int i = 0; i < passengersCount; i++)
        {
            passengersGroups[indexRow].Add(passengerQueue[i]);
            indexRow = indexRow == passengersPerRow - 1 ? 0 : indexRow + 1;
        }

        ReAlignQueue();
    }
    IEnumerator AllignePassengers(int index)
    {
        Vector3 startPosition = Vector3.zero;
        int row = 0;
        if (index != -1)
            row = index;
        float ofesset = (_dataHelper.passengersInRowsMax - passengersPerRow) / 2;
        for (row = 0; row < passengersGroups.Count; row++)
        {
            List<Passenger> group = passengersGroups[row];



            for (int col = 0; col < group.Count; col++)
            {
                Passenger passenger = group[col];
                if (col < passengersInCol)
                {
                    passenger.gameObject.SetActive(true);
                    Vector3 targetPosition = startPosition + new Vector3((row + ofesset) * spacing, 0f, (-col) * spacing);

                    if (col == 0)
                    {
                        passenger.SetColorId(passenger.passengerColorId, true);
                    }

                    if (index == row)
                        passenger.Walk();
                    passenger.transform.DOLocalMove(targetPosition, timeBetweenEveryPassenger).SetEase(Ease.OutExpo).SetId(passenger);
                }
                else
                {
                    break;
                }

                //passenger.transform.localPosition = targetPosition;
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
    public void ReAlignQueue(int index = -1)
    {
        StartCoroutine(AllignePassengers(index));
    }
    public bool CheckLineIfHasColor(Car car)
    {
        if (_dataHelper.levelHolder.AddCarToParking(car, true) >= 0)
        {
            int passengersCount = Mathf.Min(passengersPerRow, passengersGroups.Count);

            for (int i = 0; i < passengersCount; i++)
            {
                if (passengersGroups[i].Count == 0) continue;

                if (passengersGroups[i][0].passengerColorId == car.carColorId)
                {
                    return true;
                }
            }
        }
        else
        {
            return true;
        }

        return false;
    }
    [SerializeField] float timeToWait = 0.1f;
    IEnumerator PickupPassenger(List<Car> cars)
    {
        int count = Mathf.Min(passengersPerRow, passengersGroups.Count);
        List<Car> tmpcars = new List<Car>(cars);
        for (int i = 0; i < tmpcars.Count; i++)
        {
            if (tmpcars[i].isCarFull) continue;

            for (int m = 0; m < _dataHelper.passengersInCols; m++)
            {

                int index = count - 1;
                for (int j = 0; j < count; j++)
                {
                    if (tmpcars.Count <= i || passengersGroups.Count == 0)
                        break;

                    index = Mathf.Min(index, passengersGroups.Count - 1);

                    // Skip if row is empty (shouldn't happen now, but safe)
                    if (passengersGroups[index].Count == 0)
                    {
                        index--;
                        continue;
                    }

                    Passenger frontPassenger = passengersGroups[index][0];
                    if (tmpcars[i].TryPickupPassenger(frontPassenger))
                    {
                        passengersCount--;
                        UpdatePassengerCounter();
                        passengersGroups[index].Remove(frontPassenger);
                        ReAlignQueue(index);

                        yield return new WaitForSeconds(timeToWait);
                    }

                    // Win condition check
                    if (passengersGroups.TrueForAll(g => g.Count == 0))
                    {
                        if (_dataHelper.lose || _dataHelper.win) yield break;

                        _dataHelper.Win();
                        yield break;
                    }

                    index--;
                }
            }
        }
    }

    IEnumerator PickupPassengerAuto(List<Car> cars)
    {
        int count = Mathf.Min(passengersPerRow, passengersGroups.Count);
        List<Car> tmpcars = new List<Car>(cars);
        for (int i = 0; i < tmpcars.Count; i++)
        {
            if (tmpcars[i].isCarFull) continue;

            for (int m = 0; m < passengersGroups.Count; m++)
            {

                for (int j = 0; j < passengersGroups[m].Count; j++)
                {
                    if (tmpcars.Count <= i || passengersGroups.Count == 0)
                        break;

                    // Skip if row is empty (shouldn't happen now, but safe)
                    if (passengersGroups[m].Count == 0)
                    {
                        continue;
                    }

                    Passenger frontPassenger = passengersGroups[m][j];
                    if (tmpcars[i].TryPickupPassenger(frontPassenger))
                    {
                        passengersCount--;
                        UpdatePassengerCounter();
                        passengersGroups[m].Remove(frontPassenger);
                        j--;
                        ReAlignQueue(m);

                        yield return new WaitForSeconds(timeToWait);
                    }

                    // Win condition check
                    if (passengersGroups.TrueForAll(g => g.Count == 0))
                    {
                        if (_dataHelper.lose || _dataHelper.win) yield break;


                        _dataHelper.Win();
                        yield break;
                    }

                    //index--;
                }
            }
        }
    }


    int FindNearestNonEmptyGroup(int emptyIndex)
    {
        int minDistance = int.MaxValue;
        int closestIndex = -1;

        for (int i = passengersGroups.Count - 1; i >= 0; i--)
        {
            if (i != emptyIndex && passengersGroups[i].Count > 0)
            {
                int distance = Mathf.Abs(i - emptyIndex);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
        }

        return closestIndex;
    }
}

