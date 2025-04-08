using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventoryBase inventory;
    public InventoryUIBase uiManager;

    public void AddItem(ItemBase drink)
    {
        bool isHave = false;
        foreach (var itemPlate in inventory.allItemPlates)
        {
            if (itemPlate.drink == drink)
            {
                itemPlate.count++;
                isHave = true;
            }
        }

        if (!isHave)
            inventory.allItemPlates.Add(new ItemPlate(drink));

        //uiManager.RefreshUI(inventory);
    }

    public void RemoveItem(ItemBase drink)
    {
        foreach (var itemPlate in inventory.allItemPlates)
        {
            if (itemPlate.drink == drink)
                itemPlate.count--;

            if (itemPlate.count <= 0)
            {
                inventory.allItemPlates.Remove(itemPlate);
                return;
            }
        }

        //uiManager.RefreshUI(inventory);
    }

    public ItemPlate InspectIsHave(ItemBase drink)
    {
        return inventory.allItemPlates.Find(x => x.drink == drink);
    }
}