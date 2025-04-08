using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBag : Cup
{
    public Drink drink;
    public override void Start()
    {
        base.Start();
        Init(drink, null);
    }
    
}
