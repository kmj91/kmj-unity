using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Random = UnityEngine.Random;

using GameGlobalScript;

public class CubeAction : GameScript
{
    public float m_speed;                           // 큐브 이동 속도

    private int m_iX;                               // 인덱스 x
    private int m_iY;                               // 인덱스 y
    private int m_iZ;                               // 인덱스 z
    private float m_fShakeAmount;                   // 흔들리는 힘
    private float m_fShakeTime;                     // 흔들리는 시간
    private float m_fShakeTick;                     // 0이되면 흔들림을 멈추는 틱
    private Vector3 m_backupPosition;               // 흔들리기 전 포지션 값
    private Vector3 m_destPosition;                 // 이동 목표 좌표
    private en_CubeState m_cubeState;               // 큐브 상태
    private Action[] m_arrCubeStateProc;            // 큐브 상태 처리 함수 배열
    private GameManager m_gameManager;              // 게임 매니저

    // 초기화
    public void Init(GameManager gameManager, float speed, int iX, int iY, int iZ, float fShakeAmout, float fShakeTime)
    {
        m_gameManager = gameManager;
        m_speed = speed;
        m_iX = iX;
        m_iY = iY;
        m_iZ = iZ;
        m_fShakeAmount = fShakeAmout;
        m_fShakeTime = fShakeTime;
    }

    public override void MoveForward()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.forward;
        // 큐브 상태 앞으로 이동
        m_cubeState = en_CubeState.MOVE_FORWARD;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveBack()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.back;
        // 큐브 상태 뒤쪽으로 이동
        m_cubeState = en_CubeState.MOVE_BACK;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveLeft()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.left;
        // 큐브 상태 왼쪽으로 이동
        m_cubeState = en_CubeState.MOVE_LEFT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveRight()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.right;
        // 큐브 상태 오른쪽으로 이동
        m_cubeState = en_CubeState.MOVE_RIGHT;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveUp()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.up;
        // 큐브 상태 위쪽으로 이동
        m_cubeState = en_CubeState.MOVE_UP;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);
    }

    public override void MoveDown()
    {
        // 이동 좌표
        m_destPosition = transform.position + Vector3.down;
        // 큐브 상태 아래쪽으로 이동
        m_cubeState = en_CubeState.SHAKE;
        // 데이터 잘라내기
        m_gameManager.CutData(m_iY, m_iZ, m_iX);

        m_backupPosition = transform.position;
        m_fShakeTick = m_fShakeTime;
    }


    private void Awake()
    {
        // 상태 초기화
        m_cubeState = en_CubeState.STAY;

        m_arrCubeStateProc = new Action[] 
        {
            Stay,
            Forward,
            Back,
            Left,
            Right,
            Up,
            Down,
            Shake
        };
    }


    private void Update()
    {
        // 상태 처리
        m_arrCubeStateProc[(int)m_cubeState]();
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
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ + 1, m_iX);
            // 위에 큐브들 떨어지는지
            m_gameManager.CheckCeiling(m_iY, m_iZ, m_iX);
            // 인덱스 이동
            m_iZ = m_iZ + 1;
            // 큐브가 이동 한 후 아래에 큐브가 있는지
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                m_gameManager.MoveDownGameObject(m_iY, m_iZ, m_iX);
            }
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
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ - 1, m_iX);
            // 위에 큐브들 떨어지는지
            m_gameManager.CheckCeiling(m_iY, m_iZ, m_iX);
            // 인덱스 이동
            m_iZ = m_iZ - 1;
            // 큐브가 이동 한 후 아래에 큐브가 있는지
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                m_gameManager.MoveDownGameObject(m_iY, m_iZ, m_iX);
            }
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
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX - 1);
            // 위에 큐브들 떨어지는지
            m_gameManager.CheckCeiling(m_iY, m_iZ, m_iX);
            // 인덱스 이동
            m_iX = m_iX - 1;
            // 큐브가 이동 한 후 아래에 큐브가 있는지
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                m_gameManager.MoveDownGameObject(m_iY, m_iZ, m_iX);
            }
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
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX + 1);
            // 위에 큐브들 떨어지는지
            m_gameManager.CheckCeiling(m_iY, m_iZ, m_iX);
            // 인덱스 이동
            m_iX = m_iX + 1;
            // 큐브가 이동 한 후 아래에 큐브가 있는지
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                m_gameManager.MoveDownGameObject(m_iY, m_iZ, m_iX);
            }
        }
    }

    private void Up()
    {
        // 위쪽 이동
        transform.position = transform.position + (Vector3.up * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 했는가
        if (m_destPosition.y <= transform.position.y)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
        }
    }

    private void Down()
    {
        // 아래쪽 이동
        transform.position = transform.position + (Vector3.down * m_speed) * Time.deltaTime;

        // 수직 이동 거리만큼 이동 했는가
        if (m_destPosition.y >= transform.position.y)
        {
            // 위치 맞추기
            transform.position = m_destPosition;
            // 큐브 정지
            m_cubeState = en_CubeState.STAY;
            // 데이터 붙여넣기
            m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY - 1, m_iZ, m_iX);
            // 위에 큐브들 떨어지는지
            m_gameManager.CheckCeiling(m_iY, m_iZ, m_iX);
            // 인덱스 이동
            m_iY = m_iY - 1;
            // 큐브가 이동 한 후 아래에 큐브가 있는지
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                // 이동 좌표
                m_destPosition = transform.position + Vector3.down;
                // 데이터 잘라내기
                m_gameManager.CutData(m_iY, m_iZ, m_iX);
                // 떨어짐
                m_cubeState = en_CubeState.MOVE_DOWN;
            }
        }
    }

    private void Shake()
    {
        if (m_fShakeTick > 0)
        {
            // 흔들기
            transform.position = Random.insideUnitSphere * m_fShakeAmount + m_backupPosition;
            m_fShakeTick = m_fShakeTick - Time.deltaTime;
        }
        else
        {
            // 흔들기로 흐트러진 포지션 맞추기
            transform.position = m_backupPosition;
            // 다시 아래쪽 검사
            if (m_gameManager.CheckFloor(m_iY, m_iZ, m_iX))
            {
                // 떨어짐
                m_cubeState = en_CubeState.MOVE_DOWN;
            }
            else
            {
                // 떨어지지 않음
                // 큐브 정지
                m_cubeState = en_CubeState.STAY;
                // 데이터 붙여넣기
                m_gameManager.PasteData(m_iY, m_iZ, m_iX, m_iY, m_iZ, m_iX);
            }
        }
    }
}
