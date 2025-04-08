using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu( fileName = "newInventory", menuName = "InventorySystem/Inventory")]
public class InventoryBase :ScriptableObject
{
    public List<ItemPlate> allItemPlates;

}

[System.Serializable]
public class ItemPlate
{
    public string itemName;
    [FormerlySerializedAs("item")] public ItemBase drink;
    public int count;

    public ItemPlate(ItemBase drink)
    {
        this.drink=drink;
        count=1;
        itemName=drink.name;
    }
    
}
