using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InventorySample : MonoBehaviour
{
    public InventoryManager inventoryManager;
    [FormerlySerializedAs("item")] public ItemBase drink;
    // Start is called before the first frame update
    void Start()
    {
        inventoryManager.RemoveItem(drink);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
