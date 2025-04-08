using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Drink", menuName = "MakingSystem/Drink")]
public class Drink : ScriptableObject
{
    public enum  DrinkName
    {
        苏打水,
        精酿啤酒,
        威士忌,
        伏特加,
        银朗姆酒,
        莫吉托,
        玛格丽特,
        干马天尼,
        正山小种,
        祁门红茶,
        医疗包
    }
    public DrinkName drinkName;
    public List<ItemBase> compose;
    public Sprite icon;
    public GameObject drinkPrefab;
    public int money;
    public string context;
    public float needTime;
}

