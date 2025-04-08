using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DeskObject : MonoBehaviour
{
    public void Show()
    {
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        float size = transform.localScale.x;
        transform.localScale = new Vector3(size, size, 0);
        transform.DOScale(size, 0.5f);
    }

    public void Close()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }
    
    
}