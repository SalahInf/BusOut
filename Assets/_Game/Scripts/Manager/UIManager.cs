using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    DataHelper _dataHelper => DataHelper.instance;
    [SerializeField] CanvasGroup _backGround;
    [SerializeField] CanvasGroup _winUi;
    [SerializeField] CanvasGroup _loseUi;
    [SerializeField] CanvasGroup _pauseUi;
    [SerializeField] CanvasGroup _gameplayUi;
    [SerializeField] RectTransform _carRect;
    [SerializeField] RectTransform _winFx;
    [SerializeField] CanvasGroup _oneLeftSpotLeftUi;
    [SerializeField] CanvasGroup _autoFillBtn;

    void FadeUi(bool state)
    {
        StartCoroutine(WaitToShow(state, 5, 0));
        _dataHelper.pause = state;
    }

    public void ShowWin(bool state)
    {
        ShowWinFx(true);
        StartCoroutine(WaitToShow(state, 1, state ? 1f : 0));

    }

    public void ShowLose(bool state)
    {
        StartCoroutine(WaitToShow(state, 2, state ? 0.3f : 0));
    }

    public void ShowPause(bool state)
    {
        StartCoroutine(WaitToShow(state, 3, 0));
    }

    public void ShowGameplayUI(bool state)
    {
        StartCoroutine(WaitToShow(state, 4, 0));

    }
    public void ShowOneLeftSpotLeftUi(bool state)
    {

        StartCoroutine(WaitToShow(state, 6, 0.05f));
    }

    IEnumerator WaitToShow(bool state, int index, float time)
    {

        yield return new WaitForSeconds(time);

        switch (index)
        {
            case 1:

                FadeUi(true);
                _winUi.alpha = 0;
                //_winUi.gameObject.SetActive(state);
                ShowWinFx(false);
                _winUi.DOFade(state ? 1 : 0, 0.2f).OnComplete(() => _winUi.gameObject.SetActive(state));
                _gameplayUi.gameObject.SetActive(false);
                _pauseUi.gameObject.SetActive(false);
                _oneLeftSpotLeftUi.gameObject.SetActive(false);
                ShowAutoFill(false);
                _dataHelper.SoundManager.PlaySound(0);
                break;
            case 2:
                FadeUi(true);
                _loseUi.alpha = 0;
                //_loseUi.gameObject.SetActive(state);
                _loseUi.DOFade(state ? 1 : 0, 0.2f).OnComplete(() => _loseUi.gameObject.SetActive(state));
                _gameplayUi.gameObject.SetActive(false);
                _winUi.gameObject.SetActive(false);
                _pauseUi.gameObject.SetActive(false);
                _oneLeftSpotLeftUi.gameObject.SetActive(false);
                ShowAutoFill(false);
                _dataHelper.SoundManager.PlaySound(1);
                break;
            case 3:
                FadeUi(true);
                _pauseUi.alpha = 0;
                //_pauseUi.gameObject.SetActive(state);
                _pauseUi.DOFade(state ? 1 : 0, 0.2f).OnComplete(() => _pauseUi.gameObject.SetActive(state));

                _gameplayUi.gameObject.SetActive(false);
                _winUi.gameObject.SetActive(false);
                _oneLeftSpotLeftUi.gameObject.SetActive(false);
                ShowAutoFill(false);
                _loseUi.gameObject.SetActive(false);
                break;
            case 4:
                FadeUi(false);

                _gameplayUi.DOFade(state ? 1 : 0, 0.1f).OnComplete(() => _gameplayUi.gameObject.SetActive(state));
                _winUi.gameObject.SetActive(false);
                _loseUi.gameObject.SetActive(false);
                _oneLeftSpotLeftUi.gameObject.SetActive(false);
                ShowAutoFill(false);
                _pauseUi.gameObject.SetActive(false);
                break;
            case 5:
                //_backGround.gameObject.SetActive(state);
                //if (_backGround.alpha != 1)
                _backGround.DOFade(state ? 1 : 0, 0.1f).OnComplete(() => _backGround.gameObject.SetActive(state));
                break;
            case 6:
               

             DOTween.Kill(5);             
                _oneLeftSpotLeftUi.gameObject.SetActive(state);
                Transform t = _oneLeftSpotLeftUi.transform.GetChild(0).transform;
                _oneLeftSpotLeftUi.DOFade(state ? 1 : 0, 0.15f).OnComplete(() => t.DOShakeScale(1.5f, 0f).SetEase(Ease.InOutBack).OnComplete(() => _oneLeftSpotLeftUi.DOFade(0, 0.1f)).SetId(5));
                break;
        }
    }


    public void ShowAutoFill(bool state)
    {
        _autoFillBtn.gameObject.SetActive(state);      
    }

    public void ShowWinFx(bool state)
    {
        _winFx.gameObject.SetActive(state);

    }
}
