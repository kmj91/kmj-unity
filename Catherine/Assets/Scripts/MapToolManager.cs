using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MapToolGlobalScript;

public class MapToolManager : MonoBehaviour
{

    public GameObject gameStage;
    public GameObject guideLine;
    public GameObject normalCubePrefab;

    private int height;
    private int selectElement;                      // 선택된 요소
    private ObjectData[,,] arrMapObject;            // 맵 오브젝트 배열
    private bool mouseFlag;

    private Camera screenCamera;
    private GameObject normalCube;
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
                normalCube.SetActive(true);
                createObject = normalCube;
                break;
        }
        
    }


    private void Start()
    {
        // 오브젝트 배열 생성
        arrMapObject = new ObjectData[100, 10, 10];

        // 노말 큐브 프리팹
        normalCube = Instantiate<GameObject>(normalCubePrefab);
        normalCube.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        normalCube.SetActive(false);

        screenCamera = Camera.main;

        // 레이어 마스크
        layerMaskFloor = 1 << LayerMask.NameToLayer("Floor");
    }


    private void Update()
    {


        if (mouseFlag)
        {
            MouseProc();
        }

        // 키처리
        KeyProc();
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

        Debug.DrawRay(world, screenCamera.transform.forward * 20f, Color.red);

        // 반올림, 올림
        createPoint.x = Mathf.Round(rayHit.point.x);
        createPoint.y = Mathf.Ceil(rayHit.point.y);
        createPoint.z = Mathf.Round(rayHit.point.z);

        createObject.transform.position = createPoint;

        var position = createObject.transform.position;
        // 비어있음
        if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType == MenuElementType.EMPTY)
        {
            // 만들 수 있음 초록색
            createObject.GetComponent<MeshRenderer>().material.color = new Color(0, 200, 0, 0.1f);
        }
        else
        {
            // 만들 수 없음 빨간색
            createObject.GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.1f);
        }
    }


    // 키 처리
    private void KeyProc()
    {
        ObjectData objectData;

        if (Input.GetMouseButton(0))
        {
            if (createObject == null)
            {
                return;
            }

            var position = createObject.transform.position;


            if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
            {
                return;
            }

            // 오브젝트 생성
            //switch

            objectData.objectType = MenuElementType.NORMAL_CUBE;
            objectData.gameObject = Instantiate<GameObject>(normalCubePrefab);
            objectData.gameObject.transform.position = position;
            objectData.gameObject.transform.parent = gameStage.transform;

            arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;

            //createObject.SetActive(false);
            //createObject = null;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (height + 1 < 100)
            {
                ++height;
                guideLine.transform.position = new Vector3(0f, height, 0f);
                // 카메라 위로
                screenCamera.transform.position = screenCamera.transform.position + new Vector3(0f, 1f, 0f);
                // 현제 층과 위 아래 오브젝트 색상 변경
                updateColor();
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (height - 1 >= 0)
            {
                --height;
                guideLine.transform.position = new Vector3(0f, height, 0f);
                // 카메라 아래로
                screenCamera.transform.position = screenCamera.transform.position + new Vector3(0f, -1f, 0f);
            }
        }
    }


    // 현제 층과 위 아래 오브젝트 색상 변경
    private void updateColor()
    {
        int iY;
        int iX;
        int iZ;

        iY = height - 1;
        //while()

        //---------------------------------------------------------
        // 오브젝트 배열돌면서 해당 배열안에 있는 오브젝트들
        // 스크립트 만들어서 호출해서 바꿔야
        //---------------------------------------------------------

        // 알파값 변경
        //var color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
        //color.a = 0.5f;
        //objectData.gameObject.GetComponent<MeshRenderer>().material.color = color;

    }
}
