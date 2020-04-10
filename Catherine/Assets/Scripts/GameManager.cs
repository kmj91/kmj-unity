using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GameMessageScript;
using UnityDequeScript;
using MapToolGlobalScript;
using GameGlobalScript;

using Cinemachine;

public class GameManager : MonoBehaviour
{
    //--------------------------------
    // public 변수
    //--------------------------------

    public float playerSpeed;                       // 플레이어 이동 속도
    public float cubeSpeed;                         // 큐브 이동 속도
    public int undoToken;                           // 복원 지점 토큰
    public Queue<GameMessage> messageQueue;         // 게임 매니저 큐
    public UnityDeque<UndoData> undoDeque;
    public GameObject gameStage;                    // 생성된 게임 오브젝트가 자식으로 들어갈 부모
    public GameObject playerPrefab;                 // 플레이어 프리팹
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject iceCubePrefab;                // 얼음 큐브 프리팹
    public GameObject followCam;                    // 카메라
    public en_Direction m_playerDirection;          // 플레이어 방향
    public bool canPlayerControl;                   // 플레이어 조작 플래그
    

    //--------------------------------
    // private 변수
    //--------------------------------

    private int undoArraySize;                      // 배열 크기
    private int m_mapSizeY;                         // 맵 크기 Y축
    private int m_mapSizeZ;                         // 맵 크기 Z축
    private int m_mapSizeX;                         // 맵 크기 X축
    private int m_iPlayerJumpPower;                 // 플레이어 점프력
    private st_IndexPos m_playerPosition;           // 맵에 생성된 플레이어 오브젝트 위치 (배열 인덱스 기준)
    private st_IndexPos backupm_playerPosition;
    private st_IndexPos destPosition;                // 맵에 생성된 목적지 오브젝트 위치
    //private PlayerMovement playerMovement;          // 플레이어 무브먼트
    private GameObject m_playerObject;              // 플레이어 오브젝트
    private GameObject GameOverUI;
    private Camera m_mainCamera;                    // 메인 카메라
    private PlayerAction m_playerAction;            // 플레이어 액션 스크립트
    private st_GameObjectData[,,] m_arrMapData;     // 게임 맵 오브젝트 정보 배열
    private Action[] arrMsgProc;                    // 메시지 함수 배열
    private KeyCode[] arrKeyDownCode;               // 키 다운 코드 배열
    private Action[] arrKeyDownProc;                // 키 다운 함수 배열
    private KeyCode[] arrKeyAutoCode;               // 키 오토 코드 배열
    private Action[] arrKeyAutoProc;                // 키 오토 함수 배열
    private KeyCode[] arrKeyUpCode;                 // 키 업 코드 배열
    private Action[] arrKeyUpProc;                  // 키 업 함수 배열
    private bool restartFlag;
    private bool isPlayerActive;                    // 플레이어 생성 확인
    private bool isDestActive;                      // 목적지 생성 확인
    private bool isGripKeyPressed;                  // 붙잡기 누름
    private bool isPlayerClimbing;                  // 플레이어 등반 확인


    //--------------------------------
    // private 함수
    //--------------------------------

    private void Awake()
    {
        arrMsgProc = new Action[] 
        {
            CreateUndoPoint,
            UpdateUndoCube
        };
        arrKeyDownCode = new KeyCode[]
        {
            KeyCode.Mouse0          // 마우스 왼쪽 클릭
        };
        arrKeyDownProc = new Action[]
        {
            GripStart               // 붙잡기 시작
        };
        arrKeyAutoCode = new KeyCode[] 
        {
            KeyCode.W,              // 위
            KeyCode.UpArrow,
            KeyCode.S,              // 아래
            KeyCode.DownArrow,
            KeyCode.A,              // 왼쪽
            KeyCode.LeftArrow,
            KeyCode.D,              // 오른쪽
            KeyCode.RightArrow
        };
        arrKeyAutoProc = new Action[] 
        {
            Up,
            Up,
            Down,
            Down,
            Left,
            Left,
            Right,
            Right
        };
        arrKeyUpCode = new KeyCode[]
        {
            KeyCode.Mouse0          // 마우스 왼쪽 클릭
        };
        arrKeyUpProc = new Action[]
        {
            GripEnd                 // 붙잡기 끝
        };

        // 게임 초기화
        InitGame();
        m_iPlayerJumpPower = 1;
    }


    private void Start()
    {
        // 플레이어 무브먼트
        //playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        // UI
        GameOverUI = GameObject.Find("Canvas").transform.Find("gameOverUI").gameObject;
        // 메인 카메라
        m_mainCamera = Camera.main;
        // 다시시작 플래그
        restartFlag = false;
        // 초기화
        undoToken = 0;
        messageQueue = new Queue<GameMessage>();
        undoDeque = new UnityDeque<UndoData>();
        // 시네머신 카메라 설정
        var cinemachine = followCam.GetComponent<CinemachineFreeLook>();
        cinemachine.Follow = m_playerObject.transform;
        cinemachine.LookAt = m_playerObject.transform;
    }

    private void Update()
    {
        GameMessage gameMsg;
        UndoData undoData;          // 되돌리기 정보
        CubeMovement cubeMovement;

        // 플레이어 이동 위치
        if (m_playerPosition != backupm_playerPosition)
        {
            backupm_playerPosition = m_playerPosition;

            Debug.Log("iY : " + m_playerPosition.iY + ", iZ : " + m_playerPosition.iZ + ", iX : " + m_playerPosition.iX);
        }

        // 키처리
        KeyProc();

        if (restartFlag)
        {
            // 씬 다시 불러오기
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("SampleScene");
            }
        }

        //if (playerMovement.isDeath)
        //{
        //    // 게임오버
        //    GameOverUI.SetActive(true);
        //    // 다시 시작
        //    restartFlag = true;
        //}

        //// 되돌리기 테스트
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    // Deque가 비어있지 않으면 되돌리기
        //    if (undoDeque.Count > 0)
        //    {
        //        // 뒤에서부터 데이터 하나 꺼내기
        //        undoData = undoDeque.Pop_Back();

        //        //-----------------------------------------------
        //        // 1. 플레이어 애니메이션도 대기상태로 복구해야됨
        //        //-----------------------------------------------

        //        // 플레이어 위치 되돌리기
        //        playerMovement.setPlayerPostion(undoData.playerPos);
        //        // 플레이어 상태 되돌리기
        //        playerMovement.UndoProcess();

        //        // 큐브 되돌리기
        //        for (int i = 0; i < undoArraySize; ++i)
        //        {
        //            if (!undoData.cubePosArray[i].flag)
        //            {
        //                break;
        //            }

        //            cubeMovement = undoData.cubePosArray[i].cubeObject.GetComponent<CubeMovement>();

        //            // 큐브 위치 되돌리기
        //            cubeMovement.transform.position = undoData.cubePosArray[i].CubePos;
        //            // 큐브 대기 상태로
        //            cubeMovement.UpdateStateToIdle();
        //        }
        //    }

