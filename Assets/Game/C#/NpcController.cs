using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class NpcController : MonoBehaviour
{
    private NavMeshAgent _agent;
    public DeskData deskData;
    private StateController _stateController;

    public GameObject[] modes;

    //public List<Drink> drinks;
    private Drink _needDrink;

    public GameObject tipPrefab;
    public Transform tipPos;
    private GameObject tip;
    private Transform drinkSpawnOnHand;
    public UnityEvent eventsOnFalseDrink;
    public UnityEvent eventsOnTrueDrink;

    [Header("气泡")] 
    public GameObject chatPrefab;
    public List<Sprite> chatIcons;


    private void Awake()
    {
        RandomMode();
        RandomDrink();
    }

    public void OnSpawn(Vector3 posStart, Desk desk, Check check)
    {
        deskData.desk = desk;
        deskData.check = check;
        deskData.check.isUSe = true;
        Vector3 startPos = CheckPoint(posStart);
        _agent = GetComponent<NavMeshAgent>();
        _agent.Warp(startPos);
        _stateController = GetComponent<StateController>();
        _stateController.SwitchState(StateController.State.MoveToPos);
        drinkSpawnOnHand = transform.GetComponentInChildren<NpcMode>().hand;
    }

    public bool SetTarget(Vector3 pos, float minDistance)
    {
        Vector3 newPos = CheckPoint(pos);
        _agent.SetDestination(newPos);
        return Vector3.Distance(transform.position, newPos) < minDistance;
    }

    public Vector3 CheckPoint(Vector3 pos)
    {
        NavMeshHit hit;
        Vector3 newPos = Vector3.zero;
        if (NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas))
        {
            newPos = hit.position;
        }

        return newPos;
    }

    public void RandomMode()
    {
        int id = Random.Range(0, modes.Length);
        GameObject a = Instantiate(modes[id], transform);
        a.transform.localPosition = Vector3.zero;
    }

    public void RandomDrink()
    {
        List<Drink> drinks = GameManager.instance.GetComponent<LevelManager>().GetItemCanDo();
        int id = Random.Range(0, drinks.Count);
        SetDrink(drinks[id]);
    }


    public void ShowTip()
    {
        GameObject tip = Instantiate(tipPrefab);
        tip.transform.SetParent(tipPos);
        float waitDTime = GetComponent<StateController>().waitDoTime;
        float waitAskTime = GetComponent<StateController>().waitAskTime;

        tip.GetComponent<Tip>().Init(_needDrink, waitDTime, waitAskTime);
        this.tip = tip;
    }

    public void ChangeTipState(TipState state)
    {
        tip.GetComponent<Tip>().SwitchState(state);
    }


    public void CloseTip()
    {
        if (tip != null)
        {
            tip.GetComponent<Tip>().Close();
        }
    }

    public void ShowAngleTip()
    {
        if (tip != null)
        {
            tip.GetComponent<Tip>().ShowAngle();
        }
    }
    
    public void ShowChat()
    {
        if (tip != null)
        {
            tip.GetComponent<Tip>().ShowAngle();
        }
    }

    public void ShowTrueOfFalse(bool isActive)
    {
        if (tip != null)
        {
            tip.GetComponent<Tip>().ShowTrueOfFalse(isActive);
        }
    }

    public void SpawnDrinkOnHand(Drink drink)
    {
        GameObject drinkPrefab = Instantiate(drink.drinkPrefab);
        drinkPrefab.transform.SetParent(drinkSpawnOnHand);
        drinkPrefab.transform.localPosition = Vector3.zero;
        drinkPrefab.transform.localEulerAngles = Vector3.zero;
    }

    public Drink GetNeedDrink()
    {
        return _needDrink;
    }


    public void OnSentDrinkFalse()
    {
        eventsOnFalseDrink?.Invoke();
    }

    public void OnSentDrinkTrue(Drink drink)
    {
        SpawnDrinkOnHand(drink);
        eventsOnTrueDrink?.Invoke();
    }

    public void ExitCheck()
    {
        deskData.check.isUSe = false;
    }

    public void SetDrink(Drink drink)
    {
        _needDrink = drink;
    }

    public void CloseCup()
    {
        drinkSpawnOnHand.gameObject.SetActive(false);
    }
    
}

[System.Serializable]
public class DeskData
{
    public Desk desk;
    public Check check;
}