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
    public GameObject playerPrefab;                 // 플레이어 프리팹
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject iceCubePrefab;                // 얼음 큐브 프리팹
    public GameObject cameraTarget;                 // 카메라가 바라보는 곳

    private int height;                             // 오브젝트 배열의 Y축
    private int selectElement;                      // 선택된 요소
    private bool mouseFlag;

    private Camera screenCamera;                    // 메인 카메라
    private GameObject player;                      // 선택 영역 표시용 플레이어
    private GameObject normalCube;                  // 선택 영역 표시용 일반 큐브
    private GameObject iceCube;                     // 선택 영역 표시용 얼음 큐브
    private GameObject createObject;                // 만들어질 게임 오브젝트
    private GameObject selectField;                 // 선택 영역
    private LayerMask layerMaskFloor;               // 레이어 마스크

    public void MousePointerChange(int element)
    {
        
        selectElement = element;

        

        // 선택된 요소
        switch ((MenuElementType)element)
        {
            case MenuElementType.EMPTY:
                // 빈 상태
                // Update에서 마우스 처리 false
                mouseFlag = false;
                // 선택 영역
                selectField.SetActive(false);
                // 없음
                player.SetActive(false);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = null;
                break;
            case MenuElementType.PLAYER:
                // 플레이어
                // Update에서 마우스 처리 true
                mouseFlag = true;
                // 선택 영역
                selectField.SetActive(true);
                // 플레이어 보이기
                player.SetActive(true);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = player;
                break;
            case MenuElementType.NORMAL_CUBE:
                // 노말 큐브
                // Update에서 마우스 처리 true
                mouseFlag = true;
                // 선택 영역
                selectField.SetActive(true);
                // 노말 큐브 보이기
                player.SetActive(false);
                normalCube.SetActive(true);
                iceCube.SetActive(false);
                createObject = normalCube;
                break;
            case MenuElementType.ICE_CUBE:
                // 얼음 큐브
                // Update에서 마우스 처리 true
                mouseFlag = true;
                // 선택 영역
                selectField.SetActive(true);
                // 얼음 큐브 보이기
                player.SetActive(false);
                normalCube.SetActive(false);
                iceCube.SetActive(true);
                createObject = iceCube;
                break;
        }
        
    }


    private void Start()
    {
        // 오브젝트 배열 생성
        // Y, Z, X
        arrMapObject = new ObjectData[100, 10, 10];

        // 선택 영역
        selectField = Instantiate<GameObject>(normalCubePrefab);
        selectField.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        selectField.name = "SelectField";
        selectField.SetActive(false);

        // 선택 영역 표시용 오브젝트
        player = Instantiate<GameObject>(playerPrefab);
        player.transform.parent = selectField.transform;
        player.SetActive(false);
        normalCube = Instantiate<GameObject>(normalCubePrefab);
        normalCube.transform.parent = selectField.transform;
        normalCube.SetActive(false);
        iceCube = Instantiate<GameObject>(iceCubePrefab);
        iceCube.transform.parent = selectField.transform;
        iceCube.SetActive(false);

        


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

        selectField.transform.position = createPoint;

        var position = selectField.transform.position;
        // 비어있음
        if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType == MenuElementType.EMPTY)
        {
            // 만들 수 있음 초록색
            selectField.GetComponent<MeshRenderer>().material.color = new Color(0, 200, 0, 0.1f);
        }
        else
        {
            // 만들 수 없음 빨간색
            selectField.GetComponent<MeshRenderer>().material.color = new Color(200, 0, 0, 0.1f);
        }
    }


    // 키 처리
    private void KeyProc()
    {
        ObjectData objectData;


        // 마우스 왼쪽 클릭
        if (Input.GetMouseButton(0))
        {
            if (createObject == null)
            {
                return;
            }

            var position = selectField.transform.position;

            // 오브젝트 생성
            switch ((MenuElementType)selectElement)
            {
                case MenuElementType.PLAYER:
                    // 플레이어

                    if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                    {
                        break;
                    }

                    position = createObject.transform.position;

                    objectData.objectType = MenuElementType.PLAYER;
                    objectData.gameObject = Instantiate<GameObject>(playerPrefab);
                    objectData.gameObject.name = "player [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                    objectData.color = Color.black;
                    objectData.gameObject.transform.position = position;
                    objectData.gameObject.transform.parent = gameStage.transform;

                    arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                    break;
                case MenuElementType.NORMAL_CUBE:
                    // 노말 큐브

                    if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                    {
                        break;
                    }

                    objectData.objectType = MenuElementType.NORMAL_CUBE;
                    objectData.gameObject = Instantiate<GameObject>(normalCubePrefab);
                    objectData.gameObject.name = "NormalCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                    objectData.color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
                    objectData.gameObject.transform.position = position;
                    objectData.gameObject.transform.parent = gameStage.transform;

                    arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                    break;
                case MenuElementType.ICE_CUBE:
                    // 얼음 큐브

                    if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                    {
                        break;
                    }

                    objectData.objectType = MenuElementType.ICE_CUBE;
                    objectData.gameObject = Instantiate<GameObject>(iceCubePrefab);
                    objectData.gameObject.name = "IceCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                    objectData.color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
                    objectData.gameObject.transform.position = position;
                    objectData.gameObject.transform.parent = gameStage.transform;

                    arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                    break;
            }
        }

        // W 키
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

        // S 키
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
            Load();
        }
        
    }


    // 현제 층과 위 아래 오브젝트 색상 변경
    private void updateColor()
    {
        int iY;
        int iX;
        int iZ;

        if (height - 1 >= 0)
        {
            iY = height - 1;
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY && arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER)
                    {
                        // 색깔, 알파값 변경
                        arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b / 2, 0.8f);
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
                if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY && arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER)
                {
                    // 원래 색깔로 변경
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
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY && arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER)
                    {
                        // 색깔, 알파값 변경
                        arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r / 2, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b, 0.5f);
                    }
                    ++iX;
                }
                ++iZ;
            }
        }
    }


    private void Load()
    {
        int iY;
        int iX;
        int iZ;

        // 기존에 있던 오브젝트 삭제
        iY = 0;
        while (iY < 100)
        {
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].gameObject != null)
                    {
                        Destroy(arrMapObject[iY, iZ, iX].gameObject);
                    }
                    ++iX;
                }
                ++iZ;
            }
            ++iY;
        }

        Stream rs = new FileStream("a.dat", FileMode.Open);
        BinaryFormatter deserializer = new BinaryFormatter();

        arrMapObject = (ObjectData[,,])deserializer.Deserialize(rs);
        rs.Close();

        // 불러오기 오브젝트 생성
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
                            arrMapObject[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ);
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;

                            if (height > iY)
                            {
                                // 알파값 변경
                                arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b / 2, 0.8f);
                            }
                            else if (height < iY)
                            {
                                // 알파값 변경
                                arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r / 2, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b, 0.5f);
                            }

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
