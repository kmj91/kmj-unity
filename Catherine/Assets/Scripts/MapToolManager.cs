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
    private Vector3 MouseFieldPoint;                // 현제 맵툴 필드위에서의 마우스 위치
    private Vector3 palyerPostion;                  // 맵툴에 생성된 플레이어 오브젝트 위치
    private Vector3 destPostion;                    // 맵툴에 생성된 목적지 오브젝트 위치
    private bool checkPlayer;                       // 플레이어 생성 확인
    private bool checkDest;                         // 목적지 생성 확인
    private bool controlToggle;                     // 컨트롤 키 토글
    private bool onFieldMouse;                      // 맵툴 필드 위에 마우스가 있나 없나

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


    public void MousePointerChange(int element)
    {
        
        selectElement = element;

        

        // 선택된 요소
        switch ((MenuElementType)element)
        {
            case MenuElementType.EMPTY:
                // 빈 상태
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
                // 선택 영역
                selectField.SetActive(true);
                // 플레이어 보이기
                player.SetActive(true);
                normalCube.SetActive(false);
                iceCube.SetActive(false);
                createObject = player;
                break;
            case MenuElementType.DEST:
                // 목적지
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
            selectField.SetActive(false);
            onFieldMouse = false;
            return;
        }
        else {
            onFieldMouse = true;
        }


        Debug.DrawRay(world, screenCamera.transform.forward * 20f, Color.red);

        // 반올림, 올림
        MouseFieldPoint.x = Mathf.Round(rayHit.point.x);
        MouseFieldPoint.y = Mathf.Ceil(rayHit.point.y);
        MouseFieldPoint.z = Mathf.Round(rayHit.point.z);

        // 아무것도 선택 안한 상태임
        if (selectElement == (int)MenuElementType.EMPTY)
        {
            selectField.SetActive(false);
            return;
        }
        else
        {
            selectField.SetActive(true);
        }

        selectField.transform.position = MouseFieldPoint;

        // 비어있음
        if (arrMapObject[(int)MouseFieldPoint.y, (int)MouseFieldPoint.z, (int)MouseFieldPoint.x].objectType == MenuElementType.EMPTY)
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
            if (controlToggle)
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
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY &&
                        arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER &&
                        arrMapObject[iY, iZ, iX].objectType != MenuElementType.DEST)
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
                if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY &&
                    arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER &&
                    arrMapObject[iY, iZ, iX].objectType != MenuElementType.DEST)
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
                    if (arrMapObject[iY, iZ, iX].objectType != MenuElementType.EMPTY &&
                        arrMapObject[iY, iZ, iX].objectType != MenuElementType.PLAYER &&
                        arrMapObject[iY, iZ, iX].objectType != MenuElementType.DEST)
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

    // 카메라 아래로 이동
    private void Down()
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
        ObjectData objectData;
        Vector3 position = MouseFieldPoint;

        // 오브젝트 생성
        switch ((MenuElementType)selectElement)
        {
            case MenuElementType.EMPTY:

                break;
            case MenuElementType.PLAYER:
                // 플레이어

                // 마우스가 필드위에서 벗어남
                if (!onFieldMouse)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                {
                    return;
                }

                // 플레이어 오브젝트가 이미 맵툴에 생성된 적이 있음
                if (checkPlayer == true)
                {
                    // 기존의 플레이어 오브젝트를 제거함
                    arrMapObject[(int)palyerPostion.y, (int)palyerPostion.z, (int)palyerPostion.x].objectType = MenuElementType.EMPTY;
                    Destroy(arrMapObject[(int)palyerPostion.y, (int)palyerPostion.z, (int)palyerPostion.x].gameObject);
                }

                // 플레이어 생성 위치 저장
                palyerPostion = position;
                // 플레이어 생성 확인 true
                checkPlayer = true;

                objectData.objectType = MenuElementType.PLAYER;
                objectData.gameObject = Instantiate<GameObject>(playerPrefab);
                objectData.gameObject.name = "Player [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                objectData.color = Color.black;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                objectData.gameObject.transform.position = createObject.transform.position;
                objectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                return;
            case MenuElementType.DEST:
                // 목적지

                // 마우스가 필드위에서 벗어남
                if (!onFieldMouse)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                {
                    return;
                }

                // 목적지 오브젝트가 이미 맵툴에 생성된 적이 있음
                if (checkDest == true)
                {
                    // 기존의 플레이어 오브젝트를 제거함
                    arrMapObject[(int)destPostion.y, (int)destPostion.z, (int)destPostion.x].objectType = MenuElementType.EMPTY;
                    Destroy(arrMapObject[(int)destPostion.y, (int)destPostion.z, (int)destPostion.x].gameObject);
                }

                // 목적지 생성 위치 저장
                destPostion = position;
                // 목적지 생성 확인 true
                checkDest = true;

                objectData.objectType = MenuElementType.DEST;
                objectData.gameObject = Instantiate<GameObject>(playerPrefab);
                objectData.gameObject.name = "Dest [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                objectData.color = Color.black;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                objectData.gameObject.transform.position = createObject.transform.position;
                objectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                break;
            case MenuElementType.NORMAL_CUBE:
                // 노말 큐브

                // 마우스가 필드위에서 벗어남
                if (!onFieldMouse)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                {
                    return;
                }

                objectData.objectType = MenuElementType.NORMAL_CUBE;
                objectData.gameObject = Instantiate<GameObject>(normalCubePrefab);
                objectData.gameObject.name = "NormalCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                objectData.color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                objectData.gameObject.transform.position = createObject.transform.position;
                objectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                return;
            case MenuElementType.ICE_CUBE:
                // 얼음 큐브

                // 마우스가 필드위에서 벗어남
                if (!onFieldMouse)
                {
                    return;
                }

                // 해당 위치가 비어있지 않음
                if (arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType != MenuElementType.EMPTY)
                {
                    return;
                }

                objectData.objectType = MenuElementType.ICE_CUBE;
                objectData.gameObject = Instantiate<GameObject>(iceCubePrefab);
                objectData.gameObject.name = "IceCube [" + (int)position.y + ", " + (int)position.z + ", " + (int)position.x + "]";
                objectData.color = objectData.gameObject.GetComponent<MeshRenderer>().material.color;
                // 오브젝트 마다 중심점이 다름
                // 각자 맞게 배치됨
                objectData.gameObject.transform.position = createObject.transform.position;
                objectData.gameObject.transform.parent = gameStage.transform;

                arrMapObject[(int)position.y, (int)position.z, (int)position.x] = objectData;
                return;
        }
    }

    // 마우스 오른쪽 클릭 오토
    private void MouseRightClickAuto()
    {
        Vector3 position = MouseFieldPoint;
        MenuElementType objectType = arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType;


        // 마우스가 필드위에서 벗어남
        if (!onFieldMouse)
        {
            return;
        }

        switch (objectType)
        {
            case MenuElementType.EMPTY:
                // 해당 위치가 비어있음
                return;
            case MenuElementType.PLAYER:
                // 플레이어 생성 확인 false
                checkPlayer = false;
                break;
            case MenuElementType.DEST:
                // 목적지 생성 확인 false;
                checkDest = false;
                break;
        }


        arrMapObject[(int)position.y, (int)position.z, (int)position.x].objectType = MenuElementType.EMPTY;
        Destroy(arrMapObject[(int)position.y, (int)position.z, (int)position.x].gameObject);
    }

    // 왼쪽 컨트롤 키 다운
    private void LeftControlDown()
    {
        controlToggle = true;
    }

    // 왼쪽 컨트롤 키 업
    private void LeftControlUp()
    {
        controlToggle = false;
    }

    // 세이브
    private void Save()
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

    // 로드
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
                        case MenuElementType.PLAYER:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "Player [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            break;
                        case MenuElementType.DEST:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "Dest [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            break;
                        case MenuElementType.NORMAL_CUBE:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + normalCubePrefab.transform.position;
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
                        case MenuElementType.ICE_CUBE:
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(iceCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "IceCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + iceCubePrefab.transform.position;
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
