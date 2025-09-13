using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines.Primitives;
using UnityEngine;

public class LevelSecunseTutorial : MonoBehaviour
{
    Car carbleu;
    Car yellow;

    Collider _colbleuCar;
    Collider _colyelowCar;


    public void Init(Car bleu, Car yelow)
    {

        carbleu = bleu;
        yellow = yelow;


        _colbleuCar = carbleu.GetComponent<BoxCollider>();
        _colyelowCar = yellow.GetComponent<BoxCollider>();

        _colyelowCar.enabled = false;
        _colbleuCar.enabled = false;
        Sequence(0.5f);
    }


    void EnableYelowCarFromMoving()
    {
        _colyelowCar.enabled = true;
        _colbleuCar.enabled = false;
    }

    void EnableBleuCarFromMoving()
    {
        _colbleuCar.enabled = true;
        _colyelowCar.enabled = false;
    }

    int secenceIndex = 0;

    public void Sequence(float time)
    {

        StartCoroutine(SequenceWaiter(time));
    }


    public void ActivatePointerBleu(bool state, int children)
    {
        carbleu.carPointerTutorial.SetActive(state);
        carbleu.carPointerTutorial.transform.GetChild(children).gameObject.SetActive(state);
    }
    public void ActivatePointerYelow(bool state, int children)
    {
        yellow.carPointerTutorial.SetActive(state);
        yellow.carPointerTutorial.transform.GetChild(children).gameObject.SetActive(state);
    }

    IEnumerator SequenceWaiter(float time)
    {
        if (secenceIndex <= 2)
        {
            yield return new WaitForSeconds(time);

            switch (secenceIndex)
            {
                case 0:
                    EnableBleuCarFromMoving();
                    ActivatePointerBleu(true, 1);
                    break;
                case 1:
                    EnableYelowCarFromMoving();
                    ActivatePointerYelow(true, 1);
                    break;
                case 2:
                    EnableBleuCarFromMoving();
                    ActivatePointerBleu(true, 0);
                    break;
            }
            secenceIndex++;
        }
    }



}
