using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using GameGlobalScript;

public class PlayerAction : GameScript
{
    public float m_speed;                           // 캐릭터 이동 속도

    private Vector3 m_destPosition;                 // 이동 목표 좌표 
    private en_PlayerState m_playerState;           // 플레이어 상태
    private Action[] m_arrPlayerStateProc;          // 플레이어 상태 처리 함수 배열
    private GameManager m_gameManager;              // 게임 매니저

    // 초기화
    public void Init(GameManager gameManager, float speed)
    {
        m_scriptType = en_ActionScriptType.PLAYER;
        m_gameManager = gameManager;
        m_speed = speed;
    }

    public override void MoveForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞으로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD;
    }

    public void MoveForwardUp(int jumpPower = 1)
    {
        // 좌표
        transform.position = transform.position + Vector3.forward + (Vector3.up * jumpPower);
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void MoveForwardDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public override void MoveBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽으로 이동
        m_playerState = en_PlayerState.MOVE_BACK;
    }

    public void MoveBackUp(int jumpPower = 1)
    {
        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void MoveBackDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public override void MoveLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽으로 이동
        m_playerState = en_PlayerState.MOVE_LEFT;
    }

    public void MoveLeftUp(int jumpPower = 1)
    {
        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void MoveLeftDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public override void MoveRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 오른쪽으로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT;
    }

    public void MoveRightUp(int jumpPower = 1)
    {
        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void MoveRightDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    // 당기기 앞쪽
    public void PullForward()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward;
    }

    // 당기기 뒤쪽
    public void PullBack()
    {
        // 좌표
        transform.position = transform.position + Vector3.back;
    }

    // 당기기 왼쪽
    public void PullLeft()
    {
        // 좌표
        transform.position = transform.position + Vector3.left;
    }

    // 당기기 오른쪽
    public void PullRight()
    {
        // 좌표
        transform.position = transform.position + Vector3.right;
    }


    // 등반 앞쪽
    public void ClimbingForward()
    {
        //--------------------------------
        // ★    < 플레이어
        // ■    < 큐브
        //--------------------------------

        // 좌표
        transform.position = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    // 등반 뒤쪽
    public void ClimbingBack()
    {
        //--------------------------------
        // ■    < 큐브
        // ★    < 플레이어
        //--------------------------------

        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    // 등반 왼쪽
    public void ClimbingLeft()
    {
        //--------------------------------
        // ★■    < 큐브
        // ^ 플레이어
        //--------------------------------

        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    // 등반 오른쪽
    public void ClimbingRight()
    {
        //--------------------------------
        // ■★    < 플레이어
        // ^ 큐브
        //--------------------------------

        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }


    // 등반 오르기 앞쪽
    public void ClimbingUpForward()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward + Vector3.up;
    }

    // 등반 오르기 뒤쪽
    public void ClimbingUpBack()
    {
        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.up;
    }

    // 등반 오르기 왼쪽
    public void ClimbingUpLeft()
    {
        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.up;
    }

    // 등반 오르기 오른쪽
    public void ClimbingUpRight()
    {
        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.up;
    }


    // 등반 이동 앞쪽
    public void ClimbingMoveForward()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward;
    }

    // 등반 이동 뒤쪽
    public void ClimbingMoveBack()
    {
        // 좌표
        transform.position = transform.position + Vector3.back;
    }

    // 등반 이동 왼쪽
    public void ClimbingMoveLeft()
    {
        // 좌표
        transform.position = transform.position + Vector3.left;
    }

    // 등반 이동 오른쪽
    public void ClimbingMoveRight()
    {
        // 좌표
        transform.position = transform.position + Vector3.right;
    }


    // 방향 앞으로
    public void DirectionForward()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    // 방향 뒤로
    public void DirectionBack()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    // 방향 왼쪽
    public void DirectionLeft()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    // 방향 오른쪽
    public void DirectionRight()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }


    private void Awake()
    {
        // 상태 초기화
        m_playerState = en_PlayerState.STAY;

        m_arrPlayerStateProc = new Action[]
        {
            Stay,
            Forward,
            Back,
            Left,
            Right,
            Up,
            Down
        };
    }


    private void Update()
    {
        // 상태 처리
        m_arrPlayerStateProc[(int)m_playerState]();
    }


    private void Stay()
    {

    }

    private void Forward()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.z <= transform.position.z)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
        }
    }

    private void Back()
    {
        // 뒤쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.z >= transform.position.z)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
        }
    }

    private void Left()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.x >= transform.position.x)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
        }
    }

    private void Right()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.x <= transform.position.x)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
        }
    }

    private void Up()
    {

    }

    private void Down()
    {

    }
}
