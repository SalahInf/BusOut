using System;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using System.Linq;

public class Level : MonoBehaviour
{
    public List<Car> cars = new List<Car>();
    public List<Garage> garages = new List<Garage>();
    public int PassengersCount;

    [Header("Number of unique colors to assign")]
    public Transform passengersHolder;
    public Transform carsHolder;

    private Dictionary<int, int> passengersColors = new Dictionary<int, int>();
    public List<(int colorId, int groupSize)> colorGroups = new List<(int, int)>();
    public List<Passenger> passengers = new List<Passenger>();
    private List<int> prioritizedColorOrder = new();
    private System.Random rng = new System.Random();
    public Transform centerPoint;

    public int GetPassengersCount()
    {
        PassengersCount = 0;

        for (int i = 0; i < cars.Count; i++)
        {
            PassengersCount += cars[i].carPassengers;
            int colorCarId = cars[i].carColorId;
            bool foundColor = false;
            foreach (var item in passengersColors)
            {
                if (item.Key == colorCarId)
                {
                    foundColor = true;
                    passengersColors[colorCarId] += cars[i].carPassengers;
                    break;
                }
            }
            if (!foundColor)
            {
                passengersColors.Add(colorCarId, cars[i].carPassengers);
            }

        }
        return PassengersCount;
    }
    public void StopAllCars()
    {
        foreach (var item in cars)
        {
            if (item != null)
                item.follower.enabled = false;
        }
    }
    public void AddPassengers(int colorindex, int carIndex)
    {
        bool foundColor = false;
        PassengersCount += cars[carIndex].carPassengers;
        foreach (var item in passengersColors)
        {
            if (item.Key == colorindex)
            {
                foundColor = true;
                passengersColors[colorindex] += cars[carIndex].carPassengers;
                break;
            }
        }
        if (!foundColor)
        {
            passengersColors.Add(colorindex, cars[carIndex].carPassengers);
        }
    }
    public List<(int, int)> GetColorIndex()
    {
        List<(int, int)> indexColor = new List<(int, int)>();
        for (int i = 0; i < DataHelper.instance.materialsStickman.Length; i++)
        {
            int count = 0;
            indexColor.Add((i, 0));
            for (int j = 0; j < cars.Count; j++)
            {
                if (cars[j].carColorId == i)
                {
                    count += cars[j].carPassengers;
                    indexColor[i] = (cars[j].carColorId, count);
                }
            }
        }

        for (int i = 0; i < indexColor.Count; i++)
        {
            for (int j = 0; j < garages.Count; j++)
            {
                for (int g = 0; g < garages[j].colorIndex.Count; g++)
                {
                    if (garages[j].colorIndex[g] == i)
                    {
                        int value = indexColor[i].Item2;
                        switch (garages[j].carIndex[g])
                        {
                            case 0:
                                value += 4;
                                break;
                            case 1:
                                value += 6;
                                break;
                            case 2:
                                value += 10;
                                break;
                        }
                        indexColor[i] = (indexColor[i].Item1, value);
                    }

                }

            }
        }

        return indexColor;
    }
    //

    //public List<Car> tmpcars;
    //void InitCarsValues()
    //{
    //    tmpcars.Clear();
    //    tmpcars = new List<Car>(cars);

    //    for (int i = 0; i < garages.Count; i++)
    //    {
    //        for (int j = 0; j < garages[i].carIndex.Count; j++)
    //        {
    //            Car c = new Car();
    //            c.carId = garages[i].carIndex[j];
    //            c.carColorId = garages[i].colorIndex[j];
    //            c.carPos = garages[i].transform.localPosition;
    //            switch (garages[i].carIndex[j])
    //            {
    //                case 0:
    //                    c.carPassengers = 4;
    //                    break;
    //                case 1:
    //                    c.carPassengers = 6;
    //                    break;
    //                case 2:
    //                    c.carPassengers = 10;
    //                    break;
    //            }
    //            PassengersCount += c.carPassengers;
    //            tmpcars.Add(c);

    //        }
    //    }
    //}
    public void SetPassengersColors(List<Passenger> passengers)
    {
        //InitCarsValues();
        PassengersCount = GetPassengersCount();
        if (PassengersCount <= 0 || cars.Count == 0)
        {
            return;
        }
        InitializeColorQuotasFromCars();
        DistributeGroupsFromPool();
        AssignGroupsToPassengers(passengers);
    }
    private void InitializeColorQuotasFromCars()
    {
        passengersColors.Clear();
        colorGroups.Clear();
        prioritizedColorOrder.Clear();
        //InitCarsValues();
        var sortedCars = cars
            .Where(car => car.carPassengers > 0)
            .OrderByDescending(car => Vector3.Distance(car.carPos, centerPoint.localPosition))
            .ToList();

        foreach (var car in sortedCars)
        {
            int colorId = car.carColorId;

            if (!passengersColors.ContainsKey(colorId))
            {
                passengersColors[colorId] = 0;
                prioritizedColorOrder.Add(colorId);
            }

            passengersColors[colorId] += car.carPassengers;
        }
    }
    private readonly List<int> groupSizePool = new List<int>
    {
        1, 2,2,2, 3,3,3, 4, 4, 5,
        1, 2, 3, 4, 5, 6
    };
    private void DistributeGroupsFromPool()
    {
        int remainingPassengers = PassengersCount;
        Dictionary<int, int> availableSeatsPerColor = new(passengersColors);

        while (remainingPassengers > 0 && availableSeatsPerColor.Count > 0)
        {
            var biasedColors = prioritizedColorOrder
                .Where(id => availableSeatsPerColor.ContainsKey(id))
                .Take(3)
                .OrderBy(x => rng.Next())
                .ToList();

            if (biasedColors.Count == 0)
                biasedColors = availableSeatsPerColor.Keys.ToList();

            int colorId = biasedColors[rng.Next(biasedColors.Count)];
            int colorRemaining = availableSeatsPerColor[colorId];

            var validGroupSizes = groupSizePool
                .Where(size => size <= colorRemaining && size <= remainingPassengers)
                .ToList();

            if (validGroupSizes.Count == 0)
            {
                availableSeatsPerColor.Remove(colorId);
                continue;
            }

            int groupSize = validGroupSizes[rng.Next(validGroupSizes.Count)];

            colorGroups.Add((colorId, groupSize));
            availableSeatsPerColor[colorId] -= groupSize;
            remainingPassengers -= groupSize;

            if (availableSeatsPerColor[colorId] <= 0)
                availableSeatsPerColor.Remove(colorId);
        }
    }

    private void AssignGroupsToPassengers(List<Passenger> passengers)
    {
        int indexPassenger = 0;

        foreach (var (colorId, groupSize) in colorGroups)
        {
            for (int j = 0; j < groupSize && indexPassenger < passengers.Count; j++)
            {
                passengers[indexPassenger].SetColorId(colorId);
                indexPassenger++;
            }
        }
    }





}



