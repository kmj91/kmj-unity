using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    private LayerMask layerMaskCube;        // 레이어 마스크

    // 큐브 이동 목표 좌표
    public Vector3 destPos;
    // 큐브 이동 속도
    public float speed = 2f;

    // 상태
    private CubeMoveState cubeMoveState = CubeMoveState.IDLE;

    private enum CubeMoveState
    {
        IDLE,                       // 대기
        DOWN                        // 추락
    }


    void Start()
    {
        // 레이어 마스크
        layerMaskCube = LayerMask.NameToLayer("Cube");
    }

    
    void FixedUpdate()
    {
        MoveProcess();
        Gravity();
    }


    public void Gravity()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        // 큐브의 밑 8방향 검사
        // □□□
        // □？□
        // □□□
        // 바로 밑
        check.x = transform.position.x;
        check.y = transform.position.y - 1f;
        check.z = transform.position.z;
        // 없다
        if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
        {
            cubeMoveState = CubeMoveState.DOWN;
        }
    }

    private void MoveProcess()
    {
        switch (cubeMoveState) {
            case CubeMoveState.DOWN:
                // 큐브 떨어짐
                transform.position = transform.position + (Vector3.down * speed) * Time.deltaTime;
                break;
        }
    }
}
