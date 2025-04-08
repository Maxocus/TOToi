using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakingPos : ActiveItem
{
    public override void Start()
    {
        base.Start();
        TagSet("制作位置");
    }

    public override void OnAround()
    {
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
        player.GetComponent<MakingManager>().OpenPage();
    }
}