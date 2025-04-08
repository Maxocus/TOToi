using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public float eachDayTime;
    
    public enum DayState
    {
        Null,
        Better1,
        Less1,
        Less0
    }

    private DayState _dayState = DayState.Null;
    private IEnumerator _enumerator;
    private DialogManager _dialogManager;
    public GameObject sleepPage;
    public Text stateText;
    public Text timeText;
    public Text dayText;
    public Bed _bed;
    private NpcManager _npcManager;

    private const float REAL_SECONDS_PER_GAME_SECOND = 180f;
    private const float INITIAL_REAL_TIME = 43200f; // 12小时对应的秒数
    private float _startTime;
    private int _day = 0;

    private void Awake()
    {
        _dialogManager = GetComponent<DialogManager>();
        _npcManager = GetComponent<NpcManager>();
    }

    private void Start()
    {
        GameProcessStart();
    }

    public DayState GetDayState()
    {
        return _dayState;
    }

    private void GameProcessStart()
    {
        if (_enumerator != null)
        {
            StopCoroutine(_enumerator);
        }

        _startTime = Time.time; // 记录新的开始时间
        _enumerator = GameTimeRoutine();
        StartCoroutine(_enumerator);
        _day++;
        dayText.text=$"Day {_day}";
    }

    private IEnumerator GameTimeRoutine()
    {
        // 初始状态设置（12点开始）
        _dayState = DayState.Better1;
        stateText.text = "营业中";
        stateText.color = Color.green;

        while (true)
        {
            float elapsed = Time.time - _startTime;
        
            // 更新时钟显示
            float totalRealSeconds = elapsed * REAL_SECONDS_PER_GAME_SECOND + INITIAL_REAL_TIME;
            totalRealSeconds %= 86400; // 保持在一天范围内
            UpdateClockVisuals(totalRealSeconds);

            // 获取当前游戏时间的小时数
            int currentHour = Mathf.FloorToInt(totalRealSeconds / 3600f);

            // 状态机逻辑
            switch (_dayState)
            {
                case DayState.Better1:
                    // 当到达23点时进入关店状态
                    if (currentHour >= 23)
                    {
                        _dayState = DayState.Less1;
                        _dialogManager.Show(DialogType.TimeLess1);
                        stateText.text = "关店时间";
                        stateText.color = Color.red;
                    }
                    break;

                case DayState.Less1:
                    // 当超过24点（实际显示为0点）时进入休息状态
                    if (currentHour >= 24) // 0点后的判断
                    {
                        _dayState = DayState.Less0;
                        stateText.text = "休息时间";
                        stateText.color = Color.white;
                        _dialogManager.Show(DialogType.TimeLess0);
                        Sleep();
                        yield break; // 结束协程
                    }
                    break;
            }

            yield return null;
        }
    }

    public void Sleep()
    {
        if (_enumerator != null)
        {
            AddAction.instance.BecomeBlack(sleepPage.GetComponent<Image>(), () =>
            {
                _bed.UpSleep();
                GameProcessStart(); // 重新开始游戏流程
                _npcManager.DestroyAllNpc();
            });
            StopCoroutine(_enumerator);
            _enumerator = null;
        }
    }

    void UpdateClockVisuals(float totalRealSeconds)
    {
        float totalRealMinutes = totalRealSeconds / 60f;
        float hours = totalRealMinutes / 60f; 
        float minutes = totalRealMinutes % 60f;
        int displayHours = Mathf.FloorToInt(hours);
        int displayMinutes = Mathf.FloorToInt(minutes);
        timeText.text = $"{displayHours:00}:{displayMinutes:00}";
    }
}