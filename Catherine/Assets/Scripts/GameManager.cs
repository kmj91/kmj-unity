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
    public int undoToken;                           // 복원 지점 토큰
    public Queue<GameMessage> messageQueue;         // 게임 매니저 큐
    public UnityDeque<UndoData> undoDeque;
    public GameObject gameStage;                    // 생성된 게임 오브젝트가 자식으로 들어갈 부모
    public GameObject playerPrefab;                 // 플레이어 프리팹
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject iceCubePrefab;                // 얼음 큐브 프리팹
    public GameObject followCam;                    // 카메라
    public bool canPlayerControl;                   // 플레이어 조작 플래그

    //--------------------------------
    // private 변수
    //--------------------------------

    private int undoArraySize;                      // 배열 크기
    private int m_mapSizeY;                         // 맵 크기 Y축
    private int m_mapSizeZ;                         // 맵 크기 Z축
    private int m_mapSizeX;                         // 맵 크기 X축
    private int m_iJumpPower;
    private st_IndexPos playerPosition;             // 맵에 생성된 플레이어 오브젝트 위치 (배열 인덱스 기준)
    private st_IndexPos backupPlayerPosition;
    private st_IndexPos destPosition;                // 맵에 생성된 목적지 오브젝트 위치
    //private PlayerMovement playerMovement;          // 플레이어 무브먼트
    private GameObject m_playerObject;              // 플레이어 오브젝트
    private GameObject GameOverUI;
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
    }


    private void Start()
    {
        // 플레이어 무브먼트
        //playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        // UI
        GameOverUI = GameObject.Find("Canvas").transform.Find("gameOverUI").gameObject;
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
        if (playerPosition != backupPlayerPosition)
        {
            backupPlayerPosition = playerPosition;

            Debug.Log("iY : " + playerPosition.iY + ", iZ : " + playerPosition.iZ + ", iX : " + playerPosition.iX);
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
        playerPosition = mapData.playerPosition;
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
                            m_playerObject = Instantiate<GameObject>(playerPrefab);
                            m_arrMapData[iY, iZ, iX].gameObject = m_playerObject;
                            m_arrMapData[iY, iZ, iX].gameObject.name = "Player [" + iY + ", " + iZ + ", " + iX + "]";
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            m_arrMapData[iY, iZ, iX].gameObject.AddComponent<PlayerAction>();
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.PLAYER;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.PLAYER;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
                            // 플레이어 액션 스크립트
                            m_playerAction = m_playerObject.GetComponent<PlayerAction>();
                            // 초기화
                            m_playerAction.Init(this, playerSpeed);
                            // 플레이어 조작 플래그
                            canPlayerControl = true;
                            break;
                        case en_MenuElementType.DEST:
                            // 목적지
                            m_arrMapData[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            m_arrMapData[iY, iZ, iX].gameObject.name = "Dest [" + iY + ", " + iZ + ", " + iX + "]";
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            m_arrMapData[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.DEST;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.DEST;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.EMPTY;
                            break;
                        case en_MenuElementType.NORMAL_CUBE:
                            // 일반 큐브
                            m_arrMapData[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                            m_arrMapData[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            m_arrMapData[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + normalCubePrefab.transform.position;
                            m_arrMapData[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.NORMAL_CUBE;
                            m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.CUBE;
                            m_arrMapData[iY, iZ, iX].meshData = en_MeshType.BLOCK;
                            //arrMapToolObject[iY, iZ, iX].gameObject.AddComponent<CubeMovement>();
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
                            //arrMapToolObject[iY, iZ, iX].gameObject.AddComponent<CubeMovement>();
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
        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

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


    // 방향키 아래
    private void Down()
    {
        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

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


    // 방향키 왼쪽
    private void Left()
    {
        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

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


    // 방향키 오른쪽
    private void Right()
    {
        // 플레이어 조작 불가
        if (!canPlayerControl)
        {
            return;
        }

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


    // 앞으로 이동
    private void PlayerMoveForward()
    {
        int iY = playerPosition.iY;     // 플레이어 위치 Y
        int iZ = playerPosition.iZ;     // 플레이어 위치 Z
        int iX = playerPosition.iX;     // 플레이어 위치 X

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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY + 1, iZ + 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY + 1, iZ + 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY + 1, iZ + 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY + 1) + ", " + (iZ + 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY + 1;
                    playerPosition.iZ = iZ + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveForwardUp();
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

                // 이동 지점 위치에 플레이어 정보 입력
                m_arrMapData[iY, iZ + 1, iX].objectTag = en_GameObjectTag.PLAYER;
                m_arrMapData[iY, iZ + 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                m_arrMapData[iY, iZ + 1, iX].gameObject = m_playerObject;
                m_playerObject.name = "Player [" + iY + ", " + (iZ + 1) + ", " + iX + "]";

                // 플레이어 왼쪽 이동
                playerPosition.iZ = iZ + 1;

                // 이전 위치의 플레이어 정보 삭제
                m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                m_arrMapData[iY, iZ, iX].gameObject = null;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    m_playerAction.MoveForward();
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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ + 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ + 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ + 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + (iZ + 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iZ = iZ + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveForwardDown();
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ + 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ + 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ + 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + (iZ + 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iZ = iZ + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveForwardDown();
                    }
                }
            }
        }
    }


    // 뒤로 이동
    private void PlayerMoveBack()
    {
        int iY = playerPosition.iY;     // 플레이어 위치 Y
        int iZ = playerPosition.iZ;     // 플레이어 위치 Z
        int iX = playerPosition.iX;     // 플레이어 위치 X

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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY + 1, iZ - 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY + 1, iZ - 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY + 1, iZ - 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY + 1) + ", " + (iZ - 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY + 1;
                    playerPosition.iZ = iZ - 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveBackUp();
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

                // 이동 지점 위치에 플레이어 정보 입력
                m_arrMapData[iY, iZ - 1, iX].objectTag = en_GameObjectTag.PLAYER;
                m_arrMapData[iY, iZ - 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                m_arrMapData[iY, iZ - 1, iX].gameObject = m_playerObject;
                m_playerObject.name = "Player [" + iY + ", " + (iZ - 1) + ", " + iX + "]";

                // 플레이어 왼쪽 이동
                playerPosition.iZ = iZ - 1;

                // 이전 위치의 플레이어 정보 삭제
                m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                m_arrMapData[iY, iZ, iX].gameObject = null;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    m_playerAction.MoveBack();
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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ - 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ - 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ - 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + (iZ - 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iZ = iZ - 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveBackDown();
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ - 1, iX].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ - 1, iX].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ - 1, iX].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + (iZ - 1) + ", " + iX + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iZ = iZ - 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveBackDown();
                    }
                }
            }
        }
    }


    // 왼쪽 이동
    private void PlayerMoveLeft()
    {
        int iY = playerPosition.iY;     // 플레이어 위치 Y
        int iZ = playerPosition.iZ;     // 플레이어 위치 Z
        int iX = playerPosition.iX;     // 플레이어 위치 X

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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY + 1, iZ, iX - 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY + 1, iZ, iX - 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY + 1, iZ, iX - 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY + 1) + ", " + iZ + ", " + (iX - 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY + 1;
                    playerPosition.iX = iX - 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveLeftUp();
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

                // 이동 지점 위치에 플레이어 정보 입력
                m_arrMapData[iY, iZ, iX - 1].objectTag = en_GameObjectTag.PLAYER;
                m_arrMapData[iY, iZ, iX - 1].objectLayer = en_GameObjectLayer.PLAYER;
                m_arrMapData[iY, iZ, iX - 1].gameObject = m_playerObject;
                m_playerObject.name = "Player [" + iY + ", " + iZ + ", " + (iX - 1) + "]";

                // 플레이어 왼쪽 이동
                playerPosition.iX = iX - 1;

                // 이전 위치의 플레이어 정보 삭제
                m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                m_arrMapData[iY, iZ, iX].gameObject = null;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    m_playerAction.MoveLeft();
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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ, iX - 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX - 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX - 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + iZ + ", " + (iX - 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iX = iX - 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveLeftDown();
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ, iX - 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX - 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX - 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + iZ + ", " + (iX - 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iX = iX + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveLeftDown();
                    }
                }
            }
        }
    }


    // 오른쪽 이동
    private void PlayerMoveRight()
    {
        int iY = playerPosition.iY;     // 플레이어 위치 Y
        int iZ = playerPosition.iZ;     // 플레이어 위치 Z
        int iX = playerPosition.iX;     // 플레이어 위치 X


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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY + 1, iZ, iX + 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY + 1, iZ, iX + 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY + 1, iZ, iX + 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY + 1) + ", " + iZ + ", " + (iX + 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY + 1;
                    playerPosition.iX = iX + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveRightUp();
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

                // 이동 지점 위치에 플레이어 정보 입력
                m_arrMapData[iY, iZ, iX + 1].objectTag = en_GameObjectTag.PLAYER;
                m_arrMapData[iY, iZ, iX + 1].objectLayer = en_GameObjectLayer.PLAYER;
                m_arrMapData[iY, iZ, iX + 1].gameObject = m_playerObject;
                m_playerObject.name = "Player [" + iY + ", " + iZ + ", " + (iX + 1) + "]";

                // 플레이어 왼쪽 이동
                playerPosition.iX = iX + 1;

                // 이전 위치의 플레이어 정보 삭제
                m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                m_arrMapData[iY, iZ, iX].gameObject = null;

                // 화면상의 플레이어 이동
                if (isPlayerActive)
                {
                    m_playerAction.MoveRight();
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

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ, iX + 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX + 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX + 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + iZ + ", " + (iX + 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iX = iX + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveRightDown();
                    }
                }
                else
                {
                    // 비어 있음

                    // 매달림

                    // 이동 지점 위치에 플레이어 정보 입력
                    m_arrMapData[iY - 1, iZ, iX + 1].objectTag = en_GameObjectTag.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX + 1].objectLayer = en_GameObjectLayer.PLAYER;
                    m_arrMapData[iY - 1, iZ, iX + 1].gameObject = m_playerObject;
                    m_playerObject.name = "Player [" + (iY - 1) + ", " + iZ + ", " + (iX + 1) + "]";

                    // 플레이어 왼쪽 이동
                    playerPosition.iY = iY - 1;
                    playerPosition.iX = iX + 1;

                    // 이전 위치의 플레이어 정보 삭제
                    m_arrMapData[iY, iZ, iX].objectTag = en_GameObjectTag.EMPTY;
                    m_arrMapData[iY, iZ, iX].objectLayer = en_GameObjectLayer.EMPTY;
                    m_arrMapData[iY, iZ, iX].gameObject = null;

                    // 화면상의 플레이어 이동
                    if (isPlayerActive)
                    {
                        m_playerAction.MoveRightDown();
                    }
                }
            }
        }
    }


    // 붙잡기 앞
    private void PlayerGripForward()
    {

    }


    // 붙잡기 뒤
    private void PlayerGripBack()
    {

    }


    // 붙잡기 왼쪽
    private void PlayerGripLeft()
    {

    }


    // 붙잡기 오른쪽
    private void PlayerGripRight()
    {

    }
}