        //    //Debug.Log("undoDeque Count : " + undoDeque.Count);
        //}


        // 처리할 메시지
        if (messageQueue.Count != 0)
        {
            gameMsg = messageQueue.Peek() as GameMessage;

            Debug.Log(gameMsg.messageType);

            // 메시지 처리
            arrMsgProc[(int)gameMsg.messageType]();
        }
    }


    // 키 처리
    private void KeyProc()
    {
        int iCnt;
        int iSize;

        
        if (Input.anyKeyDown)
        {
            // 키 다운
            iSize = arrKeyDownCode.Length;
            iCnt = 0;
            while (iCnt < iSize)
            {
                if (Input.GetKeyDown(arrKeyDownCode[iCnt]))
                {
                    arrKeyDownProc[iCnt]();
                }
                ++iCnt;
            }

            // 키 오토
            //-----------------------------------------
            // 나중에 getKey로 수정해야됨
            //-----------------------------------------
            iSize = arrKeyAutoCode.Length;
            iCnt = 0;
            while (iCnt < iSize)
            {
                if (Input.GetKeyDown(arrKeyAutoCode[iCnt]))
                {
                    arrKeyAutoProc[iCnt]();
                }
                ++iCnt;
            }
        }

        // 키 업
        iSize = arrKeyUpCode.Length;
        iCnt = 0;
        while (iCnt < iSize)
        {
            if (Input.GetKeyUp(arrKeyUpCode[iCnt]))
            {
                arrKeyUpProc[iCnt]();
            }
            ++iCnt;
        }
    }


    // 게임 초기화
    private void InitGame()
    {
        int iX;
        int iZ;
        int iY;
        st_MapToolData mapData;
        CubeAction cubeScript;                          // 큐브 스크립트
        Stream rs;
        BinaryFormatter deserializer;
        st_MapToolObjectData[,,] arrMapToolObject;      // 맵 오브젝트 배열

        // 파일 불러오기
        rs = new FileStream("a.dat", FileMode.Open);
        deserializer = new BinaryFormatter();

        // 맵 정보 불러오기
        mapData = (st_MapToolData)deserializer.Deserialize(rs);
        // 맵 정보 저장
        m_mapSizeY = mapData.iMapSizeY;
        m_mapSizeZ = mapData.iMapSizeZ;
        m_mapSizeX = mapData.iMapSizeX;
        m_playerPosition = mapData.playerPosition;
        destPosition = mapData.destPosition;
        isPlayerActive = mapData.isPlayerActive;
        isDestActive = mapData.isDestActive;

        // 맵툴에서 불러온 오브젝트 정보를 저장할 배열 생성
        // Y, Z, X
        arrMapToolObject = new st_MapToolObjectData[mapData.iMapSizeY, mapData.iMapSizeZ, mapData.iMapSizeX];
        // 맵툴에서 불러온 오브젝트 정보 배열에 저장
        arrMapToolObject = (st_MapToolObjectData[,,])deserializer.Deserialize(rs);
        // 파일 닫기
        rs.Close();
        // 게임에서 쓸 오브젝트 배열 생성
        m_arrMapData = new st_GameObjectData[mapData.iMapSizeY, mapData.iMapSizeZ, mapData.iMapSizeX];

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
                    switch (arrMapToolObject[iY, iZ, iX].objectType)
                    {
                        case en_MenuElementType.PLAYER:
                            // 플레이어
                            // 플레이어 게임 오브젝트 추가
                            m_playerObject = Instantiate<GameObject>(playerPrefab);
                            // 배열에 게임 오브젝트 저장
                            m_arrMapData[iY, iZ, iX].gameObject = m_playerObject;
                            // 게임 오브젝트 이름 변경
                            m_arrMapData[iY, iZ, iX].gameObject.name = "Player [" + iY + ", " + iZ + ", " + iX + "]";
                            // 오브젝트 위치 저장
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            // 배열에 태그, 레이어 메쉬 정보 저장
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.PLAYER;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.PLAYER;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
                            // 플레이어 액션 스크립트 추가
                            m_arrMapData[iY, iZ, iX].gameObject.AddComponent<PlayerAction>();
                            // 플레이어 액션 스크립트 멤버 변수로 저장
                            m_playerAction = m_playerObject.GetComponent<PlayerAction>();
                            // 배열에 액션 스크립트 저장
                            m_arrMapData[iY, iZ, iX].actionScript = m_playerAction;
                            // 스크립트 초기화
                            m_playerAction.Init(this, playerSpeed);
                            // 플레이어 조작 플래그
                            canPlayerControl = true;
                            break;
                        case en_MenuElementType.DEST:
                            // 목적지
                            // 목적지 게임 오브젝트 추가 및 배열에 저장
                            m_arrMapData[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            // 게임 오브젝트 이름 변경
                            m_arrMapData[iY, iZ, iX].gameObject.name = "Dest [" + iY + ", " + iZ + ", " + iX + "]";
                            // 오브젝트 위치 저장
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            // 이 게임 오브젝트를 게임 스테이지 하위 항목의 위치로 변경
                            m_arrMapData[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            // 배열에 태그, 레이어, 메쉬 정보 저장
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.DEST;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.DEST;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
                            break;
                        case en_MenuElementType.NORMAL_CUBE:
                            // 일반 큐브
                            // 일반 큐브 게임 오브젝트 추가 및 배열에 저장
                            m_arrMapData[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                            // 게임 오브젝트 이름 변경
                            m_arrMapData[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            // 오브젝트 위치 저장
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + normalCubePrefab.transform.position;
                            // 이 게임 오브젝트를 게임 스테이지 하위 항목의 위치로 변경
                            m_arrMapData[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            // 배열에 태그, 레이어, 메쉬 정보 저장
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.NORMAL_CUBE;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.CUBE;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.BLOCK;
                            // 큐브 액션 스크립트 추가
                            m_arrMapData[iY, iZ, iX].gameObject.AddComponent<CubeAction>();
                            // 배열에 액션 스크립트 저장
                            cubeScript = m_arrMapData[iY, iZ, iX].gameObject.GetComponent<CubeAction>();
                            m_arrMapData[iY, iZ, iX].actionScript = cubeScript;
                            // 스크립트 초기화
                            cubeScript.Init(cubeSpeed);
                            break;
                        case en_MenuElementType.ICE_CUBE:
                            // 얼음 큐브
                            m_arrMapData[iY, iZ, iX].gameObject = Instantiate<GameObject>(iceCubePrefab);
                            m_arrMapData[iY, iZ, iX].gameObject.name = "IceCube [" + iY + ", " + iZ + ", " + iX + "]";
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + iceCubePrefab.transform.position;
                            m_arrMapData[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.ICE_CUBE;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.CUBE;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.BLOCK;
                            // 큐브 액션 스크립트 추가
                            m_arrMapData[iY, iZ, iX].gameObject.AddComponent<CubeAction>();
                            // 배열에 액션 스크립트 저장
                            cubeScript = m_arrMapData[iY, iZ, iX].gameObject.GetComponent<CubeAction>();
                            m_arrMapData[iY, iZ, iX].actionScript = cubeScript;
                            // 스크립트 초기화
                            cubeScript.Init(cubeSpeed);
                            break;
                        default:
                            // 비어 있음
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
                            break;
                    }

                    ++iX;
                }
                ++iZ;
            }
            ++iY;
        }
    }


    // 새로운 복원 지점 만들기
    private void CreateUndoPoint()
    {
        UndoPointDataMsg UndoMsg;
        UndoData undoData;          // 되돌리기 정보

        UndoMsg = messageQueue.Dequeue() as UndoPointDataMsg;

        // 플레이어 위치
        undoData.playerPos = UndoMsg.playerPos;

        // 배열 크기
        undoArraySize = 20;
        undoData.cubePosArray = new CubePosData[undoArraySize];
        // 배열 복사
        UndoMsg.cubePosArray.CopyTo(undoData.cubePosArray, 0);

        // 되돌리기 저장
        while (!undoDeque.Push_Back(undoData))
        {
            Debug.Log("buffer full");

            // 버퍼가 꽉참
            // 먼저 들어온 데이터부터 비우기
            undoDeque.Pop_Front();
        }

        // 복원 지점 토큰 값 증가
        ++undoToken;

        //Debug.Log("undoDeque Count : " + undoDeque.Count);
        Debug.Log("undoToken : " + undoToken);
    }

    // 현제 복원 지점에 새롭게 복원할 큐브 추가
    private void UpdateUndoCube()
    {
        UndoCubeDataMsg UndoMsg;
        UndoData undoData;          // 되돌리기 정보
        int iCnt;
        int iSize;

        // 추가할 복원 정보
        UndoMsg = messageQueue.Dequeue() as UndoCubeDataMsg;

        if (undoDeque.Count <= 0)
        {
            return;
        }
        // 원래 있던 복원 지점 꺼내기
        undoData = undoDeque.Pop_Back();

        iCnt = 0;
        iSize = undoData.cubePosArray.Length;
        while (iCnt < iSize)
        {
            if (undoData.cubePosArray[iCnt].flag == false)
            {
                // 복원 정보 추가
                undoData.cubePosArray[iCnt].flag = true;
                undoData.cubePosArray[iCnt].cubeObject = UndoMsg.cubeObject;
                undoData.cubePosArray[iCnt].CubePos = UndoMsg.cubePos;
                // 다시 넣기
                undoDeque.Push_Back(undoData);
                return;
            }
            ++iCnt;
        }

        Debug.Log("복원 지점에 추가하지 못함");
    }



    // 붙잡기 시작
    private void GripStart()
    {
        isGripKeyPressed = true;
    }


    // 붙잡기 끝
    private void GripEnd()
    {
        isGripKeyPressed = false;
    }


    // 방향키 위
    private void Up()
    {
        float CameraAngleY;      // 카메라 방향

        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

        //------------------------------------------------
        // 카메라 방향에 따른 키 입력 값 변화
        //------------------------------------------------
        CameraAngleY = m_mainCamera.transform.eulerAngles.y;
        // 오른쪽 →
        if (45 <= CameraAngleY && CameraAngleY < 135)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingUp();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripRight();
                return;
            }

            // 이동
            PlayerMoveRight();
        }
        // 뒤쪽 ↓
        else if (135 <= CameraAngleY && CameraAngleY < 225)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingUp();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripBack();
            }

            // 이동
            PlayerMoveBack();
        }
        // 왼쪽 ←
        else if (225 <= CameraAngleY && CameraAngleY < 315)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingUp();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripLeft();
            }

            // 이동
            PlayerMoveLeft();
        }
        // 앞쪽 ↑
        else
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingUp();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripForward();
            }

            // 이동
            PlayerMoveForward();
        }
    }


    // 방향키 아래
    private void Down()
    {
        float CameraAngleY;      // 카메라 방향

        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

        //------------------------------------------------
        // 카메라 방향에 따른 키 입력 값 변화
        //------------------------------------------------
        CameraAngleY = m_mainCamera.transform.eulerAngles.y;
        // 오른쪽 →
        if (45 <= CameraAngleY && CameraAngleY < 135)
        {
            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripLeft();
            }
            // 일반
            else
            {
                PlayerMoveLeft();
            }
        }
        // 뒤쪽 ↓
        else if (135 <= CameraAngleY && CameraAngleY < 225)
        {
            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripForward();
            }
            // 일반
            else
            {
                PlayerMoveForward();
            }
        }
        // 왼쪽 ←
        else if (225 <= CameraAngleY && CameraAngleY < 315)
        {
            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripRight();
            }
            // 일반
            else
            {
                PlayerMoveRight();
            }
        }
        // 앞쪽 ↑
        else
        {
            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripBack();
            }
            // 일반
            else
            {
                PlayerMoveBack();
            }
        }
    }


    // 방향키 왼쪽
    private void Left()
    {
        float CameraAngleY;      // 카메라 방향

        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

        //------------------------------------------------
        // 카메라 방향에 따른 키 입력 값 변화
        //------------------------------------------------
        CameraAngleY = m_mainCamera.transform.eulerAngles.y;
        // 오른쪽 →
        if (45 <= CameraAngleY && CameraAngleY < 135)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingLeft();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripForward();
                return;
            }

            // 이동
            PlayerMoveForward();
        }
        // 뒤쪽 ↓
        else if (135 <= CameraAngleY && CameraAngleY < 225)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingLeft(true);
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripRight();
                return;
            }

            // 이동
            PlayerMoveRight();
        }
        // 왼쪽 ←
        else if (225 <= CameraAngleY && CameraAngleY < 315)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingLeft();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripBack();
                return;
            }
            // 이동
            PlayerMoveBack();
        }
        // 앞쪽 ↑
        else
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingLeft();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripLeft();
                return;
            }
            
            // 이동
            PlayerMoveLeft();
        }
    }


    // 방향키 오른쪽
    private void Right()
    {
        float CameraAngleY;      // 카메라 방향

        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

        //------------------------------------------------
        // 카메라 방향에 따른 키 입력 값 변화
        //------------------------------------------------
        CameraAngleY = m_mainCamera.transform.eulerAngles.y;
        // 오른쪽 →
        if (45 <= CameraAngleY && CameraAngleY < 135)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingRight();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripBack();
                return;
            }

            // 이동
            PlayerMoveBack();
        }
        // 뒤쪽 ↓
        else if (135 <= CameraAngleY && CameraAngleY < 225)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingRight(true);
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripLeft();
                return;
            }

            // 이동
            PlayerMoveLeft();
        }
        // 왼쪽 ←
        else if (225 <= CameraAngleY && CameraAngleY < 315)
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingRight();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripForward();
                return;
            }

            // 이동
            PlayerMoveForward();
        }
        // 앞쪽 ↑
        else
        {
            // 매달림
            if (isPlayerClimbing)
            {
                PlayerClimbingRight();
                return;
            }

            // 붙잡기
            if (isGripKeyPressed)
            {
                PlayerGripRight();
                return;
            }

            // 이동
            PlayerMoveRight();
        }
    }


    // 앞으로 이동
    private void PlayerMoveForward()
    {
        // 플레이어가 앞을 안보고 있음
        if (m_playerDirection != en_Direction.FORWARD)
        {
            // 플레이어의 방향 앞쪽으로 변경
            m_playerAction.DirectionForward();
            m_playerDirection = en_Direction.FORWARD;
            return;
        }

        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iZ + 1 >= m_mapSizeZ)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 이동 지점이 비어있나
        if (m_arrMapData[iY, iZ + 1, iX].meshData == en_MeshType.BLOCK)
        {
            // 블록

            //--------------------------------
            // 위쪽 검사
            //   ？
            // ★■
            //--------------------------------

            // 맵 바깥으로 나가면 안됨
            if (iY + 1 >= m_mapSizeY)
            {
                // 더이상 갈 수 없음
                Debug.Log("맵의 끝");
                return;
            }

            if (m_arrMapData[iY + 1, iZ + 1, iX].meshData == en_MeshType.BLOCK)
            {
                // 블록

                // 못올라감
                //--------------------------------
                // 아이템을 통해 올라갈 수 있도록...
                //--------------------------------
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------

                if (m_arrMapData[iY + 1, iZ, iX].meshData == en_MeshType.EMPTY)
                {
                    // 비어 있음

                    //--------------------------------
                    // 점프
                    // ↗
                    // ★■
                    // ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY + 1, iZ + 1, iX);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY + 1;
                    m_playerPosition.iZ = iZ + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveForwardUp();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.FORWARD;
                    }
                }
            }
        }
        else
        {
            // 비어 있음

            //--------------------------------
            // 아래쪽 검사
            // ★
            // ■？
            //--------------------------------

            if (m_arrMapData[iY - 1, iZ + 1, iX].meshData == en_MeshType.BLOCK)
            {
                // 블록

                //--------------------------------
                // 이동
                // ★→
                // ■■
                //--------------------------------

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY, iZ + 1, iX);

                // 플레이어 좌표 이동
                m_playerPosition.iZ = iZ + 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.MoveForward();
                    // 플레이어 방향
                    m_playerDirection = en_Direction.FORWARD;
                    // 플레이어 조작 불가
                    canPlayerControl = false;
                }
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 2칸 아래쪽 검사
                // ★
                // ■
                //   ？
                //--------------------------------

                // 맵 바깥으로 나가면 안됨
                if (iY - 2 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (m_arrMapData[iY - 2, iZ + 1, iX].meshData == en_MeshType.BLOCK)
                {
                    // 블록

                    //--------------------------------
                    // 이동
                    // ★
                    // ■↘
                    //   ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ + 1, iX);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iZ = iZ + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveForwardDown();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.FORWARD;
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ + 1, iX);

                    // 플레이어 왼쪽 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iZ = iZ + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.ClimbingForward();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.BACK;
                        // 등반 플래그 활성화
                        isPlayerClimbing = true;
                    }
                }
            }
        }
    }


    // 뒤로 이동
    private void PlayerMoveBack()
    {
        // 플레이어가 뒤를 안보고 있음
        if (m_playerDirection != en_Direction.BACK)
        {
            // 플레이어의 방향 뒤쪽으로 변경
            m_playerAction.DirectionBack();
            m_playerDirection = en_Direction.BACK;
            return;
        }

        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iZ - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 이동 지점이 비어있나
        if (m_arrMapData[iY, iZ - 1, iX].meshData == en_MeshType.BLOCK)
        {
            // 블록

            //--------------------------------
            // 위쪽 검사
            //   ？
            // ★■
            //--------------------------------

            // 맵 바깥으로 나가면 안됨
            if (iY + 1 >= m_mapSizeY)
            {
                // 더이상 갈 수 없음
                Debug.Log("맵의 끝");
                return;
            }

            if (m_arrMapData[iY + 1, iZ - 1, iX].meshData == en_MeshType.BLOCK)
            {
                // 블록

                // 못올라감
                //--------------------------------
                // 아이템을 통해 올라갈 수 있도록...
                //--------------------------------
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------

                if (m_arrMapData[iY + 1, iZ, iX].meshData == en_MeshType.EMPTY)
                {
                    // 비어 있음

                    //--------------------------------
                    // 점프
                    // ↗
                    // ★■
                    // ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY + 1, iZ - 1, iX);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY + 1;
                    m_playerPosition.iZ = iZ - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveBackUp();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.BACK;
                    }
                }
            }
        }
        else
        {
            // 비어 있음

            //--------------------------------
            // 아래쪽 검사
            // ★
            // ■？
            //--------------------------------

            if (m_arrMapData[iY - 1, iZ - 1, iX].meshData == en_MeshType.BLOCK)
            {
                // 블록

                //--------------------------------
                // 이동
                // ★→
                // ■■
                //--------------------------------

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY, iZ - 1, iX);

                // 플레이어 좌표 이동
                m_playerPosition.iZ = iZ - 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.MoveBack();
                    // 플레이어 방향
                    m_playerDirection = en_Direction.BACK;
                    // 플레이어 조작 불가
                    canPlayerControl = false;
                }
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 2칸 아래쪽 검사
                // ★
                // ■
                //   ？
                //--------------------------------

                // 맵 바깥으로 나가면 안됨
                if (iY - 2 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (m_arrMapData[iY - 2, iZ - 1, iX].meshData == en_MeshType.BLOCK)
                {
                    // 블록

                    //--------------------------------
                    // 이동
                    // ★
                    // ■↘
                    //   ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ - 1, iX);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iZ = iZ - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveBackDown();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.BACK;
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ - 1, iX);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iZ = iZ - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.ClimbingBack();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.FORWARD;
                        // 등반 플래그 활성화
                        isPlayerClimbing = true;
                    }
                }
            }
        }
    }


    // 왼쪽 이동
    private void PlayerMoveLeft()
    {
        // 플레이어가 왼쪽을 안보고 있음
        if (m_playerDirection != en_Direction.LEFT)
        {
            // 플레이어의 방향 왼쪽으로 변경
            m_playerAction.DirectionLeft();
            m_playerDirection = en_Direction.LEFT;
            return;
        }

        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iX - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 이동 지점이 비어있나
        if (m_arrMapData[iY, iZ, iX - 1].meshData == en_MeshType.BLOCK)
        {
            // 블록

            //--------------------------------
            // 위쪽 검사
            //   ？
            // ★■
            //--------------------------------

            // 맵 바깥으로 나가면 안됨
            if (iY + 1 >= m_mapSizeY)
            {
                // 더이상 갈 수 없음
                Debug.Log("맵의 끝");
                return;
            }

            if (m_arrMapData[iY + 1, iZ, iX - 1].meshData == en_MeshType.BLOCK)
            {
                // 블록

                // 못올라감
                //--------------------------------
                // 아이템을 통해 올라갈 수 있도록...
                //--------------------------------
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------

                if (m_arrMapData[iY + 1, iZ, iX].meshData == en_MeshType.EMPTY)
                {
                    // 비어 있음

                    //--------------------------------
                    // 점프
                    // ↗
                    // ★■
                    // ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY + 1, iZ, iX - 1);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY + 1;
                    m_playerPosition.iX = iX - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveLeftUp();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.LEFT;
                    }
                }
            }
        }
        else
        {
            // 비어 있음

            //--------------------------------
            // 아래쪽 검사
            // ★
            // ■？
            //--------------------------------

            if (m_arrMapData[iY - 1, iZ, iX - 1].meshData == en_MeshType.BLOCK)
            {
                // 블록

                //--------------------------------
                // 이동
                // ★→
                // ■■
                //--------------------------------

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY, iZ, iX - 1);

                // 플레이어 좌표 이동
                m_playerPosition.iX = iX - 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.MoveLeft();
                    // 플레이어 방향
                    m_playerDirection = en_Direction.LEFT;
                    // 플레이어 조작 불가
                    canPlayerControl = false;
                }
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 2칸 아래쪽 검사
                // ★
                // ■
                //   ？
                //--------------------------------

                // 맵 바깥으로 나가면 안됨
                if (iY - 2 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (m_arrMapData[iY - 2, iZ, iX - 1].meshData == en_MeshType.BLOCK)
                {
                    // 블록

                    //--------------------------------
                    // 이동
                    // ★
                    // ■↘
                    //   ■
                    //--------------------------------

                    // 플레이어 좌표 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ, iX - 1);

                    // 플레이어 왼쪽 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iX = iX - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveLeftDown();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.LEFT;
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 플레이어 좌표 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ, iX - 1);

                    // 플레이어 왼쪽 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iX = iX - 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.ClimbingLeft();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.RIGHT;
                        // 등반 플래그 활성화
                        isPlayerClimbing = true;
                    }
                }
            }
        }
    }


    // 오른쪽 이동
    private void PlayerMoveRight()
    {
        // 플레이어가 오른쪽을 안보고 있음
        if (m_playerDirection != en_Direction.RIGHT)
        {
            // 플레이어의 방향 오른쪽으로 변경
            m_playerAction.DirectionRight();
            m_playerDirection = en_Direction.RIGHT;
            return;
        }

        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X


        // 맵 바깥으로 나가면 안됨
        if (iX + 1 >= m_mapSizeX)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 이동 지점이 비어있나
        if (m_arrMapData[iY, iZ, iX + 1].meshData == en_MeshType.BLOCK)
        {
            // 블록

            //--------------------------------
            // 위쪽 검사
            //   ？
            // ★■
            //--------------------------------

            // 맵 바깥으로 나가면 안됨
            if (iY + 1 >= m_mapSizeY)
            {
                // 더이상 갈 수 없음
                Debug.Log("맵의 끝");
                return;
            }

            if (m_arrMapData[iY + 1, iZ, iX + 1].meshData == en_MeshType.BLOCK)
            {
                // 블록

                // 못올라감
                //--------------------------------
                // 아이템을 통해 올라갈 수 있도록...
                //--------------------------------
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------

                if (m_arrMapData[iY + 1, iZ, iX].meshData == en_MeshType.EMPTY)
                {
                    // 비어 있음

                    //--------------------------------
                    // 점프
                    // ↗
                    // ★■
                    // ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY + 1, iZ, iX + 1);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY + 1;
                    m_playerPosition.iX = iX + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveRightUp();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.RIGHT;
                    }
                }
            }
        }
        else
        {
            // 비어 있음

            //--------------------------------
            // 아래쪽 검사
            // ★
            // ■？
            //--------------------------------

            if (m_arrMapData[iY - 1, iZ, iX + 1].meshData == en_MeshType.BLOCK)
            {
                // 블록

                //--------------------------------
                // 이동
                // ★→
                // ■■
                //--------------------------------

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY, iZ, iX + 1);

                // 플레이어 좌표 이동
                m_playerPosition.iX = iX + 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.MoveRight();
                    // 플레이어 방향
                    m_playerDirection = en_Direction.RIGHT;
                    // 플레이어 조작 불가
                    canPlayerControl = false;
                }
            }
            else
            {
                // 비어 있음

                //--------------------------------
                // 2칸 아래쪽 검사
                // ★
                // ■
                //   ？
                //--------------------------------

                // 맵 바깥으로 나가면 안됨
                if (iY - 2 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (m_arrMapData[iY - 2, iZ, iX + 1].meshData == en_MeshType.BLOCK)
                {
                    // 블록

                    //--------------------------------
                    // 이동
                    // ★
                    // ■↘
                    //   ■
                    //--------------------------------

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ, iX + 1);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iX = iX + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.MoveRightDown();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.RIGHT;
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 배열의 데이터 이동
                    MoveData(iY, iZ, iX, iY - 1, iZ, iX + 1);

                    // 플레이어 좌표 이동
                    m_playerPosition.iY = iY - 1;
                    m_playerPosition.iX = iX + 1;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        // 플레이어 좌표 이동
                        m_playerAction.ClimbingRight();
                        // 플레이어 방향
                        m_playerDirection = en_Direction.LEFT;
                        // 등반 플래그 활성화
                        isPlayerClimbing = true;
                    }
                }
            }
        }
    }


    // 붙잡기 앞
    private void PlayerGripForward()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:
                // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 밀기
                    PlayerPushForward();
                }
                else
                {
                    // 앞으로 이동
                    PlayerMoveForward();
                }
                return;
            case en_Direction.BACK:
                // 뒤쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 당기기
                    PlayerPullForward();
                }
                else
                {
                    // 앞으로 이동
                    PlayerMoveForward();
                }
                return;
            case en_Direction.LEFT:
                // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 앞으로 이동
                    PlayerMoveForward();
                }
                return;
            case en_Direction.RIGHT:
                // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 앞으로 이동
                    PlayerMoveForward();
                }
                return;
        }
    }


    // 붙잡기 뒤
    private void PlayerGripBack()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:
                // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 당기기
                    PlayerPullBack();
                }
                else
                {
                    // 뒤쪽으로 이동
                    PlayerMoveBack();
                }
                return;
            case en_Direction.BACK:
                // 뒤쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 밀기
                    PlayerPushBack();
                }
                else
                {
                    // 뒤쪽으로 이동
                    PlayerMoveBack();
                }
                return;
            case en_Direction.LEFT:
                // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 뒤쪽으로 이동
                    PlayerMoveBack();
                }
                return;
            case en_Direction.RIGHT:
                // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 뒤쪽으로 이동
                    PlayerMoveBack();
                }
                return;
        }
    }


    // 붙잡기 왼쪽
    private void PlayerGripLeft()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:
                // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 왼쪽으로 이동
                    PlayerMoveLeft();
                }
                return;
            case en_Direction.BACK:
                // 뒤쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 왼쪽으로 이동
                    PlayerMoveLeft();
                }
                return;
            case en_Direction.LEFT:
                // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 밀기
                    PlayerPushLeft();
                }
                else
                {
                    // 왼쪽으로 이동
                    PlayerMoveLeft();
                }
                return;
            case en_Direction.RIGHT:
                // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 당기기
                    PlayerPullLeft();
                }
                else
                {
                    // 왼쪽으로 이동
                    PlayerMoveLeft();
                }
                return;
        }
    }


    // 붙잡기 오른쪽
    private void PlayerGripRight()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:
                // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 오른쪽으로 이동
                    PlayerMoveRight();
                }
                return;
            case en_Direction.BACK:
                // 뒤쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 조작 불가
                    return;
                }
                else
                {
                    // 오른쪽으로 이동
                    PlayerMoveRight();
                }
                return;
            case en_Direction.LEFT:
                // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 당기기
                    PlayerPullRight();
                }
                else
                {
                    // 오른쪽으로 이동
                    PlayerMoveRight();
                }
                return;
            case en_Direction.RIGHT:
                // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                // 앞쪽에 큐브가 있나
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 큐브 밀기
                    PlayerPushRight();
                }
                else
                {
                    // 오른쪽으로 이동
                    PlayerMoveRight();
                }
                return;
        }
    }


    // 플레이어 밀기 앞쪽
    private void PlayerPushForward()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        if (CubeMoveForward(iY, iZ + 1, iX))
        {
            // 밀지 못함
            return;
        }
    }

    // 플레이어 당기기 앞쪽
    private void PlayerPullForward()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iZ + 1 >= m_mapSizeZ)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 당기는 쪽이 블록인가
        if (m_arrMapData[iY, iZ + 1, iX].meshData == en_MeshType.BLOCK)
        {
            // 블록
            // 이동할 수 없음
            Debug.Log(iY + ", " + (iZ + 1) + ", " + iX + " 좌표로 이동할 수 없음");
            return;
        }

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ + 1, iX);   // 플레이어 데이터
        MoveData(iY, iZ - 1, iX, iY, iZ, iX);   // 큐브 데이터
        // 플레이어 좌표 이동
        m_playerPosition.iZ = iZ + 1;

        // 화면상의 플레이어 이동
        m_playerAction.PullForward();
        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveForward();
    }

    // 플레이어 밀기 뒤쪽
    private void PlayerPushBack()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        if (CubeMoveBack(iY, iZ - 1, iX))
        {
            // 밀지 못함
            return;
        }
    }

    // 플레이어 당기기 뒤쪽
    private void PlayerPullBack()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iZ - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 당기는 쪽이 블록인가
        if (m_arrMapData[iY, iZ - 1, iX].meshData == en_MeshType.BLOCK)
        {
            // 블록
            // 이동할 수 없음
            Debug.Log(iY + ", " + (iZ - 1) + ", " + iX + " 좌표로 이동할 수 없음");
            return;
        }

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ - 1, iX);   // 플레이어 데이터
        MoveData(iY, iZ + 1, iX, iY, iZ, iX);   // 큐브 데이터
        // 플레이어 좌표 이동
        m_playerPosition.iZ = iZ - 1;

        // 화면상의 플레이어 이동
        m_playerAction.PullBack();
        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveBack();
    }

    // 플레이어 밀기 왼쪽
    private void PlayerPushLeft()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        if (CubeMoveLeft(iY, iZ, iX - 1))
        {
            // 밀지 못함
            return;
        }
    }

    // 플레이어 당기기 왼쪽
    private void PlayerPullLeft()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iX - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 당기는 쪽이 블록인가
        if (m_arrMapData[iY, iZ, iX - 1].meshData == en_MeshType.BLOCK)
        {
            // 블록
            // 이동할 수 없음
            Debug.Log(iY + ", " + iZ + ", " + (iX - 1) + " 좌표로 이동할 수 없음");
            return;
        }

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ, iX - 1);   // 플레이어 데이터
        MoveData(iY, iZ, iX + 1, iY, iZ, iX);   // 큐브 데이터
        // 플레이어 좌표 이동
        m_playerPosition.iX = iX - 1;

        // 화면상의 플레이어 이동
        m_playerAction.PullLeft();
        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveLeft();
    }

    // 플레이어 밀기 오른쪽
    private void PlayerPushRight()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        if (CubeMoveRight(iY, iZ, iX + 1))
        {
            // 밀지 못함
            return;
        }
    }

    // 플레이어 당기기 오른쪽
    private void PlayerPullRight()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        // 맵 바깥으로 나가면 안됨
        if (iX + 1 >= m_mapSizeX)
        {
            // 더이상 갈 수 없음
            Debug.Log("맵의 끝");
            return;
        }

        // 당기는 쪽이 블록인가
        if (m_arrMapData[iY, iZ, iX + 1].meshData == en_MeshType.BLOCK)
        {
            // 블록
            // 이동할 수 없음
            Debug.Log(iY + ", " + iZ + ", " + (iX + 1) + " 좌표로 이동할 수 없음");
            return;
        }

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ, iX + 1);   // 플레이어 데이터
        MoveData(iY, iZ, iX - 1, iY, iZ, iX);   // 큐브 데이터
        // 플레이어 좌표 이동
        m_playerPosition.iX = iX + 1;

        // 화면상의 플레이어 이동
        m_playerAction.PullRight();
        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveRight();
    }


    // 플레이어 등반 오르기
    private void PlayerClimbingUp()
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:  // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (iY + 1 >= m_mapSizeY)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 위쪽 검사
                //   ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + (iZ + 1) + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY + 1, iZ + 1, iX);

                // 플레이어 좌표 이동
                m_playerPosition.iY = iY + 1;
                m_playerPosition.iZ = iZ + 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.ClimbingUpForward();
                    // 등반 플래그 활성화
                    isPlayerClimbing = false;
                }
                return;
            case en_Direction.BACK:     // 뒤쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (iY + 1 >= m_mapSizeY)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 위쪽 검사
                //   ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + (iZ - 1) + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY + 1, iZ - 1, iX);

                // 플레이어 좌표 이동
                m_playerPosition.iY = iY + 1;
                m_playerPosition.iZ = iZ - 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.ClimbingUpBack();
                    // 등반 플래그 활성화
                    isPlayerClimbing = false;
                }
                return;
            case en_Direction.LEFT:     // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (iY + 1 >= m_mapSizeY)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 위쪽 검사
                //   ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + (iX - 1) + " 좌표로 이동할 수 없음");
                    return;
                }

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY + 1, iZ, iX - 1);

                // 플레이어 좌표 이동
                m_playerPosition.iY = iY + 1;
                m_playerPosition.iX = iX - 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.ClimbingUpLeft();
                    // 등반 플래그 활성화
                    isPlayerClimbing = false;
                }
                return;
            case en_Direction.RIGHT:    // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                if (iY + 1 >= m_mapSizeY)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 위쪽 검사
                //   ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + (iX + 1) + " 좌표로 이동할 수 없음");
                    return;
                }

                //--------------------------------
                // 플레이어 위쪽 검사
                // ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY + 1, iZ, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    // 이동할 수 없음
                    Debug.Log((iY + 1) + ", " + iZ + ", " + iX + " 좌표로 이동할 수 없음");
                    return;
                }

                // 배열의 데이터 이동
                MoveData(iY, iZ, iX, iY + 1, iZ + 1, (iX + 1));

                // 플레이어 좌표 이동
                m_playerPosition.iY = iY + 1;
                m_playerPosition.iX = iX + 1;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    // 플레이어 좌표 이동
                    m_playerAction.ClimbingUpRight();
                    // 등반 플래그 활성화
                    isPlayerClimbing = false;
                }
                return;
        }
    }


    // 플레이어 등반 왼쪽
    private void PlayerClimbingLeft(bool isReverse = false)
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:  // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 왼쪽 검사
                //   ■
                // ？★
                //--------------------------------
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionLeft();
                        m_playerDirection = en_Direction.LEFT;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iZ + 1 >= m_mapSizeZ)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 왼쪽 검사
                    // ？■
                    //   ★
                    //--------------------------------
                    if (m_arrMapData[iY, iZ + 1, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ■■
                        // ←★
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ, iX - 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ↖■
                        //   ★
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX - 1);

                        // 플레이어 왼쪽 이동
                        m_playerPosition.iZ = iZ + 1;
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                            m_playerAction.ClimbingMoveForward();
                            m_playerAction.DirectionRight();
                            m_playerDirection = en_Direction.RIGHT;
                        }
                    }
                }
                return;
            case en_Direction.BACK:     // 뒤쪽

                // 키조작 반전 플래그
                if (isReverse)
                {
                    PlayerClimbingRight();
                    return;
                }

                // 맵 바깥으로 나가면 안됨
                if (iX - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 왼쪽 검사
                // ？★
                //   ■
                //--------------------------------
                if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionLeft();
                        m_playerDirection = en_Direction.LEFT;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iZ - 1 < 0)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 왼쪽 검사
                    //   ★
                    // ？■
                    //--------------------------------
                    if (m_arrMapData[iY, iZ - 1, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ←★
                        // ■■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ, iX - 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        //   ★
                        // ↙■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX - 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ - 1;
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                            m_playerAction.ClimbingMoveBack();
                            m_playerAction.DirectionRight();
                            m_playerDirection = en_Direction.RIGHT;
                        }
                    }
                }
                return;
            case en_Direction.LEFT:     // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 < 0)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 뒤쪽 검사
                // ■★
                //   ？
                //--------------------------------
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionBack();
                        m_playerDirection = en_Direction.BACK;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iZ - 1 < 0)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 뒤쪽 검사
                    // ■★
                    // ？
                    //--------------------------------
                    if (m_arrMapData[iY, iZ - 1, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ■★
                        // ■↓
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveBack();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ■★
                        // ↙
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX - 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ - 1;
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                            m_playerAction.ClimbingMoveBack();
                            m_playerAction.DirectionForward();
                            m_playerDirection = en_Direction.FORWARD;
                        }
                    }
                }
                return;
            case en_Direction.RIGHT:    // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 앞쪽 검사
                // ？
                // ★■
                //--------------------------------
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionForward();
                        m_playerDirection = en_Direction.FORWARD;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iX + 1 >= m_mapSizeX)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 앞쪽 검사
                    //   ？
                    // ★■
                    //--------------------------------
                    if (m_arrMapData[iY, iZ + 1, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ↑■
                        // ★■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveForward();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        //   ↗
                        // ★■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX + 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ + 1;
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                            m_playerAction.ClimbingMoveForward();
                            m_playerAction.DirectionBack();
                            m_playerDirection = en_Direction.BACK;
                        }
                    }
                }
                return;
        }
    }


    // 플레이어 등반 왼쪽
    private void PlayerClimbingRight(bool isReverse = false)
    {
        int iY = m_playerPosition.iY;     // 플레이어 위치 Y
        int iZ = m_playerPosition.iZ;     // 플레이어 위치 Z
        int iX = m_playerPosition.iX;     // 플레이어 위치 X

        switch (m_playerDirection)
        {
            case en_Direction.FORWARD:  // 앞쪽

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 오른쪽 검사
                // ■
                // ★？
                //--------------------------------
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionRight();
                        m_playerDirection = en_Direction.RIGHT;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iZ + 1 >= m_mapSizeZ)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 오른쪽 검사
                    // ■？
                    // ★
                    //--------------------------------
                    if (m_arrMapData[iY, iZ + 1, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ■■
                        // ★→
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ, iX + 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ■↗
                        // ★
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX + 1);

                        // 플레이어 왼쪽 이동
                        m_playerPosition.iZ = iZ + 1;
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                            m_playerAction.ClimbingMoveForward();
                            m_playerAction.DirectionLeft();
                            m_playerDirection = en_Direction.LEFT;
                        }
                    }
                }
                return;
            case en_Direction.BACK:     // 뒤쪽

                // 키조작 반전 플래그
                if (isReverse)
                {
                    PlayerClimbingLeft();
                    return;
                }

                // 맵 바깥으로 나가면 안됨
                if (iX + 1 >= m_mapSizeX)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 오른쪽 검사
                // ★？
                // ■
                //--------------------------------
                if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionRight();
                        m_playerDirection = en_Direction.RIGHT;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iZ - 1 < 0)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 오른쪽 검사
                    // ★
                    // ■？
                    //--------------------------------
                    if (m_arrMapData[iY, iZ - 1, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ★→
                        // ■■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ, iX + 1);

                        // 플레이어 좌표 이동
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ★
                        // ■↘
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX + 1);

                        // 플레이어 왼쪽 이동
                        m_playerPosition.iZ = iZ - 1;
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                            m_playerAction.ClimbingMoveBack();
                            m_playerAction.DirectionLeft();
                            m_playerDirection = en_Direction.LEFT;
                        }
                    }
                }
                return;
            case en_Direction.LEFT:     // 왼쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ + 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 앞쪽 검사
                //   ？
                // ■★
                //--------------------------------
                if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionForward();
                        m_playerDirection = en_Direction.FORWARD;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iX - 1 < 0)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 앞쪽 검사
                    // ？
                    // ■★
                    //--------------------------------
                    if (m_arrMapData[iY, iZ + 1, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ■↑
                        // ■★
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveForward();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ↖
                        // ■★
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ + 1, iX - 1);

                        // 플레이어 왼쪽 이동
                        m_playerPosition.iZ = iZ + 1;
                        m_playerPosition.iX = iX - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveLeft();
                            m_playerAction.ClimbingMoveForward();
                            m_playerAction.DirectionBack();
                            m_playerDirection = en_Direction.BACK;
                        }
                    }
                }
                return;
            case en_Direction.RIGHT:    // 오른쪽

                // 맵 바깥으로 나가면 안됨
                if (iZ - 1 >= m_mapSizeZ)
                {
                    // 더이상 갈 수 없음
                    Debug.Log("맵의 끝");
                    return;
                }

                //--------------------------------
                // 뒤쪽 검사
                // ★■
                // ？
                //--------------------------------
                if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
                {
                    if (isPlayerActive)
                    {
                        // 플레이어의 방향 왼쪽으로 변경
                        m_playerAction.DirectionBack();
                        m_playerDirection = en_Direction.BACK;
                    }
                }
                else
                {
                    // 맵 바깥으로 나가면 안됨
                    if (iX + 1 >= m_mapSizeX)
                    {
                        // 더이상 갈 수 없음
                        Debug.Log("맵의 끝");
                        return;
                    }

                    //--------------------------------
                    // 큐브 뒤쪽 검사
                    // ★■
                    //   ？
                    //--------------------------------
                    if (m_arrMapData[iY, iZ - 1, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
                    {
                        //--------------------------------
                        // 이동
                        // ★■
                        // ↓■
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX);

                        // 플레이어 좌표 이동
                        m_playerPosition.iZ = iZ - 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveBack();
                        }
                    }
                    else
                    {
                        //--------------------------------
                        // 이동
                        // ★■
                        //   ↘
                        //--------------------------------

                        // 배열의 데이터 이동
                        MoveData(iY, iZ, iX, iY, iZ - 1, iX + 1);

                        // 플레이어 왼쪽 이동
                        m_playerPosition.iZ = iZ - 1;
                        m_playerPosition.iX = iX + 1;

                        if (isPlayerActive)
                        {
                            // 플레이어 좌표 이동
                            m_playerAction.ClimbingMoveRight();
                            m_playerAction.ClimbingMoveBack();
                            m_playerAction.DirectionForward();
                            m_playerDirection = en_Direction.FORWARD;
                        }
                    }
                }
                return;
        }
    }


    // 큐브 앞쪽 이동
    private bool CubeMoveForward(int iY, int iZ, int iX)
    {
        // 맵 바깥으로 나가면 안됨
        if (iZ + 1 >= m_mapSizeZ)
        {
            // 더이상 갈 수 없음
            Debug.Log("큐브가 더이상 이동할 수 없음");
            return false;
        }

        // 앞쪽에 큐브가 있나
        if (m_arrMapData[iY, iZ + 1, iX].objectLayer == en_GameObjectLayer.CUBE)
        {
            // 이동 방향의 큐브도 이동처리
            if (!CubeMoveForward(iY, iZ + 1, iX))
            {
                //--------------------------------
                // 결과 false (이동할 수 없다면)
                // 이 큐브도 이동할 수 없다
                //--------------------------------
                return false;
            }
        }

        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveForward();

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ + 1, iX);

        return true;
    }

    // 큐브 뒤쪽 이동
    private bool CubeMoveBack(int iY, int iZ, int iX)
    {
        // 맵 바깥으로 나가면 안됨
        if (iZ - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("큐브가 더이상 이동할 수 없음");
            return false;
        }

        // 앞쪽에 큐브가 있나
        if (m_arrMapData[iY, iZ - 1, iX].objectLayer == en_GameObjectLayer.CUBE)
        {
            // 이동 방향의 큐브도 이동처리
            if (!CubeMoveBack(iY, iZ - 1, iX))
            {
                //--------------------------------
                // 결과 false (이동할 수 없다면)
                // 이 큐브도 이동할 수 없다
                //--------------------------------
                return false;
            }
        }

        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveBack();

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ - 1, iX);

        return true;
    }

    // 큐브 왼쪽 이동
    private bool CubeMoveLeft(int iY, int iZ, int iX)
    {
        // 맵 바깥으로 나가면 안됨
        if (iX - 1 < 0)
        {
            // 더이상 갈 수 없음
            Debug.Log("큐브가 더이상 이동할 수 없음");
            return false;
        }

        // 앞쪽에 큐브가 있나
        if (m_arrMapData[iY, iZ, iX - 1].objectLayer == en_GameObjectLayer.CUBE)
        {
            // 이동 방향의 큐브도 이동처리
            if (!CubeMoveLeft(iY, iZ, iX - 1))
            {
                //--------------------------------
                // 결과 false (이동할 수 없다면)
                // 이 큐브도 이동할 수 없다
                //--------------------------------
                return false;
            }
        }

        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveLeft();

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ, iX - 1);

        return true;
    }

    // 큐브 오른쪽 이동
    private bool CubeMoveRight(int iY, int iZ, int iX)
    {
        // 맵 바깥으로 나가면 안됨
        if (iX + 1 >= m_mapSizeX)
        {
            // 더이상 갈 수 없음
            Debug.Log("큐브가 더이상 이동할 수 없음");
            return false;
        }

        // 앞쪽에 큐브가 있나
        if (m_arrMapData[iY, iZ, iX + 1].objectLayer == en_GameObjectLayer.CUBE)
        {
            // 이동 방향의 큐브도 이동처리
            if (!CubeMoveRight(iY, iZ, iX + 1))
            {
                //--------------------------------
                // 결과 false (이동할 수 없다면)
                // 이 큐브도 이동할 수 없다
                //--------------------------------
                return false;
            }
        }

        // 화면상의 큐브 이동
        m_arrMapData[iY, iZ, iX].actionScript.MoveRight();

        // 배열의 데이터 이동
        MoveData(iY, iZ, iX, iY, iZ, iX + 1);

        return true;
    }


    // 배열의 게임 오브젝트 이동
    private void MoveData(int iY, int iZ, int iX, int iDestY, int iDestZ, int iDestX)
    {
        string objectName;

        // 이동 지점 위치에 큐브 정보 입력
        m_arrMapData[iDestY, iDestZ, iDestX].objectTag = m_arrMapData[iY, iZ, iX].objectTag;
        m_arrMapData[iDestY, iDestZ, iDestX].objectLayer = m_arrMapData[iY, iZ, iX].objectLayer;
        m_arrMapData[iDestY, iDestZ, iDestX].meshData = m_arrMapData[iY, iZ, iX].meshData;
        m_arrMapData[iDestY, iDestZ, iDestX].gameObject = m_arrMapData[iY, iZ, iX].gameObject;
        m_arrMapData[iDestY, iDestZ, iDestX].actionScript = m_arrMapData[iY, iZ, iX].actionScript;

        // 태그 정보로 게임 오브젝트 이름 지정
        switch (m_arrMapData[iY, iZ, iX].objectTag)
        {
            case en_GameObjectTag.PLAYER:
                objectName = "Player";
                break;
            case en_GameObjectTag.DEST:
                objectName = "Dest";
                break;
            case en_GameObjectTag.NORMAL_CUBE:
                objectName = "NormalCube";
                break;
            case en_GameObjectTag.ICE_CUBE:
                objectName = "IceCube";
                break;
            default:
                objectName = "Unknown";
                break;
        }

        m_arrMapData[iDestY, iDestZ, iDestX].gameObject.name = objectName + " [" + iDestY + ", " + iDestZ + ", " + iDestX + "]";

        // 이전 위치의 큐브 정보 삭제
        m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
        m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
        m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
        m_arrMapData[iY, iZ, iX].gameObject = null;
        m_arrMapData[iY, iZ, iX].actionScript = null;
    }
}
