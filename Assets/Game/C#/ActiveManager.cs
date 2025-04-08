using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveManager : MonoBehaviour
{
    public Transform point;
    public float radius;
    public List<ActiveItem> activeItem = new List<ActiveItem>();
    private ActiveItem _itemCurr;
    public Camera camera;


    private void Start()
    {
    }

    private void Update()
    {
        DeteMine();
        PickUp();
    }

    public void DeteMine()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("ActiveItem"));
        ActiveItem item = null;
        if (hit.collider != null)
        {
            item = hit.collider.GetComponent<ActiveItem>();
        }
        if (item != _itemCurr&&!EventSystem.current.IsPointerOverGameObject())
        {
            _itemCurr?.OnExit();
            _itemCurr = item;
            _itemCurr?.OnEnter();
        }
    }

    private void PickUp()
    {
        if (Input.GetMouseButtonUp(0)&&!EventSystem.current.IsPointerOverGameObject())
        {
            if (_itemCurr != null)
            {
                if (Vector3.Distance(_itemCurr.transform.position, transform.position) < 2)
                {
                    _itemCurr.Dete();     
                }
                else
                {
                    GameManager.instance.GetComponent<DialogManager>().Show(DialogType.DistanceLong);
                }
               
            }
        }
    }

    public void DestroyItem(GameObject a, float delayTime)
    {
        Destroy(a, delayTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point.position, radius);
    }
}