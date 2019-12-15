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
        RIGHT,                      // 오른쪽
        LEFT,                       // 왼쪽
        FORWARD,                    // 앞쪽
        BACK                        // 뒤쪽
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

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }


        //switch (transform.tag)
        //{
        //    case "NormalCube":
        //        // 일반 큐브

        //        break;
        //    default:
        //        break;
        //}

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check = transform.position;
        check.x = check.x + 1;

        // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
        // 오른쪽 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, 1 << layerMaskCube))
        {
            // 큐브 이동 처리
            // 이동 불가?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
            {
                return false;
            }
        }

        // 이동 좌표
        destPos = check;
        // 오른쪽 이동
        cubeMoveState = CubeMoveState.RIGHT;

        return true;
    }

    // 큐브 왼쪽 이동
    public bool MoveLeft()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check = transform.position;
        check.x = check.x - 1;

        // 만약 왼쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
        // 왼쪽 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.left, out rayHit, 1f, 1 << layerMaskCube))
        {
            // 큐브 이동 처리
            // 이동 불가?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveLeft())
            {
                return false;
            }
        }

        // 이동 좌표
        destPos = check;
        // 왼쪽 이동
        cubeMoveState = CubeMoveState.LEFT;

        return true;
    }

    // 큐브 앞쪽 이동
    public bool MoveForward()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check = transform.position;
        check.x = check.z + 1;

        // 만약 앞쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
        // 앞쪽 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.forward, out rayHit, 1f, 1 << layerMaskCube))
        {
            // 큐브 이동 처리
            // 이동 불가?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveForward())
            {
                return false;
            }
        }

        // 이동 좌표
        destPos = check;
        // 앞쪽 이동
        cubeMoveState = CubeMoveState.FORWARD;

        return true;
    }

    // 큐브 뒤쪽 이동
    public bool MoveBack()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check = transform.position;
        check.x = check.z - 1;

        // 만약 뒤쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
        // 뒤쪽 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.back, out rayHit, 1f, 1 << layerMaskCube))
        {
            // 큐브 이동 처리
            // 이동 불가?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveBack())
            {
                return false;
            }
        }

        // 이동 좌표
        destPos = check;
        // 뒤쪽 이동
        cubeMoveState = CubeMoveState.BACK;

        return true;
    }

    public void Gravity()
    {
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        // 큐브의 밑 십자가 5방향 검사
        //   □
        // □？□
        //   □  
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

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= transform.position.x) {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 큐브 정지
                    cubeMoveState = CubeMoveState.IDLE;
                }
                break;
        }
    }
}
