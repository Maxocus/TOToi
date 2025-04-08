using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cup : ActiveItem
{
    private Drink _drink;
    private CupPoint _cupPoint;

    public void Init(Drink drink, CupPoint cupPoint)
    {
        TagSet(drink.drinkName.ToString());
        _drink = drink;
        if (cupPoint != null)
        {
            _cupPoint = cupPoint;
            _cupPoint.SetUse(true);
            transform.position = _cupPoint.point.position;     
        }
    }

    public Drink GetDrink()
    {
        return _drink;
    }

    public override void OnAround()
    {
        TagLookAt();
    }

    public override void OnEnter()
    {
        TagShow(true);
    }

    public override void OnExit()
    {
        TagShow(false);
    }

    public override void Dete()
    {
        player.GetComponent<MakingManager>().ReMoveCupPoint(_drink, _cupPoint);
        player.GetComponent<MakingManager>().PutUpCup(this);
        ControlInspect(false);
        TagShow(false);
    }

    public void ControlInspect(bool isActive)
    {
        GetComponent<Collider>().enabled = isActive;
    }

    // Update is called once per frame
    void Update()
    {
    }
}