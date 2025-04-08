using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public GameObject selectPage;
    public List<Button> selectButton;
    private List<Action> actions= new List<Action>();

    private void Awake()
    {
        actions.Add(null);
        actions.Add(null);
        actions[0]+= () =>
        {
            AddAction.instance.ClosePageBySize(selectPage);
        };
        actions[1]+= () =>
        {
            AddAction.instance.ClosePageBySize(selectPage);
        };
        selectButton[0].onClick.AddListener(() => { actions[0]?.Invoke(); });
        selectButton[1].onClick.AddListener(() => { actions[1]?.Invoke(); });
    }

    public void ShowSelect()
    {
        AddAction.instance.OpenPageBySize(selectPage);
    }

    public void RegisterAction(int id, Action action)
    {
        actions[id] += action;
    }
}