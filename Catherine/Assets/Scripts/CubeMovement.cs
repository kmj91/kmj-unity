using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    private LayerMask layerMaskCube;        // 레이어 마스크

    // 큐브 이동 목표 좌표
    public Vector3 destPos;
    // 큐브 이동 속도
    public float speed = 4f;

    // 상태
    private CubeMoveState cubeMoveState = CubeMoveState.IDLE;

    private enum CubeMoveState
    {
        IDLE,                       // 대기
        DOWN,                       // 아래
        RIGHT                       // 오른쪽
    }


    void Start()
    {
        // 레이어 마스크
        layerMaskCube = LayerMask.NameToLayer("Cube");
    }

    
    void FixedUpdate()
    {
        MoveProcess();
    }

    // 큐브 오른쪽 이동
    public bool MoveRight()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        RaycastHit rayHit;      // 레이 충돌한 물체

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check = transform.position;
        check.x = check.x + 1;

        // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
        // 오른쪽 검사
        // 있다
        if (Physics.Raycast(transform.position, transform.right, out rayHit, 1f, 1 << layerMaskCube))
        {
            // 큐브 이동 처리
            // 이동 가능
            if (rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
            {

            }
            // 이동 불가
            else
            {
                return false;
            }
        }
        // 없다
        else {
            cubeMoveState = CubeMoveState.RIGHT;
        }

        return true;
    }

    // 큐브 왼쪽 이동
    public bool MoveLeft()
    {
        return true;
    }

    // 큐브 앞쪽 이동
    public bool MoveForward()
    {
        return true;
    }

    // 큐브 뒤쪽 이동
    public bool MoveBack()
    {
        return true;
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
            case CubeMoveState.RIGHT:
                // 오른쪽 이동
                transform.position = transform.position + (Vector3.right * speed) * Time.deltaTime;
                break;
        }
    }
}
