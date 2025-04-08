using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActiveItem : MonoBehaviour
{
    // Tag object
    public GameObject tag;
    // Player object
    public GameObject player;

    // Initialization method
    public virtual void Start()
    {
        // Get the player object
        player = FindObjectOnWorld.instance.GetObject(FindObjectType.Player);
        // If the tag object is not null, hide the tag
        if (tag != null)
            tag.SetActive(false);
    }

    // Abstract method: around trigger event
    public abstract void OnAround();

    // Abstract method: enter trigger event
    public abstract void OnEnter();

    // Abstract method: exit trigger event
    public abstract void OnExit();

    // Abstract method: detect trigger event
    public abstract void Dete();

    // Show or hide the tag
    public void TagShow(bool isActive)
    {
        // If the tag object is not null, set the tag's active state
        if (tag != null)
        {
            tag.SetActive(isActive);
        }
    }

    // Set the tag text
    public void TagSet(string context)
    {
        // Set the tag text content
        tag.transform.GetComponentInChildren<Text>().text = context;
    }

    // Tag looks at the player
    public void TagLookAt()
    {
        // If the tag object is not null, set the tag to look at the player's camera
        if (tag != null)
        {
            tag.transform.LookAt(Camera.main.transform);
            tag.transform.eulerAngles = new Vector3(0, tag.transform.eulerAngles.y, 0);
        }
    }
}