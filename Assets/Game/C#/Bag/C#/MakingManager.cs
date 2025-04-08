using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MakingManager : MonoBehaviour
{
    [Header("基础元素")] public GameObject drinkMakingPage;
    public Button makingButton;
    public Button closeButton;
    private Drink _currentShowDrink;

    [Header("制作菜单")] public List<Drink> drinks;
    public GameObject drinksButtonPrefab;
    public Transform drinksButtonParent;
    private MakeMenuButton _currentShowDrinkButton;
    private List<MakeMenuButton> _makeMenuButtons = new List<MakeMenuButton>();

    [Header("购买菜单")] public Button buyPageOpenButton;
    public Button buyPageCloseButton;
    public GameObject buyPage;
    public Button countAddButton;
    public Button countReduceButton;
    public Text countText;
    public GameObject graphDrink;
    public Text moneyText;
    public Button buyButton;
    public Transform graphDrinkParent;
    private int buyCount = 1;
    private int _needMoney;


    [Header("组成成分的菜单")] public GameObject itemButtonPrefab;
    public Transform itemButtonParent;

    [Header("主页")] public Text mainNameText;
    public Text mainContextText;
    public Text timeText;
    public Image mainIconImage;

    [Header("材料提示")] public GameObject noticePage;

    [Header("背包")] private InventoryManager _inventoryManager;
    private List<ItemBase> needItem = new List<ItemBase>();
    private DialogManager _dialogManager;


    [Header("制作显示")] public List<CupPoint> cupPoints;
    private List<Drink> _havenMakingDrink = new List<Drink>();
    public GameObject cupPrefab;

    [Header("提杯子")] public GameObject cupParent;
    private Cup _currnetCup;
    private List<Cup> _allCups = new List<Cup>();

    [Header("其他ui")] public GameObject[] pages;


    private void Start()
    {
        //AddAction.instance.DelayPlay(OpenPage,2f);
        _inventoryManager = GetComponent<InventoryManager>();
        closeButton.onClick.AddListener(ClosePage);
        UpdateUIDrinkMenu();
        makingButton.onClick.AddListener(Making);
        _dialogManager = GameManager.instance.GetComponent<DialogManager>();
        buyPageCloseButton.onClick.AddListener(CloseBuyPage);
        buyPageOpenButton.onClick.AddListener(ShowBuyPage);
        countAddButton.onClick.AddListener(AddBuyCount);
        countReduceButton.onClick.AddListener(ReduceBuyCount);
        buyButton.onClick.AddListener(BuyDrink);
    }

    public void OpenPage()
    {
        AddAction.instance.OpenPageBySize(drinkMakingPage);
        _currentShowDrink = drinks[0];
        UpdateUIItemMenu();
        UpdateMainMenu();
        CloseOtherPage();
        UpdateDrinkMenuCount();
    }

    private void ClosePage()
    {
        AddAction.instance.ClosePageBySize(drinkMakingPage);
        ShowOtherPage();
    }

    private void UpdateUIDrinkMenu()
    {
        foreach (var drink in drinks)
        {
            var button = Instantiate(drinksButtonPrefab, drinksButtonParent);
            MakeMenuButton makeMenuButton = button.GetComponent<MakeMenuButton>();
            makeMenuButton.Init(drink);
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentShowDrink = drink;
                _currentShowDrinkButton?.BeUnSelect();
                _currentShowDrinkButton = makeMenuButton;
                makeMenuButton.BeSelect();
                UpdateUIItemMenu();
                UpdateMainMenu();
            });
            _makeMenuButtons.Add(makeMenuButton);
        }
    }

    private void CloseOtherPage()
    {
        foreach (var page in pages)
        {
            AddAction.instance.ClosePageBySize(page);
        }
    }

    private void ShowOtherPage()
    {
        foreach (var page in pages)
        {
            AddAction.instance.OpenPageBySize(page);
        }
    }

    private void UpdateUIItemMenu()
    {
        int count = itemButtonParent.childCount;

        for (int i = 0; i < count; i++)
        {
            Destroy(itemButtonParent.GetChild(i).gameObject);
        }

        needItem.Clear();
        foreach (var item in _currentShowDrink.compose)
        {
            if (!needItem.Contains(item))
            {
                needItem.Add(item);
            }
        }

        foreach (var item in needItem)
        {
            var button = Instantiate(itemButtonPrefab, itemButtonParent);
            button.transform.GetChild(0).GetComponent<Image>().sprite = item.icon;
        }
    }

    private void UpdateDrinkMenuCount()
    {
        foreach (var makeMenuButton in _makeMenuButtons)
        {
            makeMenuButton.SetCount(0);
        }

        foreach (var cup in _allCups)
        {
            MakeMenuButton makeMenuButton = _makeMenuButtons.Find(b => b.GetDrink() == cup.GetDrink());
            makeMenuButton?.AddCount();
        }
    }

    private void UpdateMainMenu()
    {
        mainNameText.text = _currentShowDrink.drinkName.ToString();
        mainIconImage.sprite = _currentShowDrink.icon;
        mainContextText.text = _currentShowDrink.context;
        NeedTimeShow(_currentShowDrink.needTime);
    }


    public void Making()
    {
        List<ItemBase> notHaveItem = new List<ItemBase>();
        List<Drink> canDoItem = GameManager.instance.GetComponent<LevelManager>().GetItemCanDo();
        foreach (var item in needItem)
        {
            ItemPlate itemPlate = _inventoryManager.InspectIsHave(item);
            if (itemPlate == null)
            {
                notHaveItem.Add(item);
            }
        }

        if (canDoItem.Contains(_currentShowDrink))
        {
            if (notHaveItem.Count == 0 && _havenMakingDrink.Count < 2)
            {
                MakeDelay(_currentShowDrink.needTime);
            }
            else
            {
                if (notHaveItem.Count > 0)
                {
                    _dialogManager.Show(DialogType.MakingFail);
                }
                else if (_havenMakingDrink.Count >= 2)
                {
                    _dialogManager.Show(DialogType.MakingDrinkNoPosition);
                }
            }
        }
        else
        {
            _dialogManager.Show(DialogType.MakingDrinkNoDo);
        }
    }

    private void UpdateMakingDrinkShow(Drink drink)
    {
        GameObject cup = Instantiate(cupPrefab);
        GameObject selectDrink = Instantiate(drink.drinkPrefab);
        selectDrink.transform.position = cup.transform.GetChild(0).position;
        selectDrink.transform.SetParent(cup.transform);
        CupPoint selectPoint = GetPoint();
        cup.GetComponent<Cup>().Init(drink, selectPoint);
        _allCups.Add(cup.GetComponent<Cup>());
    }

    private void ShowBuyPage()
    {
        AddAction.instance.OpenPageBySize(buyPage);
        UpdateBuyPageDrinkGraphShow();
        buyCount = 1;
        _needMoney = 0;
        foreach (var item in needItem)
        {
            _needMoney += item.money;
        }

        moneyText.text = _needMoney.ToString();
        countText.text = buyCount.ToString();
    }

    private void CloseBuyPage()
    {
        AddAction.instance.ClosePageBySize(buyPage);
    }


    private void UpdateBuyPageDrinkGraphShow()
    {
        int count = graphDrinkParent.childCount;

        for (int i = 0; i < count; i++)
        {
            Destroy(graphDrinkParent.GetChild(i).gameObject);
        }

        List<ItemBase> items = _currentShowDrink.compose;
        foreach (var item in items)
        {
            var graph = Instantiate(graphDrink, graphDrinkParent);
            graph.GetComponent<DrinkGraph>().Init(item);
        }
    }

    private void AddBuyCount()
    {
        buyCount += 1;
        countText.text = buyCount.ToString();
        moneyText.text = (_needMoney * buyCount).ToString();
    }

    private void ReduceBuyCount()
    {
        buyCount -= 1;
        if (buyCount < 1)
        {
            buyCount = 1;
        }

        countText.text = buyCount.ToString();
        moneyText.text = (_needMoney * buyCount).ToString();
    }

    private void BuyDrink()
    {
        MoneyManager money = GetComponent<MoneyManager>();
        if (money.ReMoveMoney(_needMoney * buyCount))
        {
            foreach (var item in needItem)
            {
                _inventoryManager.AddItem(item);
            }

            _dialogManager.Show(DialogType.BuySuccessful);
        }
        else
        {
            _dialogManager.Show(DialogType.BuyFail);
        }
    }

    private CupPoint GetPoint()
    {
        CupPoint selectPoint = null;
        foreach (var cupPoint in cupPoints)
        {
            if (!cupPoint.isUse)
            {
                selectPoint = cupPoint;
            }
        }

        return selectPoint;
    }

    public void ReMoveCupPoint(Drink drink, CupPoint cupPoint)
    {
        _havenMakingDrink.Remove(drink);
        cupPoint?.SetUse(false);
    }

    public void PutUpCup(Cup cup)
    {
        if (_currnetCup != null)
        {
            _currnetCup.transform.position = GetPoint().point.position;
            _currnetCup.transform.SetParent(null);
            _currnetCup.GetComponent<Cup>().ControlInspect(true);
            if (!_allCups.Contains(_currnetCup))
                _allCups.Add(_currnetCup);
        }

        PlayerControl playerControl = GetComponent<PlayerControl>();
        playerControl.PlayerAni("PutUpCup", true);
        cup.transform.position = cupParent.transform.position;
        cup.transform.SetParent(cupParent.transform);
        _currnetCup = cup;
        if (_allCups.Contains(_currnetCup))
            _allCups.Remove(_currnetCup);
    }

    public void GiveCup(NpcController npc)
    {
        bool isActive = _currnetCup.GetDrink() == npc.GetNeedDrink();
        if (isActive)
        {
            PlayerControl playerControl = GetComponent<PlayerControl>();
            playerControl.PlayerAni("PutUpCup", false);
            npc.OnSentDrinkTrue(_currnetCup.GetDrink());
            if (_allCups.Contains(_currnetCup))
                _allCups.Remove(_currnetCup);
            Destroy(_currnetCup.gameObject);
            _currnetCup = null;
        }
        else
        {
            npc.OnSentDrinkFalse();
        }

        npc.ShowTrueOfFalse(isActive);
    }

    public void GiveToRubbish()
    {
        if (_currnetCup != null)
        {
            if (_allCups.Contains(_currnetCup))
                _allCups.Remove(_currnetCup);
            Destroy(_currnetCup.gameObject);
            _currnetCup = null;
            PlayerControl playerControl = GetComponent<PlayerControl>();
            playerControl.PlayerAni("PutUpCup", false);
        }
    }

    public void GiveSaveBag(SpecialNpc npc)
    {
        if (npc.needDrink == _currentShowDrink)
        {
            PlayerControl playerControl = GetComponent<PlayerControl>();
            playerControl.PlayerAni("PutUpCup", false);
            Destroy(_currnetCup.gameObject);
            _currnetCup = null;
        }
    }


    public Cup GetCup()
    {
        return _currnetCup;
    }

    public void MakeDelay(float needTime)
    {
        StartCoroutine(MakeDelayIe(needTime));
    }

    private IEnumerator MakeDelayIe(float needTime)
    {
        float time = needTime;
        while (time > 0)
        {
            time -= Time.deltaTime;
            NeedTimeShow(time);
            yield return null;
        }

        _dialogManager.Show(DialogType.MakingSuccessful);
        _havenMakingDrink.Add(_currentShowDrink);
        UpdateMakingDrinkShow(_currentShowDrink);
        UpdateDrinkMenuCount();
    }

    private void NeedTimeShow(float time)
    {
        timeText.text = time.ToString("f0")+" S";
    }

    // public void ShowNotice()
    // {
    //     AddAction.instance.OpenPageBySize(noticePage);
    // }
    //
    // public void UpdateNoticePagePos(Vector3 pos)
    // {
    //     noticePage.GetComponent<RectTransform>().position= pos;
    // }
    //
    // public void CloseNotice()
    // {
    //     AddAction.instance.ClosePageBySize(noticePage);
    // }
}

[Serializable]
public class CupPoint
{
    public bool isUse;
    public Transform point;

    public void SetUse(bool isUse)
    {
        this.isUse = isUse;
    }
}