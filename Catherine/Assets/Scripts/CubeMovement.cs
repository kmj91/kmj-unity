﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    //--------------------------------
    // public 변수
    //--------------------------------

    // 상태
    public CubeMoveState cubeMoveState { get; private set; }
    // 딜레이
    public float actionDelay { get; private set; }
    // 큐브 이동 속도
    public float horizontalSpeed;
    // 큐브 수직 이동 속도
    public float verticalSpeed;
    // 큐브 이동 목표 좌표
    public Vector3 destPos;
    // 미끄러짐
    public bool slideEvent;
    // 떨어짐
    public bool isMoveDown;

    //--------------------------------
    // private 변수
    //--------------------------------

    // 레이어 마스크 큐브
    private LayerMask layerMaskCube;
    // 중력 영향을 받는가
    private bool isGravity;

    //--------------------------------
    // enum
    //--------------------------------

    public enum CubeMoveState
    {
        IDLE,                       // 대기
        DOWN_READY,                 // 아래 준비
        DOWN,                       // 아래
        RIGHT,                      // 오른쪽
        LEFT,                       // 왼쪽
        FORWARD,                    // 앞쪽
        BACK                        // 뒤쪽
    }

    //--------------------------------
    // 상수
    //--------------------------------

    // 큐브가 아래로 이동할 때의 딜레이
    public const float DOWN_DELAY = 1.5f;

    //--------------------------------
    // private 함수
    //--------------------------------

    private void Start()
    {
        // 상태
        cubeMoveState = CubeMoveState.IDLE;
        // 레이어 마스크 큐브
        layerMaskCube = 1 << LayerMask.NameToLayer("Cube");
        // 딜레이
        actionDelay = 0f;
        // 중력
        isGravity = true;
    }

    
    private void FixedUpdate()
    {
        MoveProcess();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Floor"))
        {
            return;
        }

        if (cubeMoveState == CubeMoveState.DOWN)
        {
            return;
        }

        isGravity = false;
        cubeMoveState = CubeMoveState.IDLE;
    }

    private void OnTriggerExit(Collider other)
    {
        isGravity = true;
    }

    //--------------------------------
    // public 함수
    //--------------------------------

    public float GetDelayTime()
    {
        if (actionDelay > DOWN_DELAY)
        {
            return 0;
        }
        else
        {
            return DOWN_DELAY - actionDelay;
        }
    }

    // 큐브 오른쪽 이동
    public bool MoveRight()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        //-----------------------------
        // 큐브 타입 검사
        //-----------------------------
        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }
        // 얼음 큐브인가?
        else if (gameObject.CompareTag("IceCube"))
        {
            // 미끄러지는 중인가?
            if (slideEvent)
            {
                // 밀려나서 계속 미끄러지는 중인 경우

                // 바닥 검사
                // 없다
                if (!Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }

                //-------------------------------------------
                // 만약 오른쪽 이동 방향에 큐브가 있다면
                // 같이 미끄러지는 큐브의 경우 이상 없음
                // 정지한 큐브라면 부딪혀서 정지
                //-------------------------------------------
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // slideEvent 체크
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                    {
                        // 정지한 큐브임
                        // 미끄러지지않음
                        slideEvent = false;
                        cubeMoveState = CubeMoveState.IDLE;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.x = destPos.x + 1f;
            }
            else
            {
                // 처음 미는 중인 경우

                // 바닥 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러짐
                    slideEvent = true;
                }

                // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // 큐브 이동 처리
                    // 이동 불가?
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
                    {
                        // 이동하지 않음
                        slideEvent = false;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.x = destPos.x + 1f;
                // 오른쪽 이동
                cubeMoveState = CubeMoveState.RIGHT;
            }

            return true;
        }


        // 미끄러지는 중인가?
        if (slideEvent)
        {
            // 밀려나서 계속 미끄러지는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브가 아님
                if (!rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }
            else
            {
                // 미끄러지지않음
                slideEvent = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            //-------------------------------------------
            // 만약 오른쪽 이동 방향에 큐브가 있다면
            // 같이 미끄러지는 큐브의 경우 이상 없음
            // 정지한 큐브라면 부딪혀서 정지
            //-------------------------------------------
            // 오른쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
            {
                // slideEvent 체크
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                {
                    // 정지한 큐브임
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.x = destPos.x + 1f;
        }
        else
        {
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브임
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러짐
                    slideEvent = true;
                }
            }

            // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 오른쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
                {
                    // 이동하지 않음
                    slideEvent = false;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.x = destPos.x + 1f;
            // 오른쪽 이동
            cubeMoveState = CubeMoveState.RIGHT;
        }

        return true;
    }

    // 큐브 왼쪽 이동
    public bool MoveLeft()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }
        // 얼음 큐브인가?
        else if (gameObject.CompareTag("IceCube"))
        {
            // 미끄러지는 중인가?
            if (slideEvent)
            {
                // 밀려나서 계속 미끄러지는 중인 경우

                // 바닥 검사
                // 없다
                if (!Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }

                //-------------------------------------------
                // 만약 오른쪽 이동 방향에 큐브가 있다면
                // 같이 미끄러지는 큐브의 경우 이상 없음
                // 정지한 큐브라면 부딪혀서 정지
                //-------------------------------------------
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // slideEvent 체크
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                    {
                        // 정지한 큐브임
                        // 미끄러지지않음
                        slideEvent = false;
                        cubeMoveState = CubeMoveState.IDLE;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.x = destPos.x - 1f;
            }
            else
            {
                // 처음 미는 중인 경우

                // 바닥 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러짐
                    slideEvent = true;
                }

                // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // 큐브 이동 처리
                    // 이동 불가?
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
                    {
                        // 이동하지 않음
                        slideEvent = false;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.x = destPos.x - 1f;
                // 오른쪽 이동
                cubeMoveState = CubeMoveState.LEFT;
            }

            return true;
        }


        // 미끄러지는 중인가?
        if (slideEvent)
        {
            // 밀려나서 계속 미끄러지는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브가 아님
                if (!rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }
            else
            {
                // 미끄러지지않음
                slideEvent = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            //-------------------------------------------
            // 만약 왼쪽 이동 방향에 큐브가 있다면
            // 같이 미끄러지는 큐브의 경우 이상 없음
            // 정지한 큐브라면 부딪혀서 정지
            //-------------------------------------------
            // 왼쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.left, out rayHit, 1f, layerMaskCube))
            {
                // slideEvent 체크
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                {
                    // 정지한 큐브임
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.x = destPos.x - 1f;
        }
        else
        {
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브임
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러짐
                    slideEvent = true;
                }
            }

            // 만약 왼쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 왼쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.left, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveLeft())
                {
                    // 이동하지 않음
                    slideEvent = false;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.x = destPos.x - 1f;
            // 왼쪽 이동
            cubeMoveState = CubeMoveState.LEFT;
        }

        return true;
    }

    // 큐브 앞쪽 이동
    public bool MoveForward()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }
        // 얼음 큐브인가?
        else if (gameObject.CompareTag("IceCube"))
        {
            // 미끄러지는 중인가?
            if (slideEvent)
            {
                // 밀려나서 계속 미끄러지는 중인 경우

                // 바닥 검사
                // 없다
                if (!Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }

                //-------------------------------------------
                // 만약 오른쪽 이동 방향에 큐브가 있다면
                // 같이 미끄러지는 큐브의 경우 이상 없음
                // 정지한 큐브라면 부딪혀서 정지
                //-------------------------------------------
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // slideEvent 체크
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                    {
                        // 정지한 큐브임
                        // 미끄러지지않음
                        slideEvent = false;
                        cubeMoveState = CubeMoveState.IDLE;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.z = destPos.z + 1f;
            }
            else
            {
                // 처음 미는 중인 경우

                // 바닥 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러짐
                    slideEvent = true;
                }

                // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // 큐브 이동 처리
                    // 이동 불가?
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
                    {
                        // 이동하지 않음
                        slideEvent = false;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.z = destPos.z + 1f;
                // 오른쪽 이동
                cubeMoveState = CubeMoveState.FORWARD;
            }

            return true;
        }


        // 미끄러지는 중인가?
        if (slideEvent)
        {
            // 밀려나서 계속 미끄러지는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브가 아님
                if (!rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }
            else
            {
                // 미끄러지지않음
                slideEvent = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            //-------------------------------------------
            // 만약 앞쪽 이동 방향에 큐브가 있다면
            // 같이 미끄러지는 큐브의 경우 이상 없음
            // 정지한 큐브라면 부딪혀서 정지
            //-------------------------------------------
            // 앞쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.forward, out rayHit, 1f, layerMaskCube))
            {
                // slideEvent 체크
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                {
                    // 정지한 큐브임
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.z = destPos.z + 1f;
        }
        else
        {
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브임
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러짐
                    slideEvent = true;
                }
            }

            // 만약 앞쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 앞쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.forward, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveForward())
                {
                    // 이동하지 않음
                    slideEvent = false;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.z = destPos.z + 1f;
            // 앞쪽 이동
            cubeMoveState = CubeMoveState.FORWARD;
        }

        return true;
    }

    // 큐브 뒤쪽 이동
    public bool MoveBack()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 움직이지 않는 큐브인가?
        if (gameObject.CompareTag("StaticCube"))
        {
            // 해당 타입의 큐브는 움직일 수 없다
            return false;
        }
        // 얼음 큐브인가?
        else if (gameObject.CompareTag("IceCube"))
        {
            // 미끄러지는 중인가?
            if (slideEvent)
            {
                // 밀려나서 계속 미끄러지는 중인 경우

                // 바닥 검사
                // 없다
                if (!Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }

                //-------------------------------------------
                // 만약 오른쪽 이동 방향에 큐브가 있다면
                // 같이 미끄러지는 큐브의 경우 이상 없음
                // 정지한 큐브라면 부딪혀서 정지
                //-------------------------------------------
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // slideEvent 체크
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                    {
                        // 정지한 큐브임
                        // 미끄러지지않음
                        slideEvent = false;
                        cubeMoveState = CubeMoveState.IDLE;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.z = destPos.z - 1f;
            }
            else
            {
                // 처음 미는 중인 경우

                // 바닥 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러짐
                    slideEvent = true;
                }

                // 만약 오른쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
                // 오른쪽 검사
                // 있다
                if (Physics.Raycast(transform.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                {
                    // 큐브 이동 처리
                    // 이동 불가?
                    if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight())
                    {
                        // 이동하지 않음
                        slideEvent = false;
                        return false;
                    }
                }

                // 이동 좌표
                destPos = transform.position;
                destPos.z = destPos.z - 1f;
                // 오른쪽 이동
                cubeMoveState = CubeMoveState.BACK;
            }

            return true;
        }


        // 미끄러지는 중인가?
        if (slideEvent)
        {
            // 밀려나서 계속 미끄러지는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브가 아님
                if (!rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }
            else
            {
                // 미끄러지지않음
                slideEvent = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            //-------------------------------------------
            // 만약 뒤쪽 이동 방향에 큐브가 있다면
            // 같이 미끄러지는 큐브의 경우 이상 없음
            // 정지한 큐브라면 부딪혀서 정지
            //-------------------------------------------
            // 앞쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.back, out rayHit, 1f, layerMaskCube))
            {
                // slideEvent 체크
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().slideEvent)
                {
                    // 정지한 큐브임
                    // 미끄러지지않음
                    slideEvent = false;
                    cubeMoveState = CubeMoveState.IDLE;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.z = destPos.z - 1f;
        }
        else
        {
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 바닥이 아이스 큐브임
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 미끄러짐
                    slideEvent = true;
                }
            }

            // 만약 뒤쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 뒤쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.back, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveBack())
                {
                    // 이동하지 않음
                    slideEvent = false;
                    return false;
                }
            }

            // 이동 좌표
            destPos = transform.position;
            destPos.z = destPos.z - 1f;
            // 뒤쪽 이동
            cubeMoveState = CubeMoveState.BACK;
        }

        return true;
    }

    public bool GravityCheck()
    {
        Vector3 ray;            // 레이 시작점
        Vector3 rayDir;         // 레이 방향
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 중력 영향을 받지않으면
        if (!isGravity)
        {
            return false;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check.x = transform.position.x;
        check.y = transform.position.y - 1f;
        check.z = transform.position.z;

        // 초기화
        actionDelay = 0f;

        // 이동 좌표
        destPos = check;

        // 큐브의 밑 십자가 5방향 검사
        //   □
        // □？□
        //   □  
        // 바로 밑
        ray = transform.position;
        rayDir = Vector3.down;
        // 밑에 고정된 발판이 있나?
        if (Physics.Raycast(ray, rayDir, out rayHit, 1f, layerMaskCube))
        {
            // 있다

            // 큐브가 아래로 이동하는 중인지 아닌지?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().isMoveDown)
            {
                // 이동하지 않음
                isMoveDown = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            // 큐브 아래로
            isMoveDown = true;
            cubeMoveState = CubeMoveState.DOWN;
            return true;
        }

        //   □
        // ？□□
        //   □  
        // 왼쪽 밑
        ray = check;
        rayDir = Vector3.left;
        // 왼쪽 밑에 큐브가 있나?
        if (Physics.Raycast(ray, rayDir, out rayHit, 1f, layerMaskCube))
        {
            // 있음

            // 큐브가 아래로 이동하는 중인지 아닌지?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().isMoveDown)
            {
                // 이동하지 않음
                isMoveDown = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            // 큐브 아래로
            isMoveDown = true;
            cubeMoveState = CubeMoveState.DOWN;
            return true;
        }

        //   □
        // □□？
        //   □  
        // 오른쪽 밑
        ray = check;
        rayDir = Vector3.right;
        // 오른쪽 밑에 큐브가 있나?
        if (Physics.Raycast(ray, rayDir, out rayHit, 1f, layerMaskCube))
        {
            // 있음

            // 큐브가 아래로 이동하는 중인지 아닌지?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().isMoveDown)
            {
                // 이동하지 않음
                isMoveDown = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            // 큐브 아래로
            isMoveDown = true;
            cubeMoveState = CubeMoveState.DOWN;
            return true;
        }

        //   ？
        // □□□
        //   □  
        // 위쪽 밑
        ray = check;
        rayDir = Vector3.forward;
        // 앞쪽 밑에 큐브가 있나?
        if (Physics.Raycast(ray, rayDir, out rayHit, 1f, layerMaskCube))
        {
            // 있음

            // 큐브가 아래로 이동하는 중인지 아닌지?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().isMoveDown)
            {
                // 이동하지 않음
                isMoveDown = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            // 큐브 아래로
            isMoveDown = true;
            cubeMoveState = CubeMoveState.DOWN;
            return true;
        }

        //   □
        // □□□
        //   ？  
        // 아래쪽 밑
        ray = check;
        rayDir = Vector3.back;
        // 뒤쪽 밑에 큐브가 있나?
        if (Physics.Raycast(ray, rayDir, out rayHit, 1f, layerMaskCube))
        {
            // 있음

            // 큐브가 아래로 이동하는 중인지 아닌지?
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().isMoveDown)
            {
                // 이동하지 않음
                isMoveDown = false;
                cubeMoveState = CubeMoveState.IDLE;
                return false;
            }

            // 큐브 아래로
            isMoveDown = true;
            cubeMoveState = CubeMoveState.DOWN;
            return true;
        }

        // 처음 떨어지고 있는 중임
        if (!isMoveDown)
        {
            cubeMoveState = CubeMoveState.DOWN_READY;
        }
        
        return true;
    }

    private void MoveProcess()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        switch (cubeMoveState) {
            case CubeMoveState.DOWN_READY:
                // 큐브 떨어짐 준비

                actionDelay = actionDelay + Time.deltaTime;

                // 대기
                if (actionDelay < DOWN_DELAY)
                {
                    break;
                }
                isMoveDown = true;
                cubeMoveState = CubeMoveState.DOWN;

                break;
            case CubeMoveState.DOWN:
                // 큐브 떨어짐
                transform.position = transform.position + (Vector3.down * verticalSpeed) * Time.deltaTime;

                // 수직 이동 거리만큼 이동 했는가
                if (destPos.y >= transform.position.y)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 떨어지는 상황인지 확인
                    GravityCheck();
                }
                break;
            case CubeMoveState.RIGHT:
                // 오른쪽 이동
                transform.position = transform.position + (Vector3.right * horizontalSpeed) * Time.deltaTime;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= transform.position.x) {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        MoveRight();
                    }
                    else
                    {
                        // 미끄러지는 중이 아니지만 밀려난 곳의 발판이 빙판이면 미끄러져야함
                        // 바닥 검사
                        // 있다
                        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 바닥이 아이스 큐브
                            if (rayHit.transform.gameObject.CompareTag("IceCube"))
                            {
                                // 미끄러짐
                                slideEvent = true;
                                MoveRight();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            case CubeMoveState.LEFT:
                // 왼쪽 이동
                transform.position = transform.position + (Vector3.left * horizontalSpeed) * Time.deltaTime;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= transform.position.x)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        MoveLeft();
                    }
                    else
                    {
                        // 미끄러지는 중이 아니지만 밀려난 곳의 발판이 빙판이면 미끄러져야함
                        // 바닥 검사
                        // 있다
                        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 바닥이 아이스 큐브
                            if (rayHit.transform.gameObject.CompareTag("IceCube"))
                            {
                                // 미끄러짐
                                slideEvent = true;
                                MoveLeft();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            case CubeMoveState.FORWARD:
                // 앞쪽 이동
                transform.position = transform.position + (Vector3.forward * horizontalSpeed) * Time.deltaTime;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= transform.position.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        MoveForward();
                    }
                    else
                    {
                        // 미끄러지는 중이 아니지만 밀려난 곳의 발판이 빙판이면 미끄러져야함
                        // 바닥 검사
                        // 있다
                        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 바닥이 아이스 큐브
                            if (rayHit.transform.gameObject.CompareTag("IceCube"))
                            {
                                // 미끄러짐
                                slideEvent = true;
                                MoveForward();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            case CubeMoveState.BACK:
                // 앞쪽 이동
                transform.position = transform.position + (Vector3.back * horizontalSpeed) * Time.deltaTime;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= transform.position.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        MoveBack();
                    }
                    else
                    {
                        // 미끄러지는 중이 아니지만 밀려난 곳의 발판이 빙판이면 미끄러져야함
                        // 바닥 검사
                        // 있다
                        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 바닥이 아이스 큐브
                            if (rayHit.transform.gameObject.CompareTag("IceCube"))
                            {
                                // 미끄러짐
                                slideEvent = true;
                                MoveBack();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            default:
                // 떨어지는 상황인지 확인
                GravityCheck();
                break;
        }
    }
}
