using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using MapToolGlobalScript;


[System.Serializable]
public class MapToolManager : MonoBehaviour
{
    public ObjectData[,,] arrMapObject;             // 맵 오브젝트 배열
    public GameObject gameStage;                    // 생성된 게임 오브젝트가 자식으로 들어갈 부모
    public GameObject guideLine;                    // 맵툴 격자
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject cameraTarget;                 // 카메라가 바라보는 곳

    private int height;                             // 오브젝트 배열의 Y축
    private int selectElement;                      // 선택된 요소
    private bool mouseFlag;

    private Camera screenCamera;                    // 메인 카메라
    private GameObject normalCube;                  // 마우스 포인터용 일반 큐브
    private GameObject createObject;                // 만들어질 게임 오브젝트
    private LayerMask layerMaskFloor;               // 레이어 마스크

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
        // Y, Z, X
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
        int iY;
        int iX;
        int iZ;
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
            objectData.color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
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
                // 현제 층과 위 아래 오브젝트 색상 변경
                updateColor();
            }
        }

        // 왼쪽 키 누르고 있음
        if (Input.GetKey(KeyCode.A))
        {
            var angle = cameraTarget.transform.eulerAngles;
            angle.y = angle.y - 1f;
            cameraTarget.transform.eulerAngles = angle;
        }

        // 오른쪽 키 누르고 있음
        if (Input.GetKey(KeyCode.D))
        {
            var angle = cameraTarget.transform.eulerAngles;
            angle.y = angle.y + 1f;
            cameraTarget.transform.eulerAngles = angle;
        }

        // 파일 내보내기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Stream ws = new FileStream("a.dat", FileMode.Create);
            BinaryFormatter serializer = new BinaryFormatter();

            serializer.Serialize(ws, arrMapObject);
            ws.Close();

            //---------------------------------
            // 전달할 다른 정보 생각해보기
            //---------------------------------
            Debug.Log("저장");
        }

        // 파일 불러오기
        if (Input.GetKeyDown(KeyCode.L))
        {
            Stream rs = new FileStream("a.dat", FileMode.Open);
            BinaryFormatter deserializer = new BinaryFormatter();

            arrMapObject = (ObjectData[,,])deserializer.Deserialize(rs);
            rs.Close();


            iY = 0;
            while (iY < 100)
            {
                iZ = 0;
                while (iZ < 10)
                {
                    iX = 0;
                    while (iX < 10)
                    {
                        switch (arrMapObject[iY, iZ, iX].objectType)
                        {
                            case MenuElementType.NORMAL_CUBE:
                                arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                                arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                                arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ);
                                arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                                break;
                        }

                        ++iX;
                    }
                    ++iZ;
                }
                ++iY;
            }
        }
        
    }


    // 현제 층과 위 아래 오브젝트 색상 변경
    private void updateColor()
    {
        int iY;
        int iX;
        int iZ;
        Color color;

        if (height - 1 >= 0)
        {
            iY = height - 1;
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY)
                    {
                        color = arrMapObject[iY, iZ, iX].color;
                        // 알파값 변경
                        arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g / 2, color.b / 2, 0.8f);
                    }
                    ++iX;
                }
                ++iZ;
            }
        }

        iY = height;
        iZ = 0;
        while (iZ < 10)
        {
            iX = 0;
            while (iX < 10)
            {
                if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY)
                {
                    // 알파값 변경
                    arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = arrMapObject[iY, iZ, iX].color;
                }
                ++iX;
            }
            ++iZ;
        }

        if (height + 1 < 100)
        {
            iY = height + 1;
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY)
                    {
                        color = arrMapObject[iY, iZ, iX].color;
                        // 알파값 변경
                        arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.r / 2, color.g / 2, color.b, 0.5f);
                    }
                    ++iX;
                }
                ++iZ;
            }
        }
    }
}
