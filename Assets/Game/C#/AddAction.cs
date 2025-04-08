using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddAction : MonoBehaviour
{
    public static AddAction instance; 
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void DelayPlay(Action action, float delay)
    {
        StartCoroutine(DelayPlayIEnumerator(action, delay));
    }

    private IEnumerator DelayPlayIEnumerator(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public void OpenPageBySize(GameObject page)
    {
        page.transform.DOKill();
        page.SetActive(true);
        page.transform.localScale=new Vector3(0,0,0);
        page.transform.DOScale(1, 0.5f);
    }

    public void ClosePageBySize(GameObject page)
    {
        page.transform.DOKill();
        page.transform.DOScale(0, 0.5f).OnComplete(()=>{page.SetActive(false);});
    }

    public void BecomeBlack(Image image, Action action = null)
    {
        image.gameObject.SetActive(true);
        image.color = new Color(1, 1, 1, 0);
        image.DOColor(Color.black, 1).OnComplete(() =>
        {
            AddAction.instance.DelayPlay(() =>
            {
                action?.Invoke();
                image.DOColor(new Color(1, 1, 1, 0), 1).OnComplete(() =>
                {
                    image.gameObject.SetActive(false);
                });
            }, 1f);
        });

    }
}
