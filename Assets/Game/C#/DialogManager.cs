using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public enum DialogType
{
   BuySuccessful,
   BuyFail,
   MakingSuccessful,
   MakingFail,
   MakingDrinkNoDo,
   TimeLess1,
   TimeLess0,
   SpecialNpc1,
   SpecialNpc2,
   MakingDrinkNoPosition,
   DistanceLong,
}

public class DialogManager : MonoBehaviour
{
    [Header("对话UI")] public GameObject dialogUI;
    public Text contentText;
    public List<DialogGroup> myDialogGroups;
    [HideInInspector] public DialogGroup _currentDialogGroup;
    public Button nextButton;
    public Text humanText;
    public bool useName;


    public virtual void Start()
    {
        nextButton.onClick.AddListener(Next);
    }

    public void Show(DialogType type)
    {
        dialogUI.SetActive(true);
        foreach (var myDialogGroup in myDialogGroups)
        {
            if (myDialogGroup.type == type)
            {
                _currentDialogGroup = myDialogGroup;
                _currentDialogGroup.Init();
            }
        }

        TextPlay(_currentDialogGroup.currentDialog.content);
        if (useName)
            humanText.text = _currentDialogGroup.currentDialog.humanName;
    }

    public void Next()
    {
        if (!_currentDialogGroup.Next())
        {
            TextPlay(_currentDialogGroup.currentDialog.content);
            if (useName)
                humanText.text = _currentDialogGroup.currentDialog.humanName;
        }
        else
        {
            dialogUI.SetActive(false);
            if (_currentDialogGroup.events == null)
            {
            }

            _currentDialogGroup.events?.Invoke();
        }
    }

    public DialogGroup GetDialogGroup(DialogType type)
    {
        foreach (var myDialogGroup in myDialogGroups)
        {
            if (myDialogGroup.type == type)
            {
                return myDialogGroup;
            }
        }

        return null;
    }

    private string fullText;
    private int currentIndex = 0;
    public float typingSpeed = 0.1f;
    private IEnumerator iEType;

    private void TextPlay(string context)
    {
        
        fullText = context;
        contentText.text = "";
        if (iEType != null)
        {
            StopCoroutine((iEType));
        }

        iEType = TypeText();
        StartCoroutine(iEType);
    }

    IEnumerator TypeText()
    {
        foreach (char c in fullText)
        {
            contentText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }


    public void AddEvent(DialogType type, UnityAction action)
    {
        DialogGroup dialogGroup = GetDialogGroup(type);
        if (dialogGroup != null)
        {
            dialogGroup.events.AddListener(action);
        }
        else
        {
            Debug.Log("NUll");
        }
    }
}

[System.Serializable]
public class DialogGroup
{
    public DialogType type;
    public int _currentDialogInt = 0;
    [HideInInspector] public Dialog currentDialog;
    public List<Dialog> myDialogs;
    public UnityEvent events;

    public void Init()
    {
        _currentDialogInt = 0;
        currentDialog = myDialogs[_currentDialogInt];
    }

    public bool Next()
    {
        _currentDialogInt++;
        if (_currentDialogInt > myDialogs.Count - 1)
        {
            return true;
        }
        else
        {
            currentDialog = myDialogs[_currentDialogInt];
            return false;
        }
    }
}

[System.Serializable]
public class Dialog
{
    public string humanName;
    public string content;
}