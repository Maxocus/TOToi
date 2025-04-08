using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rubbish : ActiveItem
{
    public override void OnAround()
    {
       
    }

    public override void OnEnter()
    {
        TagShow(true);
        TagSet("垃圾桶");
    }

    public override void OnExit()
    {
        TagShow(false);

    }

    public override void Dete()
    {
        player.GetComponent<MakingManager>().GiveToRubbish();
    }
}
