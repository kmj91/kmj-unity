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
        UNDO = 10
    }

    public class GameMessage
    {
        public GameMessage() { }

        public msgType messageType;
        public short messageSize;
    }

    public class Undo : GameMessage
    {
        public Undo(msgType setMsgType, Vector3 setPlayerPos, Vector3 setCubePos)
        {
            cubePos = new Queue<Vector3>();

            messageType = setMsgType;
            playerPos = setPlayerPos;
            cubePos.Enqueue(setCubePos);
        }

        public Vector3 playerPos;
        public Queue<Vector3> cubePos;
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

        // 테스트
        messageQueue = new Queue<GameMessage>();
    }

    private void Update()
    {
        GameMessage gameMsg;

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

        if (messageQueue.Count != 0)
        {
            gameMsg = messageQueue.Peek() as GameMessage;

            Debug.Log(gameMsg.messageType);

            switch (gameMsg.messageType)
            {
                case msgType.UNDO:

                    var test = messageQueue.Dequeue() as Undo;

                    Debug.Log(test.playerPos);
                    while (test.cubePos.Count != 0)
                    {
                        Debug.Log(test.cubePos.Dequeue());
                    }

                    break;
            }
        }
    }
}
