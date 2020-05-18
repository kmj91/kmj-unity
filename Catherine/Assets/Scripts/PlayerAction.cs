using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using GameGlobalScript;

public class PlayerAction : GameScript
{
    public float m_speed;                           // 캐릭터 이동 속도
    public int m_iX;                               // 인덱스 x
    public int m_iY;                               // 인덱스 y
    public int m_iZ;                               // 인덱스 z

    private Vector3 m_destPosition;                 // 이동 목표 좌표
    private Quaternion m_destRotation;              // 회전 목표 값
    private en_PlayerState m_playerState;           // 플레이어 상태
    private Action[] m_arrPlayerStateProc;          // 플레이어 상태 처리 함수 배열
    private GameManager m_gameManager;              // 게임 매니저
    private Animator m_animator;                    // 애니메이터
    private bool m_jump;                            // 점프 플래그

    // 초기화
    public void Init(GameManager gameManager, float speed, int iX, int iY, int iZ)
    {
        m_scriptType = en_ActionScriptType.PLAYER;
        m_gameManager = gameManager;
        m_speed = speed;
        m_iX = iX;
        m_iY = iY;
        m_iZ = iZ;
    }

    public override void MoveForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞으로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽으로 이동
        m_playerState = en_PlayerState.MOVE_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽으로 이동
        m_playerState = en_PlayerState.MOVE_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 오른쪽으로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void MoveForwardClimbUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + (Vector3.up * jumpPower);
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMB_UP;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveBackClimbUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_BACK_CLIMB_UP;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveLeftClimbUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMB_UP;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveRightClimbUp(int jumpPower = 1)
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMB_UP;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveForwardClimbDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 플레이어 상태 앞쪽 아래로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveBackClimbDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 플레이어 상태 뒤쪽 아래로 이동
        m_playerState = en_PlayerState.MOVE_BACK_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveLeftClimbDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    public void MoveRightClimbDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Jump");
    }

    // 등반 앞쪽
    public void MoveForwardClimbIdle()
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
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMB_IDLE;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 매달림
        m_animator.SetTrigger("Climb");
    }

    // 등반 뒤쪽
    public void MoveBackClimbIdle()
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
        m_playerState = en_PlayerState.MOVE_BACK_CLIMB_IDLE;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 매달림
        m_animator.SetTrigger("Climb");
    }

    // 등반 왼쪽
    public void MoveLeftClimbIdle()
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
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMB_IDLE;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 매달림
        m_animator.SetTrigger("Climb");
    }

    // 등반 오른쪽
    public void MoveRightClimbIdle()
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
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMB_IDLE;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 매달림
        m_animator.SetTrigger("Climb");
    }


    // 등반 오르기 앞쪽
    public void ClimbUpForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.up;
        // 플레이어 상태 앞쪽 위로 이동
        m_playerState = en_PlayerState.CLIMB_UP_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Climb End");
    }

    // 등반 오르기 뒤쪽
    public void ClimbUpBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.up;
        // 플레이어 상태 뒤쪽 위로 이동
        m_playerState = en_PlayerState.CLIMB_UP_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Climb End");
    }

    // 등반 오르기 왼쪽
    public void ClimbUpLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.up;
        // 플레이어 상태 왼쪽 위로 이동
        m_playerState = en_PlayerState.CLIMB_UP_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Climb End");
    }

    // 등반 오르기 오른쪽
    public void ClimbUpRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.up;
        // 플레이어 상태 오른쪽 위로 이동
        m_playerState = en_PlayerState.CLIMB_UP_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
        // 애니메이션 점프
        m_animator.SetTrigger("Climb End");
    }


    // 등반 이동 앞쪽
    public void ClimbMoveForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 플레이어 상태 등반 앞쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 등반 이동 뒤쪽
    public void ClimbMoveBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 플레이어 상태 등반 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 등반 이동 왼쪽
    public void ClimbMoveLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 플레이어 상태 등반 왼쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 등반 이동 오른쪽
    public void ClimbMoveRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 플레이어 상태 등반 앞쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }


    public void ClimbMoveForwardLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.left;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_FORWARD_LEFT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveForwardRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.right;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_FORWARD_RIGHT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveBackLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.left;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_BACK_LEFT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveBackRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.right;
        // 플레이어 상태 등반 앞쪽 왼쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_BACK_RIGHT;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveLeftForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.forward;
        // 플레이어 상태 등반 왼쪽 앞쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_LEFT_FORWARD;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveLeftBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.back;
        // 플레이어 상태 등반 왼쪽 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_LEFT_BACK;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveRightForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.forward;
        // 플레이어 상태 등반 오른쪽 앞쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_RIGHT_FORWARD;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void ClimbMoveRightBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.back;
        // 플레이어 상태 등반 오른쪽 뒤쪽 이동
        m_playerState = en_PlayerState.CLIMB_MOVE_RIGHT_BACK;
        // 회전 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public void PushForward()
    {
        // 플레이어 상태 앞쪽 밀기
        m_playerState = en_PlayerState.PUSH_FORWARD;
    }

    public void PushBack()
    {
        // 플레이어 상태 뒤쪽 밀기
        m_playerState = en_PlayerState.PUSH_BACK;
    }

    public void PushLeft()
    {
        // 플레이어 상태 왼쪽 밀기
        m_playerState = en_PlayerState.PUSH_LEFT;
    }

    public void PushRight()
    {
        // 플레이어 상태 오른쪽 밀기
        m_playerState = en_PlayerState.PUSH_RIGHT;
    }

    // 당기기 앞쪽
    public void PullForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 플레이어 상태 앞쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 뒤쪽
    public void PullBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 플레이어 상태 뒤쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 왼쪽
    public void PullLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 플레이어 상태 왼쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 오른쪽
    public void PullRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 플레이어 상태 오른쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 앞쪽 매달림
    public void PullForwardDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward + Vector3.down;
        // 플레이어 상태 앞쪽 이동 매달림
        m_playerState = en_PlayerState.MOVE_FORWARD_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 뒤쪽 매달림
    public void PullBackDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back + Vector3.down;
        // 플레이어 상태 뒤쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_BACK_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 왼쪽
    public void PullLeftDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left + Vector3.down;
        // 플레이어 상태 왼쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_LEFT_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    // 당기기 오른쪽
    public void PullRightDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right + Vector3.down;
        // 플레이어 상태 오른쪽 이동 당기기
        m_playerState = en_PlayerState.MOVE_RIGHT_CLIMB_DOWN;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
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

    public void Jump()
    {
        m_jump = true;
    }

    private void Awake()
    {
        // 상태 초기화
        m_playerState = en_PlayerState.STAY;

        m_arrPlayerStateProc = new Action[]
        {
            Stay,
            MoveForwardProc,
            MoveBackProc,
            MoveLeftProc,
            MoveRightProc,
            MoveUpProc,
            MoveDownProc,
            MoveForwardClimbUpProc,
            MoveBackClimbUpProc,
            MoveLeftClimbUpProc,
            MoveRightClimbUpProc,
            MoveForwardClimbDownProc,
            MoveBackClimbDownProc,
            MoveLeftClimbDownProc,
            MoveRightClimbDownProc,
            MoveForwardClimbIdleProc,
            MoveBackClimbIdleProc,
            MoveLeftClimbIdleProc,
            MoveRightClimbIdleProc,
            MoveForwardClimbUpProc,  // CLIMB_UP_FORWARD
            MoveBackClimbUpProc,
            MoveLeftClimbUpProc,
            MoveRightClimbUpProc,
            MoveForwardProc,            // CLIMB_MOVE_FORWARD
            MoveBackProc,
            MoveLeftProc,
            MoveRightProc,
            ClimbMoveForwardLeftProc,
            ClimbMoveForwardRightProc,
            ClimbMoveBackLeftProc,
            ClimbMoveBackRightProc,
            ClimbMoveLeftForwardProc,
            ClimbMoveLeftBackProc,
            ClimbMoveRightForwardProc,
            ClimbMoveRightBackProc,
            PushForwardProc,
            PushBackProc,
            PushLeftProc,
            PushRightProc,
            PullForwardProc,
            PullBackProc,
            PullLeftProc,
            PullRightProc,
            Turn
        };

        // 애니메이터 멤버 변수 초기화
        m_animator = GetComponent<Animator>();
        // 점프 플래그
        m_jump = false;
    }


    private void Update()
    {
        // 상태 처리
        m_arrPlayerStateProc[(int)m_playerState]();
    }


    private void Stay()
    {
        m_animator.SetFloat("Run", 0, 0.05f, Time.deltaTime);
    }

    private void MoveForwardProc()
    {
        // 앞쪽 이동
        transform.position = transform.position + (Vector3.forward * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.z <= transform.position.z)
        {
            // 위치 맞추기
            transform.position = m_destPosition;

            // 미끄러짐 체크
            if (m_gameManager.CheckSlideForward(m_iY, m_iZ + 1, m_iX))
            {
                SlideForward();
                return;
            }

            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            return;
        }

        // 애니메이션 값
        m_animator.SetFloat("Run", 1f, 0.05f, Time.deltaTime);
    }

    private void MoveBackProc()
    {
        // 뒤쪽 이동
        transform.position = transform.position + (Vector3.back * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.z >= transform.position.z)
        {
            // 위치 맞추기
            transform.position = m_destPosition;

            // 미끄러짐 체크
            if (m_gameManager.CheckSlideBack(m_iY, m_iZ - 1, m_iX))
            {
                SlideBack();
                return;
            }

            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            return;
        }

        // 애니메이션 값
        m_animator.SetFloat("Run", 1f, 0.05f, Time.deltaTime);
    }

    private void MoveLeftProc()
    {
        // 왼쪽 이동
        transform.position = transform.position + (Vector3.left * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.x >= transform.position.x)
        {
            // 위치 맞추기
            transform.position = m_destPosition;

            // 미끄러짐 체크
            if (m_gameManager.CheckSlideLeft(m_iY, m_iZ, m_iX - 1))
            {
                SlideLeft();
                return;
            }

            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX - 1);
            // 인덱스 이동
            m_iX = m_iX - 1;
            return;
        }

        // 애니메이션 값
        m_animator.SetFloat("Run", 1f, 0.05f, Time.deltaTime);
    }

    private void MoveRightProc()
    {
        // 오른쪽 이동
        transform.position = transform.position + (Vector3.right * m_speed) * Time.deltaTime;

        // 수평 이동 거리만큼 이동 했는가
        if (m_destPosition.x <= transform.position.x)
        {
            // 위치 맞추기
            transform.position = m_destPosition;

            // 미끄러짐 체크
            if (m_gameManager.CheckSlideRight(m_iY, m_iZ, m_iX + 1))
            {
                SlideRight();
                return;
            }

            // 플레이어 정지
            m_playerState = en_PlayerState.STAY;
            // 플레이어 조작 가능
            m_gameManager.canPlayerControl = true;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX + 1);
            // 인덱스 이동
            m_iX = m_iX + 1;
            return;
        }

        // 애니메이션 값
        m_animator.SetFloat("Run", 1f, 0.05f, Time.deltaTime);
    }

    private void MoveUpProc()
    {

    }

    private void MoveDownProc()
    {

    }

    // MOVE_FORWARD_CLIMB_UP
    private void MoveForwardClimbUpProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY + 1, m_iZ + 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY + 1;
            m_iZ = m_iZ + 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_BACK_CLIMB_UP
    private void MoveBackClimbUpProc() 
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY + 1, m_iZ - 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY + 1;
            m_iZ = m_iZ - 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_LEFT_CLIMB_UP
    private void MoveLeftClimbUpProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY + 1, m_iZ, m_iX - 1);
            // 인덱스 이동
            m_iY = m_iY + 1;
            m_iX = m_iX - 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_RIGHT_CLIMB_UP
    private void MoveRightClimbUpProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY + 1, m_iZ, m_iX + 1);
            // 인덱스 이동
            m_iY = m_iY + 1;
            m_iX = m_iX + 1;
            // 점프 끝
            m_jump = false;
        }
    }


    // MOVE_FORWARD_CLIMB_DOWN
    private void MoveForwardClimbDownProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ + 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iZ = m_iZ + 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_BACK_CLIMB_DOWN
    private void MoveBackClimbDownProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ - 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iZ = m_iZ - 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_LEFT_CLIMB_DOWN
    private void MoveLeftClimbDownProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ, m_iX - 1);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iX = m_iX - 1;
            // 점프 끝
            m_jump = false;
        }
    }

    // MOVE_RIGHT_CLIMB_DOWN
    private void MoveRightClimbDownProc()
    {
        if (!m_jump)
        {
            return;
        }

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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ, m_iX + 1);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iX = m_iX + 1;
            // 점프 끝
            m_jump = false;
        }
    }


    private void MoveForwardClimbIdleProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ + 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iZ = m_iZ + 1;
        }
    }

    private void MoveBackClimbIdleProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ - 1, m_iX);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iZ = m_iZ - 1;
        }
    }

    private void MoveLeftClimbIdleProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ, m_iX - 1);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iX = m_iX - 1;
        }
    }

    private void MoveRightClimbIdleProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ, m_iX + 1);
            // 인덱스 이동
            m_iY = m_iY - 1;
            m_iX = m_iX + 1;
        }
    }


    private void ClimbMoveForwardLeftProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX - 1);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            m_iX = m_iX - 1;
        }
    }

    private void ClimbMoveForwardRightProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX + 1);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            m_iX = m_iX + 1;
        }
    }

    private void ClimbMoveBackLeftProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX - 1);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            m_iX = m_iX - 1;
        }
    }

    private void ClimbMoveBackRightProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX + 1);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            m_iX = m_iX + 1;
        }
    }

    private void ClimbMoveLeftForwardProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX - 1);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            m_iX = m_iX - 1;
        }
    }

    private void ClimbMoveLeftBackProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX - 1);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            m_iX = m_iX - 1;
        }
    }

    private void ClimbMoveRightForwardProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX + 1);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            m_iX = m_iX + 1;
        }
    }

    private void ClimbMoveRightBackProc()
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
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX + 1);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            m_iX = m_iX + 1;
        }
    }

    private void PushForwardProc()
    {
    
    }

    private void PushBackProc()
    {

    }

    private void PushLeftProc()
    {

    }

    private void PushRightProc()
    {

    }

    private void PullForwardProc()
    {

    }

    private void PullBackProc()
    {

    }

    private void PullLeftProc()
    {

    }

    private void PullRightProc()
    {

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


    private void SlideForward()
    {
        // 데이터 붙여넣기
        m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX);
        // 인덱스 이동
        m_iZ = m_iZ + 1;
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 플레이어 상태 앞쪽으로 이동
        m_playerState = en_PlayerState.MOVE_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    private void SlideBack()
    {
        // 데이터 붙여넣기
        m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX);
        // 인덱스 이동
        m_iZ = m_iZ - 1;
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 플레이어 상태 뒤쪽으로 이동
        m_playerState = en_PlayerState.MOVE_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    private void SlideLeft()
    {
        // 데이터 붙여넣기
        m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX - 1);
        // 인덱스 이동
        m_iX = m_iX - 1;
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 플레이어 상태 왼쪽으로 이동
        m_playerState = en_PlayerState.MOVE_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    private void SlideRight()
    {
        // 데이터 붙여넣기
        m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX + 1);
        // 인덱스 이동
        m_iX = m_iX + 1;
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 플레이어 상태 오른쪽으로 이동
        m_playerState = en_PlayerState.MOVE_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }
}
