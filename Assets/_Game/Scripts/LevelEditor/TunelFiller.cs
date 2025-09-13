using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class TunelFiller : MonoBehaviour
{
    #if UNITY_EDITOR

    [SerializeField] List<Toggle> carstoggles;
    [SerializeField] List<Toggle> ColorsToggles;
    public int currentCarIndex = 0;
    public int currentColorIndex = 0;
    private bool isColorChanging = false;
    private bool isCarChanging = false;


    public TMP_Dropdown tmpDropdown;
    [SerializeField] List<TextMeshProUGUI> CarsTextCount = new List<TextMeshProUGUI>();
    [SerializeField] Level level;
    [SerializeField] MenuEditor _menuEditor;
    [SerializeField] Spowner _spowner;
    [SerializeField] RectTransform tunelPanel;



    [SerializeField] float _right;
    [SerializeField] float _left;

    void Start()
    {
        tmpDropdown.ClearOptions();
        tmpDropdown.RefreshShownValue();

        tmpDropdown.options.Add(new TMP_Dropdown.OptionData("Tunels"));

        foreach (var car in carstoggles)
        {
            car.onValueChanged.AddListener((isOn) => OnToggleCarChanged(car, isOn));
        }

        foreach (var color in ColorsToggles)
        {
            color.onValueChanged.AddListener((isOn) => OnToggleColorChanged(color, isOn));
        }
        EnsureOneToggleIsOn();

    }
    public void AddDropDownValue(int index)
    {
        tmpDropdown.options.Add(new TMP_Dropdown.OptionData($"Tunel > {index + 1}"));

    }

    public void RemoveDropDown(int index)
    {
        tmpDropdown.options.RemoveAt(index + 1);
    }
    void OnToggleCarChanged(Toggle changedToggle, bool isOn)
    {
        if (isCarChanging) return;
        isCarChanging = true;

        if (isOn)
        {
            // Turn off all others
            foreach (var toggle in carstoggles)
            {
                if (toggle != changedToggle)
                    toggle.isOn = false;
                else
                    currentCarIndex = carstoggles.IndexOf(toggle);
            }
        }
        else
        {
            // Prevent all toggles from being off
            if (!AnyCarToggleOn())
            {
                changedToggle.isOn = true;

            }
        }

        isCarChanging = false;
    }

    bool AnyCarToggleOn()
    {
        foreach (var toggle in carstoggles)
        {
            if (toggle.isOn)
            {
                return true;
            }
        }
        return false;
    }
    void OnToggleColorChanged(Toggle changedToggle, bool isOn)
    {
        if (isColorChanging) return;
        isColorChanging = true;

        if (isOn)
        {
            // Turn off all others
            foreach (var toggle in ColorsToggles)
            {
                if (toggle != changedToggle)
                    toggle.isOn = false;
                else
                    currentColorIndex = ColorsToggles.IndexOf(toggle);
            }
        }
        else
        {
            // Prevent all toggles from being off
            if (!AnyColorToggleOn())
            {
                changedToggle.isOn = true;

            }
        }

        isColorChanging = false;
    }
    bool AnyColorToggleOn()
    {
        foreach (var toggle in ColorsToggles)
        {
            if (toggle.isOn)
            {
                return true;
            }
        }
        return false;
    }

    void EnsureOneToggleIsOn()
    {
        if (!AnyCarToggleOn() && carstoggles.Count > 0)
        {
            carstoggles[0].isOn = true;
        }

        if (!AnyColorToggleOn() && ColorsToggles.Count > 0)
        {
            ColorsToggles[0].isOn = true;
        }


    }

    void UpdateTexTCars()
    {
        for (int i = 0; i < CarsTextCount.Count; i++)
        {
            int count = 0;
            for (int j = 0; j < level.garages.Count; j++)
            {
                for (int k = 0; k < level.garages[j].carIndex.Count; k++)
                {
                    if (level.garages[j].carIndex[k] == i)
                    {
                        count++;

                    }
                }

            }

            CarsTextCount[i].text = $"{count}";
        }

    }

    public void SaveCarInGarage()
    {
        (int, int) data = (currentCarIndex, currentColorIndex);
        int index = tmpDropdown.value - 1;
        if (index >= 0)
        {
            level.garages[index].FillGarage(data);
            
            UpdateTexTCars();
            _menuEditor.UpdateText(_spowner.colorSpownedCount, data.Item2);
        }

    }

    public void MovePanel(bool isLeftorRight = false)
    {
        tunelPanel.DOMoveX(isLeftorRight ? _right : _left, 0.3f);

    }
#endif
}
