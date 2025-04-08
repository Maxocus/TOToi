using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : ActiveItem
{
    public Transform sleepPoint;
    public Transform sleepOverPoint;
    public override void OnAround()
    {
        TagLookAt();
    }

    public override void OnEnter()
    {
        TagSet("床");
        TagShow(true);
    }

    public override void OnExit()
    {
        TagShow(false);
    }

    public override void Dete()
    {
        player.GetComponent<PlayerControl>().Sleep(sleepPoint);
        GameManager.instance.GetComponent<TimeManager>().Sleep();
    }

    public void UpSleep()
    {
        player.GetComponent<PlayerControl>().UpSleep(sleepOverPoint);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
