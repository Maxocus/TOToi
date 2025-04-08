using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindObjectOnWorld : MonoBehaviour
{
    public static FindObjectOnWorld instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public List<FindObject> findObjects;

    public GameObject GetObject(FindObjectType findObjectType)
    {
        GameObject a = null;
        foreach (var findObject in findObjects)
        {
            if (findObject.findObjectType == findObjectType)
            {
                a = findObject.gameObject;
            }
        }

        return a;
    }

    public void Init()
    {
        foreach (var findObject in findObjects)
        {
            if (findObject.findObjectType == FindObjectType.Player) continue;
            findObject.gameObject.SetActive(false);
        }
    }

    public List<FindObject> GetAllFindObjects()
    {
        return findObjects;
    }
}

public enum FindObjectType
{
    Player,
    StorePage,
    StoreItemParen,t
}

[Serializable]
public class FindObject
{
    public FindObjectType findObjectType;
    public GameObject gameObject;
}