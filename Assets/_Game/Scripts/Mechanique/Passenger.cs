using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DG.Tweening;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    DataHelper _dataHelper => DataHelper.instance;
    public int passengerColorId;
    public Car passengerCar;
    public int passengerSeatId;
    public bool isTaken = false;
    [SerializeField] SkinnedMeshRenderer _rend;
    public AnimationCurve moveCurve; // Assign in inspector for ease-in/out (e.g. easeInOut)
    [SerializeField] Animator _animator;
    [SerializeField] float _targetScale => _dataHelper.scalePassenger; // Target scale for the passenger
    [SerializeField] float _scaleForShake => _dataHelper.scalePassengerSeat; // Target scale for the passenger
    [SerializeField] float _timeShakePassenger => _dataHelper.timeShakePassenger; // Target scale for the passenger
    [SerializeField] GameObject sitDownParticle;
    Coroutine passengerCorotine;
    [HideInInspector]
    public UnityEngine.Color startColor;

    public void MoveToSeat(Transform seatTransform, Transform car, float duration)
    {
        passengerCorotine = StartCoroutine(MoveToSeatCoroutine(seatTransform, seatTransform.rotation, car, duration));
    }

    private IEnumerator MoveToSeatCoroutine(Transform targetPosition, Quaternion targetRotation, Transform car, float duration)
    {
        DOTween.Kill(this);
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        DOTween.Kill(this);
        float arcHeight = 2f;
        _animator.SetTrigger("Run");
        _dataHelper.SoundManager.PlaySound(4);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = moveCurve.Evaluate(t);
            Vector3 targetpos = new Vector3(targetPosition.position.x, targetPosition.position.y + curvedT, targetPosition.position.z);
            // Position with slight arc
            Vector3 flatLerp = Vector3.Lerp(transform.position, targetpos, elapsed / duration);
            float heightOffset = Mathf.Sin(Mathf.PI * curvedT) * arcHeight;
            Vector3 arcPos = new Vector3(flatLerp.x, flatLerp.y + heightOffset, flatLerp.z);
            // Apply
            transform.position = arcPos;
            transform.forward = targetPosition.forward;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _dataHelper.hapticsManager.PlayHaptic(1);

        BlinkColor(duration);
        DOTween.Kill(car);
        transform.DOScale(_scaleForShake, _timeShakePassenger).SetEase(Ease.OutBack).OnComplete(() => transform.DOScale(_targetScale, _timeShakePassenger));
        car.transform.DOShakeScale(_timeShakePassenger, 0.3f).SetEase(Ease.InBounce).OnComplete(() => car.transform.localScale = Vector3.one).SetId(car);
        sitDownParticle.SetActive(true);
        Destroy(sitDownParticle, 0.5f);
        _animator.SetTrigger("SitDown");
        _animator.SetTrigger("Cheering");
        transform.position = targetPosition.position;
        transform.forward = car.forward;
        transform.SetParent(targetPosition);

        //_rend.material.SetColor("_OutlineColor", StartColor);
    }

    public void Walk()
    {
        _animator.SetTrigger("Walk");
    }

    public void KillCorotine()
    {
        StopCoroutine(passengerCorotine);
    }
    [SerializeField] UnityEngine.Color _outlineColor;
    [SerializeField] UnityEngine.Color _glowOutlineColor;
    [SerializeField] UnityEngine.Color _testOutlineColor;
    public void SetColorId(int colorId, bool isFront = false)
    {
        passengerColorId = colorId;
        Material mat = new Material(DataHelper.instance.materialsStickman[colorId]);
        if (isFront)
        {
            mat.SetColor("_OutlineColor", _outlineColor);
        }
        _rend.material = mat;
    }

    public void BlinkColor(float duration)
    {
        StartCoroutine(Blink(duration));
        //_rend.material.SetColor("_OutlineColor", _glowOutlineColor);

    }
    IEnumerator Blink(float duration)
    {
        _rend.material.SetColor("_OutlineColor", _dataHelper.colorsBlinkOutline[passengerColorId]);
        //yield return new WaitForSeconds(0.1f);
        float elapsed = 0f;
        //duration = 0.2f;
        while (elapsed < duration)
        {
            _testOutlineColor = UnityEngine.Color.Lerp(_glowOutlineColor, startColor, elapsed / duration);

            _rend.material.SetColor("_OutlineColor", _testOutlineColor);
            elapsed += Time.deltaTime;
            yield return null;

        }

    }
}
