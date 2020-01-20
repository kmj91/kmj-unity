using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerMovement playerMovement;  // 플레이어 무브먼트
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
    }

    private void Update()
    {
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
    }
}
