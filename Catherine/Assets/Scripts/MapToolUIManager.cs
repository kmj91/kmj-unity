using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class MapToolUIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, 
                                IPointerUpHandler, IPointerClickHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public MapToolManager mapToolManager;           // 맵툴 매니저


    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 UI 위에 올라감
        mapToolManager.OnMouseUIEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 UI 에서 벗어남
        mapToolManager.OnMouseUIExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("pointer down");
        //Debug.Log(eventData.pointerEnter);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("pointer up");
        Debug.Log(eventData.pointerPress);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click");
        //Debug.Log(eventData.pointerEnter);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("drag");
        //transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log("drop");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("end drag");
    }

    
}
