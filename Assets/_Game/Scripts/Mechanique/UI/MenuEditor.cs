using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class MenuEditor : MonoBehaviour
{
    public TunelFiller tunelFiller;
    [SerializeField] RectTransform carsPanel;
    [SerializeField] float up;
    [SerializeField] float down;
    [SerializeField] Level _levelHolder;
    [SerializeField] List<TextMeshProUGUI> _txtIndexes;
    public void MovePanel(bool isUporDown)
    {
        carsPanel.DOMoveY(isUporDown ? up : down, 0.3f);
    }

    private void Start()
    {
        UpdateText();
    }
    public void UpdateText(List<int> colorSpowned = null, int inedex = 0)
    {

        List<(int, int)> indexColor = _levelHolder.GetColorIndex();

        int tmpnumber = 0;
        for (int i = 0; i < indexColor.Count; i++)
        {
            tmpnumber = colorSpowned == null ? 0 : colorSpowned[i];

            _txtIndexes[i].text = (indexColor[i].Item2 - tmpnumber).ToString();
        }     
    }
}
