using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GameMessageScript;
using UnityDequeScript;
using MapToolGlobalScript;
using System;

public class GameManager : MonoBehaviour
{
    //--------------------------------
    // public 변수
    //--------------------------------

    public int undoToken;                           // 복원 지점 토큰
    public Queue<GameMessage> messageQueue;         // 게임 매니저 큐
    public UnityDeque<UndoData> undoDeque;
    public GameObject gameStage;                    // 생성된 게임 오브젝트가 자식으로 들어갈 부모
    public GameObject playerPrefab;                 // 플레이어 프리팹
    public GameObject normalCubePrefab;             // 일반 큐브 프리팹
    public GameObject iceCubePrefab;                // 얼음 큐브 프리팹

    //--------------------------------
    // private 변수
    //--------------------------------

    private int undoArraySize;                      // 배열 크기
    private int mapSizeY;                           // 맵 크기 Y축
    private int mapSizeZ;                           // 맵 크기 Z축
    private int mapSizeX;                           // 맵 크기 X축
    private st_IndexPos playerPostion;              // 맵에 생성된 플레이어 오브젝트 위치 (배열 인덱스 기준)
    private st_IndexPos destPostion;                // 맵에 생성된 목적지 오브젝트 위치
    //private PlayerMovement playerMovement;          // 플레이어 무브먼트
    private GameObject m_playerObject;              // 플레이어 오브젝트
    private GameObject GameOverUI;
    private PlayerAction m_playerAction;            // 플레이어 액션 스크립트
    private st_MapObjectData[,,] arrMapObject;      // 맵 오브젝트 배열
    private Action[] arrMsgProc;                    // 메시지 함수 배열
    private KeyCode[] arrKeyCode;                   // 키 코드 배열
    private Action[] arrKeyProc;                    // 키 처리 함수 배열
    private bool restartFlag;


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
        arrKeyCode = new KeyCode[] 
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
        arrKeyProc = new Action[] 
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
    }

    private void Update()
    {
        GameMessage gameMsg;
        UndoData undoData;          // 되돌리기 정보
        CubeMovement cubeMovement;

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
        
    }


    // 게임 초기화
    private void InitGame()
    {
        int iX;
        int iZ;
        int iY;
        st_MapData mapData;
        Stream rs;
        BinaryFormatter deserializer;

        // 파일 불러오기
        rs = new FileStream("a.dat", FileMode.Open);
        deserializer = new BinaryFormatter();

        // 맵 정보 불러오기
        mapData = (st_MapData)deserializer.Deserialize(rs);
        // 맵 정보 저장
        mapSizeY = mapData.iMapSizeY;
        mapSizeZ = mapData.iMapSizeZ;
        mapSizeX = mapData.iMapSizeX;
        playerPostion = mapData.playerPostion;
        destPostion = mapData.destPostion;

        // 오브젝트 배열 생성
        // Y, Z, X
        arrMapObject = new st_MapObjectData[mapSizeY, mapSizeZ, mapSizeX];

        arrMapObject = (st_MapObjectData[,,])deserializer.Deserialize(rs);
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
                        case en_MenuElementType.PLAYER:
                            // 플레이어
                            m_playerObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject = m_playerObject;
                            arrMapObject[iY, iZ, iX].gameObject.name = "Player [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.AddComponent<PlayerAction>();
                            // 플레이어 액션 스크립트
                            m_playerAction = m_playerObject.GetComponent<PlayerAction>();
                            break;
                        case en_MenuElementType.DEST:
                            // 목적지
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(playerPrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "Dest [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = Color.black;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + playerPrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            break;
                        case en_MenuElementType.NORMAL_CUBE:
                            // 일반 큐브
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(normalCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "NormalCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + normalCubePrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            arrMapObject[iY, iZ, iX].gameObject.AddComponent<CubeMovement>();
                            break;
                        case en_MenuElementType.ICE_CUBE:
                            // 얼음 큐브
                            arrMapObject[iY, iZ, iX].gameObject = Instantiate<GameObject>(iceCubePrefab);
                            arrMapObject[iY, iZ, iX].gameObject.name = "IceCube [" + iY + ", " + iZ + ", " + iX + "]";
                            arrMapObject[iY, iZ, iX].color = arrMapObject[iY, iZ, iX].gameObject.GetComponent<MeshRenderer>().material.color;
                            arrMapObject[iY, iZ, iX].gameObject.transform.position = new Vector3(iX, iY, iZ) + iceCubePrefab.transform.position;
                            arrMapObject[iY, iZ, iX].gameObject.transform.parent = gameStage.transform;
                            arrMapObject[iY, iZ, iX].gameObject.AddComponent<CubeMovement>();
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


    // 방향키 위
    private void Up()
    {
        
    }


    // 방향키 아래
    private void Down()
    {

    }


    // 방향키 왼쪽
    private void Left()
    {

    }


    // 방향키 오른쪽
    private void Right()
    {

    }
}
