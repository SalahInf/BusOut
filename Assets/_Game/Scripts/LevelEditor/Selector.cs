using System;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;
public class Selector : MonoBehaviour
{
    public Material[] materials;

    [SerializeField] Car[] _cars;
    [SerializeField] Garage _garage;
    [SerializeField] Transform[] _objtyps;
    [HideInInspector] public float targetAngle;
    [HideInInspector] public int currentCar;
    [HideInInspector] public int currentObjtyps;
    [HideInInspector] public int currentMaterial;

    [SerializeField] Image _currentColor;

    [Range(0f, 1f), SerializeField] float _sencivity;
    [SerializeField, Range(0.001f, 100f)] float _rotationSpeed;
    public int _currentSelectotr = 0;

    private void Start()
    {
        SelactMaterial(0);
    }
    public void SelactMaterial(int indexMat)
    {
        currentMaterial = indexMat;

        _cars[currentCar].SetColors(currentMaterial);

        _currentColor.color = DataHelper.instance.materialsStickman[currentMaterial].color;

    }
    public void SelectCar(int index)
    {
        ShowCurrentSelector(0);
        _currentSelectotr = 0;
        currentCar = index;

        for (int i = 0; i < _cars.Length; i++)
        {
            _cars[i].gameObject.SetActive(currentCar == i ? true : false);
        }
        _cars[currentCar].SetColors(currentMaterial);
        _cars[currentCar].transform.localScale = Vector3.one * DataHelper.instance.scaleCarMultiplayer;
        _cars[currentCar].transform.rotation = Quaternion.Euler(0, targetAngle, 0);
    }

    public void SelectGarage(int index)
    {
        ShowCurrentSelector(1);
        _currentSelectotr = 1;
        _garage.gameObject.SetActive(true);
        _garage.transform.rotation = Quaternion.Euler(0, targetAngle, 0);
        _garage.transform.localScale = Vector3.one * DataHelper.instance.scaleCarMultiplayer;
        for (int i = 0; i < _cars.Length; i++)
        {
            _cars[i].gameObject.SetActive(false);
        }
    }

    void ShowCurrentSelector(int index)
    {
        for (int i = 0; i < _objtyps.Length; i++)
        {
            _objtyps[i].gameObject.SetActive(index == i ? true : false);
        }


    }
    private void Update()
    {
        RotateCar();
    }

    void RotateCar()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // Scroll direction determines rotation
            float direction = scroll > 0 ? 1 : -1;
            targetAngle += (90f * _sencivity) * direction;

            Vector3 targetRotation = new Vector3(0, targetAngle, 0);

            DOTween.Kill(this);
            float time = Mathf.InverseLerp(100, 0.001f, _rotationSpeed);
            transform.DORotate(targetRotation, time).SetId(this);
            //_cars[_currentCar].transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
