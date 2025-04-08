using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CommonButton : MonoBehaviour
{
    private Button _button;
    private float _scaleStart;

    private void Start()
    {
        _scaleStart = transform.localScale.x;
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append( transform.DOScale(_scaleStart * 1.2f, 0.1f));
            sequence.OnComplete(() =>
            {
                transform.DOScale(_scaleStart, 0.1f);
            });
        });
    }
}