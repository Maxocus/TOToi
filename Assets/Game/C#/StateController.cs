using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class StateController : MonoBehaviour
{
    public enum State
    {
        Idle,
        MoveToPos,
        Sit,
        WaitAsk,
        WaitDo,
        Drink,
        ExitSit,
        MoveToHome,
        Angle,
    }

    public State currentState;
    private NpcController _npcController;
    private DeskData _deskData;
    public Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Vector3 exitSitPos;
    private Vector3 _startPos;
    private float _time;
    private NpcActive _npcActive;
    private GameObject player;

    [Header("时间")] public float waitAskTime;
    public float waitDoTime;
    public float drinkTime;

    public void SwitchState(State state)
    {
        _animator = transform.GetChild(1).GetComponentInChildren<Animator>();
        if (currentState == state) return;
        StateExit();
        currentState = state;
        StateEnter();
    }

    private void Awake()
    {
        _npcController = GetComponent<NpcController>();
        SwitchState(State.Idle);
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _npcActive = GetComponent<NpcActive>();
        player = FindObjectOnWorld.instance.GetObject(FindObjectType.Player);
    }

    private void Update()
    {
        StateUpdate();
    }

    private void StateEnter()
    {
        switch (currentState)
        {
            case State.Idle:
                break;
            case State.MoveToPos:
                _startPos = transform.position;
                _navMeshAgent.enabled = true;
                _deskData = _npcController.deskData;
                _animator.SetBool("Walk", true);
                break;
            case State.Sit:
                _animator.SetBool("Sit", true);
                _navMeshAgent.enabled = false;
                exitSitPos = transform.position;
                transform.DOMove(_deskData.check.check.position, 1f);
                transform.DORotate(_deskData.check.check.eulerAngles, 1f).OnComplete(() =>
                {
                    SwitchState(State.WaitAsk);
                });
                break;
            case State.WaitAsk:
                _time = waitAskTime;
                _npcActive.inspect.AddListener(() => { SwitchState(State.WaitDo); });
                _npcController.ShowTip();
                _npcController.ChangeTipState(TipState.点单等待);
                break;
            case State.WaitDo:
                _npcActive.inspect.AddListener(() => { player.GetComponent<MakingManager>().GiveCup(_npcController); });
                _npcController.eventsOnFalseDrink.AddListener(() => { SwitchState(State.ExitSit); });
                _npcController.eventsOnTrueDrink.AddListener(() => { SwitchState(State.Drink); });
                _time = waitDoTime;
                _npcController.ChangeTipState(TipState.制作等待);

                break;
            case State.Drink:
                _animator.SetBool("Drink", true);
                int random = Random.Range(2, 6);
                _time = drinkTime * random;
                _npcController.ChangeTipState(TipState.品尝中);
                break;
            
            case State.Angle:
                _npcController.ShowAngleTip();
                AddAction.instance.DelayPlay(() => { SwitchState(State.ExitSit); }, 3f);
                break;
            
            case State.ExitSit:
                _animator.SetBool("Sit", false);
                transform.DOMove(exitSitPos, 1f).OnComplete(() => { SwitchState(State.MoveToHome); });
                break;
            case State.MoveToHome:
                _npcController.CloseTip();
                _navMeshAgent.enabled = true;
                _npcController.ExitCheck();
                _animator.SetBool("Walk", true);
                break;
        }
    }

    private void StateUpdate()
    {
        switch (currentState)
        {
            case State.Idle:

                break;
            case State.MoveToPos:
                Vector3 pos = _deskData.check.check.transform.position;
                if (_npcController.SetTarget(pos, 0.3f))
                {
                    SwitchState(State.Sit);
                }

                break;
            case State.Sit:
                break;
            case State.WaitAsk:
                _time -= Time.deltaTime;
                if (_time <= 0)
                {
                    SwitchState(State.Angle);
                }

                break;
            case State.WaitDo:
                _time -= Time.deltaTime;
                if (_time <= 0)
                {
                    SwitchState(State.Angle);
                }

                break;
            case State.Drink:
                _time -= Time.deltaTime;
                if (_time <= 0)
                {
                    SwitchState(State.ExitSit);
                }

                break;
            
            case State.Angle:
                break;
            
            case State.ExitSit:

                break;
            case State.MoveToHome:
                pos = _startPos;
                if (_npcController.SetTarget(pos, 0.3f))
                {
                    GameManager.instance.GetComponent<NpcManager>().RemoveNpc(_npcController);
                    Destroy(gameObject);
                }

                break;
        }
    }

    private void StateExit()
    {
        switch (currentState)
        {
            case State.Idle:

                break;
            case State.MoveToPos:
                _animator.SetBool("Walk", false);

                break;
            case State.Sit:

                break;
            case State.WaitAsk:
                _npcActive.inspect.RemoveAllListeners();
                break;
            case State.WaitDo:
                _npcActive.inspect.RemoveAllListeners();
                _npcController.eventsOnFalseDrink.RemoveAllListeners();
                _npcController.eventsOnTrueDrink.RemoveAllListeners();

                break;
            case State.Drink:
                _animator.SetBool("Drink", false);
                player.GetComponent<MoneyManager>().AddMoney(_npcController.GetNeedDrink().money);
                _npcController.CloseCup();
                break;
            
            case State.Angle:
                break;
            
            case State.ExitSit:
                break;
            case State.MoveToHome:

                break;
        }
    }
}