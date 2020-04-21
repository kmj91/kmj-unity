﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using GameGlobalScript;

public class PlayerAction : GameScript
{
    public float m_speed;                           // 캐릭터 이동 속도

    private Vector3 m_destPosition;                 // 이동 목표 좌표
    private Quaternion m_destRotation;              // 회전 목표 값
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

    public void MoveForwardClimbingUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + (Vector3.up * jumpPower);
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMBING_UP;
    }

    public void MoveForwardClimbingDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞쪽 아래로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMBING_DOWN;
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

    public void MoveBackClimbingUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_BACK_CLIMBING_UP;
    }

    public void MoveBackClimbingDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽 아래로 이동
        m_playerState = en_PlayerState.MOVE_BACK_CLIMBING_DOWN;
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

    public void MoveLeftClimbingUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMBING_UP;
    }

    public void MoveLeftClimbingDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMBING_DOWN;
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

    public void MoveRightClimbingUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMBING_UP;
    }

    public void MoveRightClimbingDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMBING_DOWN;
    }

    // 당기기 앞쪽
    public void PullForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 플레이어 상태 앞쪽 이동
        m_playerState = en_PlayerState.MOVE_FORWARD;
    }

    // 당기기 뒤쪽
    public void PullBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 플레이어 상태 뒤쪽 이동
        m_playerState = en_PlayerState.MOVE_BACK;
    }

    // 당기기 왼쪽
    public void PullLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 플레이어 상태 왼쪽 이동
        m_playerState = en_PlayerState.MOVE_LEFT;
    }

    // 당기기 오른쪽
    public void PullRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 플레이어 상태 오른쪽 이동
        m_playerState = en_PlayerState.MOVE_RIGHT;
    }


    // 등반 앞쪽
    public void MoveForwardClimbingState()
    {
        //--------------------------------
        // ★    < 플레이어
        // ■    < 큐브
        //--------------------------------

        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 앞쪽 등반 상태
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMBING_STATE;
    }

    // 등반 뒤쪽
    public void MoveBackClimbingState()
    {
        //--------------------------------
        // ■    < 큐브
        // ★    < 플레이어
        //--------------------------------

        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 뒤쪽 등반 상태
        m_playerState = en_PlayerState.MOVE_BACK_CLIMBING_STATE;
    }

    // 등반 왼쪽
    public void MoveLeftClimbingState()
    {
        //--------------------------------
        // ★■    < 큐브
        // ^ 플레이어
        //--------------------------------

        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 왼쪽 등반 상태
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMBING_STATE;
    }

    // 등반 오른쪽
    public void MoveRightClimbingState()
    {
        //--------------------------------
        // ■★    < 플레이어
        // ^ 큐브
        //--------------------------------

        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 오른쪽 등반 상태
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMBING_STATE;
    }


    // 등반 오르기 앞쪽
    public void ClimbingUpForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.up;
        // 플레이어 상태 앞쪽 위로 이동
        m_playerState = en_PlayerState.CLIMBING_UP_FORWARD;
    }

    // 등반 오르기 뒤쪽
    public void ClimbingUpBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.up;
        // 플레이어 상태 뒤쪽 위로 이동
        m_playerState = en_PlayerState.CLIMBING_UP_BACK;
    }

    // 등반 오르기 왼쪽
    public void ClimbingUpLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.up;
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.CLIMBING_UP_LEFT;
    }

    // 등반 오르기 오른쪽
    public void ClimbingUpRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.up;
        // 플레이어 상태 오른쪽 위로 이동
        m_playerState = en_PlayerState.CLIMBING_UP_RIGHT;
    }


    // 등반 이동 앞쪽
    public void ClimbingMoveForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 플레이어 상태 등반 앞쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_FORWARD;
    }

    // 등반 이동 뒤쪽
    public void ClimbingMoveBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 플레이어 상태 등반 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_BACK;
    }

    // 등반 이동 왼쪽
    public void ClimbingMoveLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 플레이어 상태 등반 왼쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_LEFT;
    }

    // 등반 이동 오른쪽
    public void ClimbingMoveRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 플레이어 상태 등반 앞쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_RIGHT;
    }


    public void ClimbingMoveForwardLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.left;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_FORWARD_LEFT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void ClimbingMoveForwardRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.right;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_FORWARD_RIGHT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void ClimbingMoveBackLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.left;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_BACK_LEFT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void ClimbingMoveBackRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.right;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_BACK_RIGHT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void ClimbingMoveLeftForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.forward;
        // 플레이어 상태 등반 왼쪽 앞쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_LEFT_FORWARD;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void ClimbingMoveLeftBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.back;
        // 플레이어 상태 등반 왼쪽 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_LEFT_BACK;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void ClimbingMoveRightForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.forward;
        // 플레이어 상태 등반 오른쪽 앞쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_RIGHT_FORWARD;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void ClimbingMoveRightBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.back;
        // 플레이어 상태 등반 오른쪽 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMBING_MOVE_RIGHT_BACK;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }


    // 방향 앞으로
    public void DirectionForward()
    {
        // 회전 방향
        m_destRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        // 플레이어 상태 회전
        m_playerState = en_PlayerState.TURN;
    }

    // 방향 뒤로
    public void DirectionBack()
    {
        // 회전 방향
        m_destRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        // 플레이어 상태 회전
        m_playerState = en_PlayerState.TURN;
    }

    // 방향 왼쪽
    public void DirectionLeft()
    {
        // 회전 방향
        m_destRotation = Quaternion.Euler(new Vector3(0, 270, 0));
        // 플레이어 상태 회전
        m_playerState = en_PlayerState.TURN;
    }

    // 방향 오른쪽
    public void DirectionRight()
    {
        // 회전 방향
        m_destRotation = Quaternion.Euler(new Vector3(0, 90, 0));
        // 플레이어 상태 회전
        m_playerState = en_PlayerState.TURN;
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
            Down,
            ForwardClimbingUp,
            BackClimbingUp,
            LeftClimbingUp,
            RightClimbingUp,
            ForwardClimbingDown,
            BackClimbingDown,
            LeftClimbingDown,
            RightClimbingDown,
            ForwardClimbingState,
            BackClimbingState,
            LeftClimbingState,
            RightClimbingState,
            ForwardClimbingUp,  // CLIMBING_UP_FORWARD
            BackClimbingUp,
            LeftClimbingUp,
            RightClimbingUp,
            Forward,            // CLIMBING_MOVE_FORWARD
            Back,
            Left,
            Right,
            ClimbingForwardLeft,
            ClimbingForwardRight,
            ClimbingBackLeft,
            ClimbingBackRight,
            ClimbingLeftForward,
            ClimbingLeftBack,
            ClimbingRightForward,
            ClimbingRightBack,
            Turn
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

    private void ForwardClimbingUp()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y > transform.position.y)
        {
            // 위로 이동함
            transform.position = transform.position + (Vector3.up * m_speed) * Time.deltaTime;
        }

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

    private void BackClimbingUp() 
    {
        // 뒤쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y > transform.position.y)
        {
            // 위로 이동함
            transform.position = transform.position + (Vector3.up * m_speed) * Time.deltaTime;
        }

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

    private void LeftClimbingUp()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y > transform.position.y)
        {
            // 위로 이동함
            transform.position = transform.position + (Vector3.up * m_speed) * Time.deltaTime;
        }

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

    private void RightClimbingUp()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y > transform.position.y)
        {
            // 위로 이동함
            transform.position = transform.position + (Vector3.up * m_speed) * Time.deltaTime;
        }

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


    private void ForwardClimbingDown()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void BackClimbingDown()
    {
        // 뒤쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void LeftClimbingDown()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void RightClimbingDown()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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


    private void ForwardClimbingState()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void BackClimbingState()
    {
        // 뒤쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void LeftClimbingState()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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

    private void RightClimbingState()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.y < transform.position.y)
        {
            // 아래로 이동함
            transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;
        }

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


    private void ClimbingForwardLeft()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.z > transform.position.z)
        {
            // 앞쪽 이동
            transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingForwardRight()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.z > transform.position.z)
        {
            // 앞쪽 이동
            transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingBackLeft()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.z < transform.position.z)
        {
            // 앞쪽 이동
            transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingBackRight()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.z < transform.position.z)
        {
            // 앞쪽 이동
            transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingLeftForward()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.x < transform.position.x)
        {
            // 왼쪽 이동
            transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingLeftBack()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.x < transform.position.x)
        {
            // 왼쪽 이동
            transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingRightForward()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.x > transform.position.x)
        {
            // 왼쪽 이동
            transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;
        }

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

    private void ClimbingRightBack()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 하지 못했나
        if (m_destPosition.x > transform.position.x)
        {
            // 왼쪽 이동
            transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;
        }

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


    private void Turn()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, m_destRotation, Time.deltaTime * m_speed * 2);

        if (Quaternion.Angle(transform.rotation, m_destRotation) <= m_speed)
        {
            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
        }
    }
}
