using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpecialNpc : ActiveItem
{
    public enum State
    {
        Null,
        GoToStore,
        WaitChat,
        Chat,
        WaitHelp,
        Help,
        GoBack,
    }

    public State state;
    private NavMeshAgent _agent;
    private ChatManager _chatManager;
    private DialogManager _dialogManager;
    public Transform storePoint;
    public Transform backPoint;
    private bool _isActive;
    private Animator _animator;
    private MakingManager _makingManager;
    private LevelManager _levelManager;
    public Drink needDrink;
    public List<Drink> drinkCanDo;

    private void Awake()
    {
        
    }

    public new void Start()
    {
        _levelManager = GameManager.instance.GetComponent<LevelManager>();
        _dialogManager = GameManager.instance.GetComponent<DialogManager>();
        _agent = GetComponent<NavMeshAgent>();
       
        _animator = GetComponentInChildren<Animator>();
        _chatManager = FindObjectOnWorld.instance.GetObject(FindObjectType.Player).GetComponent<ChatManager>();
        _makingManager= FindObjectOnWorld.instance.GetObject(FindObjectType.Player).GetComponent<MakingManager>();
        _dialogManager.AddEvent(DialogType.SpecialNpc1, () => { _chatManager.ShowSelect(); });
        _dialogManager.AddEvent(DialogType.SpecialNpc2, () =>
        {
            SwitchState(State.GoBack);
            foreach (var drinks in drinkCanDo)
            {
                _levelManager.AddDrinkCanDo(drinks);
            }
        });
        _chatManager.RegisterAction(0, () => { SwitchState(State.WaitHelp); });
        _chatManager.RegisterAction(1, () => { SwitchState(State.GoBack); });
        SwitchState(State.GoToStore);
        TagSet("东方客商");
    }

    private void SwitchState(State state)
    {
        StateExit();
        this.state = state;
        StateEnter();
    }

    private void Update()
    {
        TagLookAt();
        StateUpdate();
    }

    private void StateEnter()
    {
        switch (state)
        {
            case State.GoToStore:
                _animator.SetBool("Walk", true);
                break;
            case State.WaitChat:

                break;
            case State.Chat:
                _dialogManager.Show(DialogType.SpecialNpc1);
                break;
            case State.WaitHelp:
                break;
            case State.Help:
                _makingManager.GiveSaveBag(this);
                _dialogManager.Show(DialogType.SpecialNpc2);
                break;
            case State.GoBack:
                _animator.SetBool("Walk", true);
                break;
        }
    }

    private void StateUpdate()
    {
        switch (state)
        {
            case State.GoToStore:
                if (SetTarget(storePoint.transform.position, 0.5f))
                {
                    SwitchState(State.WaitChat);
                }
                break;
            case State.WaitChat:
                if (_isActive)
                {
                    SwitchState(State.Chat);
                }
                break;
            case State.Chat:
                break;
            case State.WaitHelp:
                if (_isActive)
                {
                    SwitchState(State.Help);
                }

                break;
            case State.Help:
                break;
            case State.GoBack:
                if (SetTarget(backPoint.transform.position, 0.5f))
                {
                    Destroy(gameObject);
                }

                break;
        }
    }

    private void StateExit()
    {
        switch (state)
        {
            case State.GoToStore:
                _animator.SetBool("Walk", false);
                break;
            case State.WaitChat:
                _isActive = false;
                break;
            case State.Chat:
                break;
            case State.WaitHelp:
                _isActive = false;
                break;
            case State.Help:
                break;
            case State.GoBack:
                _animator.SetBool("Walk", false);

                break;
        }
    }

    public bool SetTarget(Vector3 pos, float minDistance)
    {
        Vector3 newPos = CheckPoint(pos);
        _agent.SetDestination(newPos);
        return Vector3.Distance(transform.position, newPos) < minDistance;
    }

    private Vector3 CheckPoint(Vector3 pos)
    {
        NavMeshHit hit;
        Vector3 newPos = Vector3.zero;
        if (NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas))
        {
            newPos = hit.position;
        }

        return newPos;
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
        _isActive = true;
    }
}