using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcManager : MonoBehaviour
{
    public List<GameObject> npcPrefab;
    public Transform spawnPoint;
    public float interSpawnTime;
    private float _time;
    private Level _currentLevel;
    private List<NpcController> _allNpc = new List<NpcController>();
    private DeskManager _deskManager;
    private LevelManager _levelManager;
    private TimeManager _timeManager;

    private void Start()
    {
        _deskManager = GameManager.instance.GetComponent<DeskManager>();
        _levelManager = GameManager.instance.GetComponent<LevelManager>();
        _time = interSpawnTime;
        _timeManager= GameManager.instance.GetComponent<TimeManager>();
    }

    private void SpawnNpc()
    {
        int index = Random.Range(0, npcPrefab.Count);
        GameObject npc = Instantiate(npcPrefab[index]);
        NpcController npcController = npc.GetComponent<NpcController>();
        _allNpc.Add(npc.GetComponent<NpcController>());
        Check check = _deskManager.GetCheck();
        Desk desk = _deskManager.ByCheckFindDesk(check);
        npcController.OnSpawn(spawnPoint.position, desk, check);
    }

    private void SpawnTimePlan()
    {
        _time -= Time.deltaTime;

        if (_time <= 0&&_timeManager.GetDayState()==TimeManager.DayState.Better1)
        {
            _currentLevel = _levelManager.GetLevel();
            if (_allNpc.Count < _currentLevel.npcMax)
            {
                SpawnNpc();
                _time = interSpawnTime;
            }
        }
    }

    public void DestroyAllNpc()
    {
        foreach (var npc in _allNpc)
        {
            npc.ExitCheck();
            Destroy(npc.gameObject);
        }
        _allNpc.Clear();
        
    }


    public void RemoveNpc(NpcController npc)
    {
        _allNpc.Remove(npc);
    }

    private void Update()
    {
        SpawnTimePlan();
    }
}