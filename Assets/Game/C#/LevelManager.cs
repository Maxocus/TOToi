using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public List<Level> levels;
    private Level _currentLevel;
    public int _levelInt = -1;
    private DeskManager _deskManager;
    public List<Drink> drinkCanDo;
    [Header("升级界面")] public GameObject page;
    public Button openButton;
    public Text contextText;
    public Button upButton;
    private List<Desk> _canDoDesk = new List<Desk>();
    private MoneyManager _moneyManager;
    public GameObject spealilyNpc;

    private void Awake()
    {
        _deskManager = GetComponent<DeskManager>();
        openButton.onClick.AddListener(() =>
        {
            if (!page.activeSelf) AddAction.instance.OpenPageBySize(page);
            else AddAction.instance.ClosePageBySize(page);
        });
        upButton.onClick.AddListener(UpLevel);
        _moneyManager = FindObjectOnWorld.instance.GetObject(FindObjectType.Player).GetComponent<MoneyManager>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        foreach (var level in levels)
        {
            foreach (Desk desk in level.desks)
            {
                foreach (DeskObject compent in desk.allCompents)
                {
                    compent.Close();
                }
            }
        }

        UpLevel();
    }

    private void UpLevel()
    {
        if (_levelInt < levels.Count - 1)
        {
            Level nextLevel= levels[_levelInt + 1];
            if (_moneyManager.ReMoveMoney(nextLevel.needMoneyCanUp))
            {
                _levelInt++;
                _currentLevel = levels[_levelInt];
                _deskManager.ShowDeskAndCheck(_currentLevel);
                NextLevelContext();
                foreach (var item in _currentLevel.drinkCanDo)
                {
                    AddDrinkCanDo(item);
                }

                foreach (var desk in _currentLevel.desks)
                {
                    _canDoDesk.Add(desk);
                }

                if (_levelInt == 3)
                {
                    spealilyNpc.SetActive(true);
                }
                else
                {
                    spealilyNpc.SetActive(false);

                }
            }
        }
    }

    public List<Desk> GetCanDoDesk()
    {
        return _canDoDesk;
    }

    public List<Drink> GetItemCanDo()
    {
        return drinkCanDo;
    }

    private void NextLevelContext()
    {
        int id = _levelInt + 1;
        if (id <= levels.Count - 1)
        {
            LevelData levelData = new LevelData();
            levelData.level = $"下一等级为： {id + 1}";
            levelData.levelContext = $"升级条件为： 收益达到{levels[id].needMoneyCanUp}金币";
            levelData.npcCount = $"可以接待客人的数量为： {_currentLevel.desks.Count}";
            string items = "";
            foreach (var item in levels[id].drinkCanDo)
            {
                int index = levels[id].drinkCanDo.IndexOf(item);
                items += item.drinkName;
                if (index < levels[id].drinkCanDo.Count - 1)
                {
                    items += ",";
                }
            }

            levelData.itemCanDo = $"解锁可制作的物品为： {items}";

            contextText.text =
                $"{levelData.level}\n{levelData.levelContext}\n{levelData.npcCount}\n{levelData.itemCanDo}";
        }
    }

    public Level GetLevel()
    {
        return _currentLevel;
    }

    public void AddDrinkCanDo(Drink drink)
    {
        if (!drinkCanDo.Contains(drink))
            drinkCanDo.Add(drink);
    }
}

[Serializable]
public class Level
{
    public List<Desk> desks;
    public int npcMax;
    public List<Drink> drinkCanDo;
    public int needMoneyCanUp;
}

public class LevelData
{
    public string level;
    public string levelContext;
    public string npcCount;
    public string itemCanDo;
}