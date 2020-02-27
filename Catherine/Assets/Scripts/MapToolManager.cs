using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MapToolGlobalScript;

public class MapToolManager : MonoBehaviour
{
    public GameObject pointerCubePrefab;

    private int selectElement;                      // 선택된 요소
    private int[,,] arrMapObject;                   // 맵 오브젝트 배열
    private bool mouseFlag;

    private Camera screenCamera;
    private GameObject pointerCube;
    private GameObject createObject;
    private LayerMask layerMaskFloor;

    public void MousePointerChange(int element)
    {
        mouseFlag = true;
        selectElement = element;

        // 선택된 요소
        switch ((MenuElementType)element)
        {
            case MenuElementType.NORMAL_CUBE:
                // 노말 큐브
                pointerCube.SetActive(true);
                createObject = pointerCube;
                break;
        }
        
    }


    private void Start()
    {
        arrMapObject = new int[100, 10, 10];

        mouseFlag = true;

        screenCamera = Camera.main;

        pointerCube = Instantiate<GameObject>(pointerCubePrefab);
        pointerCube.SetActive(false);

        // 레이어 마스크
        layerMaskFloor = 1 << LayerMask.NameToLayer("Floor");
    }


    private void Update()
    {


        if (mouseFlag)
        {
            MouseProc();
        }


        if (Input.GetMouseButton(0))
        {
            if (createObject == null)
            {
                return;
            }

            var position = createObject.transform.position;

            //-------------------------------------------------------
            // 배열이 생성된 오브젝트에 대한 정보를 좀더 알아야될것 같음
            // int에서 구조체 배열로 바꿀것
            //-------------------------------------------------------
            // 배열에 생성
            arrMapObject[(int)position.y, (int)position.z, (int)position.x] = 1;

            //createObject.SetActive(false);
            //createObject = null;
        }
    }


    // 마우스 처리
    private void MouseProc()
    {
        RaycastHit rayHit;          // 레이 충돌한 물체
        Vector3 createPoint;

        // 만들게 없다
        if (createObject == null)
        {
            return;
        }

        // 화면 마우스 좌표
        var screenMouse = Input.mousePosition;
        /// 변환
        var world = screenCamera.ScreenToWorldPoint(screenMouse);

        // 레이
        if (!Physics.Raycast(world, screenCamera.transform.forward, out rayHit, 20f, layerMaskFloor))
        {
            return;
        }

        // 반올림
        createPoint.x = Mathf.Round(rayHit.point.x);
        createPoint.y = Mathf.Round(rayHit.point.y) + 0.5f;
        createPoint.z = Mathf.Round(rayHit.point.z);

        createObject.transform.position = createPoint;

        
    }



}
