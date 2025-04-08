using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBag : InventoryManager
{
    public List<ItemBase> itemsInpects;

    public void Start()
    {
        inventory.allItemPlates.Clear();
        foreach (var item in itemsInpects)
        {
            AddItem(item);
        }
    }
}