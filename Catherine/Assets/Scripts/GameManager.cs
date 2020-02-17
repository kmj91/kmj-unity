using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using GameMessageScript;
using UnityDequeScript;

public class GameManager : MonoBehaviour
{
    public Queue<GameMessage> messageQueue;     // 게임 매니저 큐
    public UnityDeque<UndoData> undoDeque;

    private int undoArraySize;                  // 배열 크기
    private PlayerMovement playerMovement;      // 플레이어 무브먼트
    private GameObject GameOverUI;
    private bool restartFlag;


    private void Start()
    {
        // 플레이어 무브먼트
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        // UI
        GameOverUI = GameObject.Find("Canvas").transform.Find("gameOverUI").gameObject;
        // 다시시작 플래그
        restartFlag = false;
        // 초기화
        messageQueue = new Queue<GameMessage>();
        undoDeque = new UnityDeque<UndoData>();
    }

    private void Update()
    {
        GameMessage gameMsg;
        UndoDataMsg UndoMsg;
        UndoData undoData;          // 되돌리기 정보
        CubeMovement cubeMovement;

        if (restartFlag)
        {
            // 씬 다시 불러오기
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("SampleScene");
            }
        }

        if (playerMovement.isDeath)
        {
            // 게임오버
            GameOverUI.SetActive(true);
            // 다시 시작
            restartFlag = true;
        }

        // 되돌리기 테스트
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Deque가 비어있지 않으면 되돌리기
            if (undoDeque.Count > 0)
            {
                undoData = undoDeque.Pop_Back();

                Debug.Log("---------------------------------------");
                Debug.Log("player : " + undoData.playerPos);
                for (int i = 0; i < undoArraySize; ++i)
                {
                    if (!undoData.cubePosArray[i].flag)
                    {
                        break;
                    }
                    Debug.Log("cube : " + undoData.cubePosArray[i].cubeObject);
                    Debug.Log("pos : " + undoData.cubePosArray[i].CubePos);
                }

                //-----------------------------------------------
                // 1. 플레이어 애니메이션도 대기상태로 복구해야됨
                //-----------------------------------------------

                // 플레이어 위치 되돌리기
                playerMovement.setPlayerPostion(undoData.playerPos);
                // 플레이어 대기 상태로
                playerMovement.UpdateStateToIdle();

                // 큐브 되돌리기
                for (int i = 0; i < undoArraySize; ++i)
                {
                    if (!undoData.cubePosArray[i].flag)
                    {
                        break;
                    }

                    cubeMovement = undoData.cubePosArray[i].cubeObject.GetComponent<CubeMovement>();

                    // 큐브 위치 되돌리기
                    //undoData.cubePosArray[i].cubeObject.transform.position = undoData.cubePosArray[i].CubePos;
                    cubeMovement.transform.position = undoData.cubePosArray[i].CubePos;
                    // 큐브 대기 상태로
                    //undoData.cubePosArray[i].cubeObject.UpdateStateToIdle();
                    cubeMovement.UpdateStateToIdle();
                }
            }
        }


        // 처리할 메시지
        if (messageQueue.Count != 0)
        {
            gameMsg = messageQueue.Peek() as GameMessage;

            Debug.Log(gameMsg.messageType);

            switch (gameMsg.messageType)
            {
                case msgType.UNDO:

                    UndoMsg = messageQueue.Dequeue() as UndoDataMsg;

                    // 플레이어 위치
                    undoData.playerPos = UndoMsg.playerPos;
                    // 큐브 위치들
                    //for (int i = 0; i < UndoMsg.cubePosArray.Length; ++i)
                    //{
                    //    if (!UndoMsg.cubePosArray[i].flag)
                    //    {
                    //        break;
                    //    }
                    //    undoData.cubePosArray[i] = UndoMsg.cubePosArray[i];
                    //}

                    // 배열 크기
                    undoArraySize = 20;
                    undoData.cubePosArray = new CubePosData[undoArraySize];
                    UndoMsg.cubePosArray.CopyTo(undoData.cubePosArray, 0);

                    // 되돌리기 저장
                    undoDeque.Push_Back(undoData);


                    Debug.Log("player : " + UndoMsg.playerPos);
                    for (int i = 0; i < UndoMsg.cubePosArray.Length; ++i)
                    {
                        if (!UndoMsg.cubePosArray[i].flag)
                        {
                            break;
                        }
                        Debug.Log("cube : " + UndoMsg.cubePosArray[i].cubeObject);
                        Debug.Log("pos : " + UndoMsg.cubePosArray[i].CubePos);
                    }

                    break;
            }
        }
    }
}
