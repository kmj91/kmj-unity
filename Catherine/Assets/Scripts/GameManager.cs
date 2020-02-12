using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Queue<GameMessage> messageQueue;     // 게임 매니저 큐

    private PlayerMovement playerMovement;      // 플레이어 무브먼트
    private GameObject GameOverUI;
    private bool restartFlag;

    public enum msgType {
        UNDO
    }

    public class GameMessage
    {
        public msgType messageType;     // 메시지 타입
    }

    public class UndoStackData : GameMessage
    {
        public UndoStackData(in Vector3 setPlayerPos, in Stack<Vector3> setCubePosStack)
        {
            cubePosStack = setCubePosStack;
            playerPos = setPlayerPos;
            messageType = msgType.UNDO;
        }

        public Vector3 playerPos;
        public Stack<Vector3> cubePosStack;
    }

    public class GameOver : GameMessage
    {
        public byte gameOverType;
    }


    private void Start()
    {
        // 플레이어 무브먼트
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        // UI
        GameOverUI = GameObject.Find("Canvas").transform.Find("gameOverUI").gameObject;
        // 다시시작 플래그
        restartFlag = false;
        // 메시지 큐 초기화
        messageQueue = new Queue<GameMessage>();
    }

    private void Update()
    {
        GameMessage gameMsg;
        UndoStackData UndoStackMsg;

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

        // 처리할 메시지
        if (messageQueue.Count != 0)
        {
            gameMsg = messageQueue.Peek() as GameMessage;

            Debug.Log(gameMsg.messageType);

            switch (gameMsg.messageType)
            {
                case msgType.UNDO:

                    UndoStackMsg = messageQueue.Dequeue() as UndoStackData;

                    Debug.Log(UndoStackMsg.playerPos);
                    while (UndoStackMsg.cubePosStack.Count != 0)
                    {
                        Debug.Log(UndoStackMsg.cubePosStack.Pop());
                    }

                    break;
            }
        }
    }
}
