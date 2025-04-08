using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DeskManager : MonoBehaviour
{
    private LevelManager _levelManager;

    private void Awake()
    {
        _levelManager = GetComponent<LevelManager>();
    }

    public Desk ByCheckFindDesk(Check check)
    {
        Level level = _levelManager.GetLevel();
        Desk deskTarget = null;
        foreach (Desk desk in level.desks)
        {
            if (desk.checkPoints.Contains(check))
            {
                deskTarget = desk;
                return deskTarget;
            }
        }

        return null;
    }

    public Check GetCheck()
    {
        List<Desk> deskCanDo = _levelManager.GetCanDoDesk();
        List<Check> availableChecks = new List<Check>();

        foreach (Desk desk in deskCanDo)
        {
            foreach (Check check in desk.checkPoints)
            {
                if (!check.isUSe)
                {
                    availableChecks.Add(check);
                }
            }
        }

        if (availableChecks.Count > 0)
        {
            int randomIndex = Random.Range(0, availableChecks.Count);
            return availableChecks[randomIndex];
        }

        return null;
    }

    public void ShowDeskAndCheck(Level level)
    {
        foreach (Desk desk in level.desks)
        {
            foreach (DeskObject compent in desk.allCompents)
            {
                compent.Show();
            }
        }
    }
}

[Serializable]
public class Desk
{
    public Transform deskPoint;
    public List<Check> checkPoints;
    public List<DeskObject> allCompents;
}

[Serializable]
public class Check
{
    public Transform check;
    public bool isUSe = false;
}