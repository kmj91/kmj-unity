using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class MapToolUIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MapToolManager mapToolManager;           // 맵툴 매니저


    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 UI 위에 올라감
        // 맵툴 매니저에게 알림
        mapToolManager.MouseUIEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 UI 에서 벗어남
        // 맵툴 매니저에게 알림
        mapToolManager.MouseUIExit();
    }


    // 드래그 시작
    public void DragStart()
    {
        mapToolManager.MouseUIDragStart();
    }

    // 드래그 끝
    public void DragEnd()
    {
        mapToolManager.MouseUIDragEnd();
    }
}
