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
    //--------------------------------
    // public 변수
    //--------------------------------

    public st_MapObjectData[,,] arrMapObject;       // 맵 오브젝트 배열
    public GameObject gameStage;                    // 생성된 게임 오브젝트가 자식으로 들어갈 부모
    public GameObject guideLine;                    // 맵툴 격자
    public GameObject playerPrefab;                 // 플레이어 프리팹
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject iceCubePrefab;                // 얼음 큐브 프리팹
    public GameObject cameraTarget;                 // 카메라가 바라보는 곳


    //--------------------------------
    // private 변수
    //--------------------------------

    private int focusPosY;                          // 현제 맵툴 필드 Y축 인덱스 좌표
    private int mapSizeY;                           // 맵 크기 Y축
    private int mapSizeZ;                           // 맵 크기 Z축
    private int mapSizeX;                           // 맵 크기 X축
    private int selectElement;                      // 선택된 요소
    private Vector3 mouseFieldPoint;                // 현제 맵툴 필드위에서의 마우스 위치
    private st_IndexPos playerPostion;              // 맵툴에 생성된 플레이어 오브젝트 위치 (배열 인덱스 기준)
    private st_IndexPos destPostion;                // 맵툴에 생성된 목적지 오브젝트 위치
    private bool isPlayerActive;                    // 플레이어 생성 확인
    private bool isDestActive;                      // 목적지 생성 확인
    private bool isCtrlKeyPressed;                  // 컨트롤 키 토글
    private bool isMouseOnField;                    // 맵툴 필드 위에 마우스가 있나 없나
    private bool isMouseOnUI;                       // 마우스가 UI 위에 있나 없나
    private bool isMouseUIDragging;                 // 마우스가 UI 드래그 중인가

    private Camera screenCamera;                    // 메인 카메라
    private GameObject selectField;                 // 선택 영역
    private GameObject player;                      // 선택 영역 표시용 플레이어
    private GameObject normalCube;                  // 선택 영역 표시용 일반 큐브
    private GameObject iceCube;                     // 선택 영역 표시용 얼음 큐브
    private GameObject createObject;                // 만들어질 게임 오브젝트
    private LayerMask layerMaskFloor;               // 레이어 마스크
    private Dictionary<KeyCode, Action> keyDown;            // 키 다운 딕셔너리
    private Dictionary<KeyCode, Action> keyAuto;            // 키 오토 딕셔너리
    private Dictionary<KeyCode, Action> keyUp;              // 키 업 딕셔너리
    private Dictionary<KeyCode, Action> keyCombination;     // 조합 키 딕셔너리


    //--------------------------------
    // enum
    //--------------------------------

    private const int MAP_SIZE_Y = 100;
    private const int MAP_SIZE_Z = 10;
    private const int MAP_SIZE_X = 10;


    //--------------------------------
    // public 함수
    //--------------------------------

    public void MousePointerChange(int element)
    {
        
        selectElement = element;

        

        // 선택된 요소
        switch ((en_MenuElementType)element)
        {
            case en_MenuElementType.EMPTY:
                // 빈 상태
                // 선택 영역
                selectField.SetActive(false);
                // 없음
                player.SetActive(false);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = null;
                break;
            case en_MenuElementType.PLAYER:
                // 플레이어
                // 선택 영역
                selectField.SetActive(true);
                // 플레이어 보이기
                player.SetActive(true);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = player;
                break;
            case en_MenuElementType.DEST:
                // 목적지
                // 선택 영역
                selectField.SetActive(true);
                // 플레이어 보이기
                player.SetActive(true);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = player;
                break;
            case en_MenuElementType.NORMAL_CUBE:
                // 노말 큐브
                // 선택 영역
                selectField.SetActive(true);
                // 노말 큐브 보이기
                player.SetActive(false);
                normalCube.SetActive(true);
                iceCube.SetActive(false);
                createObject = normalCube;
                break;
            case en_MenuElementType.ICE_CUBE:
                // 얼음 큐브
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

    // 마우스가 UI 위에 들어옴
    public void MouseUIEnter()
    {
        // 맵툴 마우스 포인터 처리를 위한 플래그 true
        isMouseOnUI = true;
    }

    // 마우스가 UI 에서 벗어남
    public void MouseUIExit()
    {
        // 맵툴 마우스 포인터 처리를 위한 플래그 false
        isMouseOnUI = false;
    }

    // UI 드래그 중
    public void MouseUIDragStart()
    {
        isMouseUIDragging = true;
    }

    // UI 드래그 끝
    public void MouseUIDragEnd()
    {
        isMouseUIDragging = false;
    }


    //--------------------------------
    // private 함수
    //--------------------------------

    private void Start()
    {
        // 오브젝트 배열 생성
        // Y, Z, X
        arrMapObject = new st_MapObjectData[MAP_SIZE_Y, MAP_SIZE_Z, MAP_SIZE_X];
        // 맵 크기 저장
        mapSizeY = MAP_SIZE_Y;
        mapSizeZ = MAP_SIZE_Z;
        mapSizeX = MAP_SIZE_X;

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

        // 키 값
        // 입력키, 호출 함수
        // 키 다운
        keyDown = new Dictionary<KeyCode, Action>
        {
            { KeyCode.W, Up},
            { KeyCode.UpArrow, Up},
            { KeyCode.S, Down},
            { KeyCode.DownArrow, Down},
            { KeyCode.LeftControl, LeftControlDown}
        };

        // 키 오토
        keyAuto = new Dictionary<KeyCode, Action>
        {
            { KeyCode.A, Left},
            { KeyCode.LeftArrow, Left},
            { KeyCode.D, Right},
            { KeyCode.RightArrow, Right},
            { KeyCode.Mouse0, MouseLeftClickAuto},
            { KeyCode.Mouse1, MouseRightClickAuto}
        };

        // 키 업
        keyUp = new Dictionary<KeyCode, Action>
        {
            { KeyCode.LeftControl, LeftControlUp}
        };

        // 조합 키
        keyCombination = new Dictionary<KeyCode, Action>
        {
            { KeyCode.S, Save},
            { KeyCode.L, Load}
        };
    }


    private void Update()
    {

        // 마우스 처리
        MouseProc();

        // 키처리
        KeyProc();
    }


    // 마우스 처리
    private void MouseProc()
    {
        RaycastHit rayHit;          // 레이 충돌한 물체

        // 화면 마우스 좌표
        var screenMouse = Input.mousePosition;
        /// 변환
        var world = screenCamera.ScreenToWorldPoint(screenMouse);

        // 레이
        if (!Physics.Raycast(world, screenCamera.transform.forward, out rayHit, 20f, layerMaskFloor))
        {
            // 마우스가 맵툴 영역을 벗어남
            // 마우스 포인터 숨김
            selectField.SetActive(false);
            isMouseOnField = false;
            return;
        }

        // 마우스가 UI 위에 있음 || 드래그 중임
        if (isMouseOnUI || isMouseUIDragging)
        {
            // 마우스 포인터 숨김
            selectField.SetActive(false);
            isMouseOnField = false;
            return;
        }


        isMouseOnField = true;

        Debug.DrawRay(world, screenCamera.transform.forward * 20f, Color.red);

        // 반올림, 올림
        mouseFieldPoint.x = Mathf.Round(rayHit.point.x);
        mouseFieldPoint.y = Mathf.Ceil(rayHit.point.y);
        mouseFieldPoint.z = Mathf.Round(rayHit.point.z);

        // 아무것도 선택 안한 상태임
        if (selectElement == (int)en_MenuElementType.EMPTY)
        {
            selectField.SetActive(false);
            return;
        }
        else
        {
            selectField.SetActive(true);
        }

        selectField.transform.position = mouseFieldPoint;

        // 비어있음
        if (arrMapObject[(int)mouseFieldPoint.y, (int)mouseFieldPoint.z, (int)mouseFieldPoint.x].objectType == en_MenuElementType.EMPTY)
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
        // 키 다운
        if (Input.anyKeyDown)
        {
            // 조합 키
            if (isCtrlKeyPressed)
            {
                foreach (var pair in keyCombination)
                {
                    if (Input.GetKeyDown(pair.Key))
                    {
                        pair.Value();
                    }
                }
            }
            // 단일 키
            else
            {
                foreach (var pair in keyDown)
                {
                    if (Input.GetKeyDown(pair.Key))
                    {
                        pair.Value();
                    }
                }
            }
        }

        // 키 오토
        foreach (var pair in keyAuto)
        {
            if (Input.GetKey(pair.Key))
            {
                pair.Value();
            }
        }

        // 키 업
        foreach (var pair in keyUp)
        {
            if (Input.GetKeyUp(pair.Key))
            {
                pair.Value();
            }
        }
    }


    // 현제 층과 위 아래 오브젝트 색상 변경
    private void updateColor()
    {
        int iX;
        int iZ;
        int iY;

        if (focusPosY - 1 >= 0)
        {
            iY = focusPosY - 1;
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.EMPTY &&
                        arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.PLAYER &&
                        arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.DEST)
                    {
                        // 색깔, 알파값 변경
                        arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b / 2, 0.8f);
                    }
                    ++iX;
                }
                ++iZ;
            }
        }

        iY = focusPosY;
        iZ = 0;
        while (iZ < 10)
        {
            iX = 0;
            while (iX < 10)
            {
                if (arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.EMPTY &&
                    arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.PLAYER &&
                    arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.DEST)
                {
                    // 원래 색깔로 변경
                    arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = arrMapObject[iY, iZ, iX].color;
                }
                ++iX;
            }
            ++iZ;
        }

        if (focusPosY + 1 < 100)
        {
            iY = focusPosY + 1;
            iZ = 0;
            while (iZ < 10)
            {
                iX = 0;
                while (iX < 10)
                {
                    if (arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.EMPTY &&
                        arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.PLAYER &&
                        arrMapObject[iY, iZ, iX].objectType != en_MenuElementType.DEST)
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

    
    // 카메라 위로 이동
    private void Up()
    {
        if (focusPosY + 1 < 100)
        {
            ++focusPosY;
            guideLine.transform.position = new Vector3(0f, focusPosY, 0f);
            // 카메라 위로
            screenCamera.transform.position = screenCamera.transform.position + new Vector3(0f, 1f, 0f);
            // 현제 층과 위 아래 오브젝트 색상 변경
            updateColor();
        }
    }

    // 카메라 아래로 이동
    private void Down()
    {
        if (focusPosY - 1 >= 0)
        {
            --focusPosY;
            guideLine.transform.position = new Vector3(0f, focusPosY, 0f);
            // 카메라 아래로
            screenCamera.transform.position = screenCamera.transform.position + new Vector3(0f, -1f, 0f);
            // 현제 층과 위 아래 오브젝트 색상 변경
            updateColor();
        }
    }

    // 카메라 왼쪽 회전
    private void Left()
    {
        var angle = cameraTarget.transform.eulerAngles;
        angle.y = angle.y - 1f;
        cameraTarget.transform.eulerAngles = angle;
    }

    // 카메라 오른쪽 회전
    private void Right()
    {
        var angle = cameraTarget.transform.eulerAngles;
        angle.y = angle.y + 1f;
        cameraTarget.transform.eulerAngles = angle;
    }

    // 마우스 왼쪽 클릭 오토
    private void MouseLeftClickAuto()
    {
        st_MapObjectData st_MapObjectData;
        Vector3 position = mouseFieldPoint;

        // 오브젝트 생성
        switch ((en_MenuElementType)selectElement)
        {
            case en_MenuElementType.EMPTY:

                break;
            case en_MenuElementType.PLAYER:
                // 플레이어

                // 마우스가 필드위에서 벗어남
                if (!isMouseOnField)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != en_MenuElementType.EMPTY)
                {
                    return;
                }

                // 플레이어 오브젝트가 이미 맵툴에 생성된 적이 있음
                if (isPlayerActive == true)
                {
                    // 기존의 플레이어 오브젝트를 제거함
                    arrMapObject[playerPostion.iY, playerPostion.iZ, playerPostion.iX].objectType = en_MenuElementType.EMPTY;
                    Destroy(arrMapObject[playerPostion.iY, playerPostion.iZ, playerPostion.iX].gameObject);
                }

                // 플레이어 생성 위치 저장
                playerPostion.iY = (int)position.y;
                playerPostion.iZ = (int)position.z;
                playerPostion.iX = (int)position.x;
                // 플레이어 생성 확인 true
                isPlayerActive = true;

                st_MapObjectData.objectType = en_MenuElementType.PLAYER;
                st_MapObjectData.gameObject = Instantiate<GameObject>(playerPrefab);
                st_MapObjectData.gameObject.name = "Player [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                st_MapObjectData.color = Color.black;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                st_MapObjectData.gameObject.transform.position = createObject.transform.position;
                st_MapObjectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = st_MapObjectData;
                return;
            case en_MenuElementType.DEST:
                // 목적지

                // 마우스가 필드위에서 벗어남
                if (!isMouseOnField)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != en_MenuElementType.EMPTY)
                {
                    return;
                }

                // 목적지 오브젝트가 이미 맵툴에 생성된 적이 있음
                if (isDestActive == true)
                {
                    // 기존의 플레이어 오브젝트를 제거함
                    arrMapObject[destPostion.iY, destPostion.iZ, destPostion.iX].objectType = en_MenuElementType.EMPTY;
                    Destroy(arrMapObject[destPostion.iY, destPostion.iZ, destPostion.iX].gameObject);
                }

                // 목적지 생성 위치 저장
                destPostion.iY = (int)position.y;
                destPostion.iZ = (int)position.z;
                destPostion.iX = (int)position.x;
                // 목적지 생성 확인 true
                isDestActive = true;

                st_MapObjectData.objectType = en_MenuElementType.DEST;
                st_MapObjectData.gameObject = Instantiate<GameObject>(playerPrefab);
                st_MapObjectData.gameObject.name = "Dest [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                st_MapObjectData.color = Color.black;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                st_MapObjectData.gameObject.transform.position = createObject.transform.position;
                st_MapObjectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = st_MapObjectData;
                break;
            case en_MenuElementType.NORMAL_CUBE:
                // 노말 큐브

                // 마우스가 필드위에서 벗어남
                if (!isMouseOnField)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != en_MenuElementType.EMPTY)
                {
                    return;
                }

                st_MapObjectData.objectType = en_MenuElementType.NORMAL_CUBE;
                st_MapObjectData.gameObject = Instantiate<GameObject>(normalCubePrefab);
                st_MapObjectData.gameObject.name = "NormalCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                st_MapObjectData.color = st_MapObjectData.gameObject.GetComponent<MeshRenderer>().material.color;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                st_MapObjectData.gameObject.transform.position = createObject.transform.position;
                st_MapObjectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = st_MapObjectData;
                return;
            case en_MenuElementType.ICE_CUBE:
                // 얼음 큐브

                // 마우스가 필드위에서 벗어남
                if (!isMouseOnField)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != en_MenuElementType.EMPTY)
                {
                    return;
                }

                st_MapObjectData.objectType = en_MenuElementType.ICE_CUBE;
                st_MapObjectData.gameObject = Instantiate<GameObject>(iceCubePrefab);
                st_MapObjectData.gameObject.name = "IceCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                st_MapObjectData.color = st_MapObjectData.gameObject.GetComponent<MeshRenderer>().material.color;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                st_MapObjectData.gameObject.transform.position = createObject.transform.position;
                st_MapObjectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = st_MapObjectData;
                return;
        }
    }

    // 마우스 오른쪽 클릭 오토
    private void MouseRightClickAuto()
    {
        Vector3 position = mouseFieldPoint;
        en_MenuElementType objectType = arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType;


        // 마우스가 필드위에서 벗어남
        if (!isMouseOnField)
        {
            return;
        }

        switch (objectType)
        {
            case en_MenuElementType.EMPTY:
                // 해당 위치가 비어있음
                return;
            case en_MenuElementType.PLAYER:
                // 플레이어 생성 확인 false
                isPlayerActive = false;
                break;
            case en_MenuElementType.DEST:
                // 목적지 생성 확인 false;
                isDestActive = false;
                break;
        }


        arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType = en_MenuElementType.EMPTY;
        Destroy(arrMapObject[(int)position.y, (int)position.z, (int)position.x].gameObject);
    }

    // 왼쪽 컨트롤 키 다운
    private void LeftControlDown()
    {
        isCtrlKeyPressed = true;
    }

    // 왼쪽 컨트롤 키 업
    private void LeftControlUp()
    {
        isCtrlKeyPressed = false;
    }

    // 세이브
    private void Save()
    {
        st_MapData mapData;
        Stream ws;
        BinaryFormatter serializer;

        // 맵 정보
        mapData.iMapSizeY = mapSizeY;
        mapData.iMapSizeZ = mapSizeZ;
        mapData.iMapSizeX = mapSizeX;
        mapData.playerPostion = playerPostion;
        mapData.destPostion = destPostion;
        mapData.isPlayerActive = isPlayerActive;
        mapData.isDestActive = isDestActive;

        // 파일 만들기
        ws = new FileStream("a.dat", FileMode.Create);
        serializer = new BinaryFormatter();

        // 맵 정보 저장
        serializer.Serialize(ws, mapData);
        // 맵 오브젝트 정보 저장
        serializer.Serialize(ws, arrMapObject);
        ws.Close();

        //---------------------------------
        // 전달할 다른 정보 생각해보기
        //---------------------------------
        Debug.Log("저장");
    }

    // 로드
    private void Load()
    {
        int iX;
        int iZ;
        int iY;
        st_MapData mapData;
        Stream rs;
        BinaryFormatter deserializer;


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

        // 파일 불러오기
        rs = new FileStream("a.dat", FileMode.Open);
        deserializer = new BinaryFormatter();

        // 맵 정보 불러오기
        mapData = (st_MapData)deserializer.Deserialize(rs);
        // 맵 오브젝트 정보 불러오기
        arrMapObject = (st_MapObjectData[,,])deserializer.Deserialize(rs);
        rs.Close();

        // 맵 정보 셋팅
        mapSizeY = mapData.iMapSizeY;
        mapSizeZ = mapData.iMapSizeZ;
        mapSizeX = mapData.iMapSizeX;
        playerPostion = mapData.playerPostion;
        destPostion = mapData.destPostion;
        isPlayerActive = mapData.isPlayerActive;
        isDestActive = mapData.isDestActive;

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
                        case en_MenuElementType.PLAYER:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "Player [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            break;
                        case en_MenuElementType.DEST:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "Dest [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            break;
                        case en_MenuElementType.NORMAL_CUBE:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + normalCubePrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;

                            if (focusPosY > iY)
                            {
                                // 알파값 변경
                                arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b / 2, 0.8f);
                            }
                            else if (focusPosY < iY)
                            {
                                // 알파값 변경
                                arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r / 2, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b, 0.5f);
                            }
                            break;
                        case en_MenuElementType.ICE_CUBE:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(iceCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "IceCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + iceCubePrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;

                            if (focusPosY > iY)
                            {
                                // 알파값 변경
                                arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color = new Color(arrMapObject[iY, iZ, iX].color.r, arrMapObject[iY, iZ, iX].color.g / 2, arrMapObject[iY, iZ, iX].color.b / 2, 0.8f);
                            }
                            else if (focusPosY < iY)
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
