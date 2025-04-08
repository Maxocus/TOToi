using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "InventorySystem/Inventory")]
public class ItemBase : ScriptableObject
{
    public enum ItemName
    {
        小苏打,
        纯净水,
        麦芽,
        大麦芽,
        啤酒花,
        酵母,
        玉米,
        糖浆,
        甘蔗汁,
        冰块,
        青柠,
        薄荷叶,
        盐,
        龙舌兰,
        君度,
        金酒,
        干味美思,
        柠檬,
        橄榄,
        正山小种茶叶,
        祁红茶叶,
    }

    public ItemName itemName;
    public int money;
    public Sprite icon;
}