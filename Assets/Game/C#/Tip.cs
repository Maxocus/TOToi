using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum TipState
{
    点单等待,
    制作等待,
    品尝中,
}

public class Tip : MonoBehaviour
{
    public Image image;
    public Image fill;
    private float _waitDoTime;
    private float _waitAskTime;
    private float _time;
    private bool _isPlay;
    private Camera _camera;
    public Image trueOfFalseTip;
    public Sprite[] sprite;
    public Text stateText;
    private TipState _state;
    public GameObject commonPage;
    public GameObject anglePage;
    public Text nameText;
    public List<Sprite> chatIcons;
    public Image chatTip;


    public void Init(Drink drink, float waitDoTime, float waitAskTime)
    {
        image.sprite = drink.icon;
        nameText.text = drink.drinkName.ToString();
        this._waitDoTime = waitDoTime;
        _waitAskTime = waitAskTime;
        _time = waitDoTime;
        Open();
        trueOfFalseTip.gameObject.SetActive(false);
    }

    public void SwitchState(TipState state)
    {
        stateText.transform.DOScale(1.5f, 0.3f).OnComplete(() => { stateText.transform.DOScale(1, 0.3f); });
        stateText.text = state.ToString();
        StateExit();
        _state = state;
        StateEnter();
    }

    private void LookUp()
    {
        _camera = Camera.main;
        transform.LookAt(_camera.transform);
    }

    public void ShowAngle()
    {
        AddAction.instance.OpenPageBySize(gameObject);
        anglePage.SetActive(true);
        commonPage.SetActive(false);
    }

    public void ShowChat()
    {
        AddAction.instance.OpenPageBySize(gameObject);
        anglePage.SetActive(false);
        commonPage.SetActive(false);
        Sprite sprite = chatIcons[UnityEngine.Random.Range(0, chatIcons.Count)];
        chatTip.sprite= sprite;
        
    }

    private void Open()
    {
        float size = transform.localScale.x;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.zero;
        transform.DOScale(size, 0.5f);
        anglePage.SetActive(false);
        commonPage.SetActive(true);
    }

    public void Close()
    {
        transform.DOScale(0, 0.5f);
    }

    public void ShowTrueOfFalse(bool isTrue)
    {
        trueOfFalseTip.gameObject.SetActive(true);
        trueOfFalseTip.transform.localScale = Vector3.zero;
        trueOfFalseTip.transform.DOScale(1, 0.5f);
        trueOfFalseTip.sprite = sprite[isTrue ? 0 : 1];
        AddAction.instance.ClosePageBySize(stateText.gameObject);
        if (isTrue)
        {
            AddAction.instance.DelayPlay(ShowChat,1.5f);
        }
    }

    private void ShowFill(float time)
    {
        if (_isPlay)
        {
            if (_time >= 0)
            {
                _time -= Time.deltaTime;
                fill.fillAmount = _time / time;
                fill.color = _time > time / 3 ? Color.green : Color.red;
            }
        }
    }

    private void Update()
    {
        LookUp();
        StateUpdate();
    }

    private void StateEnter()
    {
        switch (_state)
        {
            case TipState.点单等待:
                _time = _waitAskTime;
                _isPlay = true;
                break;
            case TipState.制作等待:
                _time = _waitDoTime;
                _isPlay = true;
                break;
            case TipState.品尝中:
                fill.gameObject.SetActive(false);
                break;
        }
    }

    private void StateUpdate()
    {
        switch (_state)
        {
            case TipState.点单等待:
                ShowFill(_waitAskTime);

                break;
            case TipState.制作等待:
                ShowFill(_waitDoTime);
                break;
            case TipState.品尝中:

                break;
        }
    }

    private void StateExit()
    {
        switch (_state)
        {
            case TipState.点单等待:

                break;
            case TipState.制作等待:
                break;
            case TipState.品尝中:
                break;
        }
    }
}