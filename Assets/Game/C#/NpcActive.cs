using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class NpcActive : ActiveItem
{
    public UnityEvent inspect=new UnityEvent();

    public override void OnAround()
    {
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
       
    }

    public override void Dete()
    {
        inspect?.Invoke();
    }
    
}