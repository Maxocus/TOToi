using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    [Header("数据")] public int moneyStart;
    private int _money;
    [Header("UI")] public Text moneyText;

    private void Start()
    {
        Init();
    }

    public void AddMoney(int value)
    {
        _money += value;
        UpdateUI();
    }

    public bool ReMoveMoney(int value)
    {
        if (_money < value)
        {
            Debug.Log("没有足够的钱");
            return false;
        }

        _money -= value;
        UpdateUI();
        return true;
    }

    private void Init()
    {
        _money = moneyStart;
        UpdateUI();
    }

    private void UpdateUI()
    {
        moneyText.text = _money.ToString();
    }
}