using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryUIBase : MonoBehaviour
{
    public abstract void RefreshUI(InventoryBase inventory);
}