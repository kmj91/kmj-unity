using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class UITitleBar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IDragHandler
{
    public GameObject targetMenu;               // 타이틀바가 소속된 메뉴
    public MapToolUIManager mapToolUIManager;   // 맵툴 UI 매니저

    private Vector2 dragPostion;                // 드래그 중 이전 위치 값
    private RectTransform rectTransform;        // 메뉴 사각 트랜스폼


    // 다운
    public void OnPointerDown(PointerEventData eventData)
    {
        // 드래그를 위한 위치값 저장
        dragPostion = eventData.pressPosition;
        // 드래그 시작
        mapToolUIManager.DragStart();
    }

    // 업
    public void OnPointerUp(PointerEventData eventData)
    {
        // 드래그 끝
        mapToolUIManager.DragEnd();
    }

    // 드래그
    public void OnDrag(PointerEventData eventData)
    {
        // 이전 위치에서 현제 위치를 빼서 이동한 위치 값 구함
        Vector2 moveVector = eventData.position - dragPostion;
        // 이전 위치 값을 현제 위치 값으로 갱신
        dragPostion = eventData.position;
        // 메뉴 이동
        rectTransform.anchoredPosition = rectTransform.anchoredPosition + moveVector;
    }


    private void Start()
    {
        rectTransform = targetMenu.GetComponent<RectTransform>();
    }

    
}
