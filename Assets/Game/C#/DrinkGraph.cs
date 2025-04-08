using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrinkGraph : MonoBehaviour
{
    public Image image;
    public Text nameText;
    public Text moneyText;

    public void Init(ItemBase itemBase)
    {
        image.sprite = itemBase.icon;
        nameText.text = itemBase.itemName.ToString();
        moneyText.text = itemBase.money.ToString();
    }
    
}
