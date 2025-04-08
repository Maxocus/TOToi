using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeMenuButton : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public GameObject select;
    public Text countText;
    private Drink _drink;
    private int _count;
    private GameObject _countPage;

    public void Init(Drink drink)
    {
        icon.sprite = drink.icon;
        nameText.text = drink.drinkName.ToString();
        select.SetActive(false);
        _drink = drink;
        _countPage = countText.transform.parent.gameObject;
    }

    public void BeSelect()
    {
        AddAction.instance.OpenPageBySize(select);
    }

    public void BeUnSelect()
    {
        AddAction.instance.ClosePageBySize(select);
    }

    public void SetCount(int count)
    {
        _count = count;
        countText.text = _count.ToString();
        _countPage.SetActive(false);
    }

    public void AddCount()
    {
        _count += 1;
        AddAction.instance.OpenPageBySize(_countPage);
        countText.text = _count.ToString();
    }

    public Drink GetDrink()
    {
        return _drink;
    }
}