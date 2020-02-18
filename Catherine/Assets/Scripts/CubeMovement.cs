using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameMessageScript;

public class CubeMovement : MonoBehaviour
{
    //--------------------------------
    // public 변수
    //--------------------------------

    // 상태
    public CubeMoveState cubeMoveState { get; private set; }
    // 딜레이
    public float actionDelay { get; private set; }
    // 연결된 큐브 딜레이
    public float chainActionDelay { get; private set; }
    // 큐브 이동 속도
    public float horizontalSpeed;
    // 큐브 수직 이동 속도
    public float verticalSpeed;
    // 큐브가 아래로 이동할 때의 딜레이
    public float downDelay;
    // 교체할 중력 값
    public ParticleSystem.MinMaxCurve iceEffectGravity;
    // 교체할 크기
    public ParticleSystem.MinMaxCurve iceEffectSize;
    // 큐브 이동 목표 좌표
    public Vector3 destPos;
    // 미끄러짐
    public bool slideEvent;
    // 떨어짐
    //public bool isMoveDown; 
    // 스파크 이펙트
    public ParticleSystem RF_SparksEffect;
    public ParticleSystem RB_SparksEffect;
    public ParticleSystem LF_SparksEffect;
    public ParticleSystem LB_SparksEffect;

    public Material mate;

    //--------------------------------
    // private 변수
    //--------------------------------

    // 틱 토큰
    private int tickToken;
    // 큐브 흔들림 틱
    private float downTick;
    // 당겨질 때의 속도
    private float pullSpeed;
    // 레이어 마스크 큐브
    private LayerMask layerMaskCube;
    // 중력 영향을 받는가
    private bool isGravity;
    // 교체될 컬러
    private Color IceColor;
    // 재질 블럭
    private MaterialPropertyBlock materialBlock;
    // 원본 중력 값
    private ParticleSystem.MinMaxCurve originalEffectGravityModifier;
    // 원본 크기
    private ParticleSystem.MinMaxCurve originalEffectStartSize;


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
        BACK,                       // 뒤쪽
        PULL_RIGHT,                 // 오른쪽 당겨짐
        PULL_LEFT,                  // 왼쪽 당겨짐
        PULL_FORWARD,               // 앞쪽 당겨짐
        PULL_BACK,                  // 뒤쪽 당겨짐
        SHAKE                       // 큐브 흔들기
    }


    //--------------------------------
    // 상수
    //--------------------------------

    private const float TICK_DELAY = 0.08f;


    //--------------------------------
    // public 함수
    //--------------------------------

    // 큐브 상태를 대기 상태로 변경
    public void UpdateStateToIdle()
    {
        cubeMoveState = CubeMoveState.IDLE;
    }


    // 아래로 떨어지기까지 남은 시간
    public float GetDownDelayTime()
    {
        if (actionDelay > downDelay)
        {
            return 0;
        }
        else
        {
            return downDelay - actionDelay;
        }
    }


    // 연결된 큐브가 아래로 떨어지기까지 남은 시간
    public float GetChainDownDelayTime()
    {
        if (chainActionDelay > downDelay)
        {
            return 0;
        }
        else
        {
            return downDelay - chainActionDelay;
        }
    }

    // 큐브 미끄러짐 오른쪽 이동
    public bool SlideRight()
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

            return true;
        }


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

        return true;
    }


    // 큐브 미끄러짐 왼쪽 이동
    public bool SlideLeft()
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

            return true;
        }


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

        return true;
    }


    // 큐브 미끄러짐 앞쪽 이동
    public bool SlideForward()
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

            return true;
        }


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

        return true;
    }


    // 큐브 미끄러짐 뒤쪽 이동
    public bool SlideBack()
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
            // 만약 뒤쪽 이동 방향에 큐브가 있다면
            // 같이 미끄러지는 큐브의 경우 이상 없음
            // 정지한 큐브라면 부딪혀서 정지
            //-------------------------------------------
            // 뒤쪽 검사
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

            return true;
        }


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

        return true;
    }


    // 큐브 오른쪽 이동
    public bool MoveRight(ref CubePosData[] cubePosArray, int iIndex, int iMaxSize)
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
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight(ref cubePosArray, iIndex + 1, iMaxSize))
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

            return true;
        }


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
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight(ref cubePosArray, iIndex + 1, iMaxSize))
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

        if (iMaxSize > iIndex)
        {
            // 배열에 저장
            cubePosArray[iIndex] = (new CubePosData(transform.gameObject, transform.position));
        }

        return true;
    }

    // 큐브 왼쪽 이동
    public bool MoveLeft(ref CubePosData[] cubePosArray, int iIndex, int iMaxSize)
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
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 미끄러짐
                slideEvent = true;
            }

            // 만약 왼쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 왼쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.left, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveLeft(ref cubePosArray, iIndex + 1, iMaxSize))
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

            return true;
        }


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
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveLeft(ref cubePosArray, iIndex + 1, iMaxSize))
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

        if (iMaxSize > iIndex)
        {
            // 배열에 저장
            cubePosArray[iIndex] = (new CubePosData(transform.gameObject, transform.position));
        }

        return true;
    }

    // 큐브 앞쪽 이동
    public bool MoveForward(ref CubePosData[] cubePosArray, int iIndex, int iMaxSize)
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
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 미끄러짐
                slideEvent = true;
            }

            // 만약 앞쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 앞쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.forward, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveForward(ref cubePosArray, iIndex + 1, iMaxSize))
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

            return true;
        }


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
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveForward(ref cubePosArray, iIndex + 1, iMaxSize))
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

        if (iMaxSize > iIndex)
        {
            // 배열에 저장
            cubePosArray[iIndex] = (new CubePosData(transform.gameObject, transform.position));
        }

        return true;
    }

    // 큐브 뒤쪽 이동
    public bool MoveBack(ref CubePosData[] cubePosArray, int iIndex, int iMaxSize)
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
            // 처음 미는 중인 경우

            // 바닥 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
            {
                // 미끄러짐
                slideEvent = true;
            }

            // 만약 뒤쪽으로 이동이 불가능한 상황이면 이동하지 않아야함
            // 뒤쪽 검사
            // 있다
            if (Physics.Raycast(transform.position, Vector3.back, out rayHit, 1f, layerMaskCube))
            {
                // 큐브 이동 처리
                // 이동 불가?
                if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveBack(ref cubePosArray, iIndex + 1, iMaxSize))
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

            return true;
        }


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
            if (!rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveBack(ref cubePosArray, iIndex + 1, iMaxSize))
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

        if (iMaxSize > iIndex)
        {
            // 배열에 저장
            cubePosArray[iIndex] = (new CubePosData(transform.gameObject, transform.position));
        }

        return true;
    }


    // 큐브 오른쪽 당겨짐
    public void PullRight(float playerSpeed)
    {
        // 이동 좌표
        destPos = transform.position;
        destPos.x = destPos.x + 1f;
        // 앞쪽 이동
        cubeMoveState = CubeMoveState.PULL_RIGHT;
        // 당겨질 때의 속도 플레이어 속도와 맞춤
        pullSpeed = playerSpeed;
    }


    // 큐브 왼쪽 당겨짐
    public void PullLeft(float playerSpeed)
    {
        // 이동 좌표
        destPos = transform.position;
        destPos.x = destPos.x - 1f;
        // 앞쪽 이동
        cubeMoveState = CubeMoveState.PULL_LEFT;
        // 당겨질 때의 속도 플레이어 속도와 맞춤
        pullSpeed = playerSpeed;
    }


    // 큐브 앞쪽 당겨짐
    public void PullForward(float playerSpeed)
    {
        // 이동 좌표
        destPos = transform.position;
        destPos.z = destPos.z + 1f;
        // 앞쪽 이동
        cubeMoveState = CubeMoveState.PULL_FORWARD;
        // 당겨질 때의 속도 플레이어 속도와 맞춤
        pullSpeed = playerSpeed;
    }


    // 큐브 뒤쪽 당겨짐
    public void PullBack(float playerSpeed)
    {
        // 이동 좌표
        destPos = transform.position;
        destPos.z = destPos.z - 1f;
        // 앞쪽 이동
        cubeMoveState = CubeMoveState.PULL_BACK;
        // 당겨질 때의 속도 플레이어 속도와 맞춤
        pullSpeed = playerSpeed;
    }





    //--------------------------------
    // private 함수
    //--------------------------------

    private void Start()
    {
        ParticleSystem.MainModule particleMain;     // 파티클 메인

        // 상태
        cubeMoveState = CubeMoveState.IDLE;
        // 레이어 마스크 큐브
        layerMaskCube = 1 << LayerMask.NameToLayer("Cube");
        // 딜레이
        actionDelay = 0f;
        // 틱 토큰
        tickToken = 0;
        // 큐브 흔들림 틱
        downTick = 0f;
        // 당겨질 때의 속도
        pullSpeed = 0f;
        // 중력
        isGravity = true;

        // 재질 블럭 할당
        materialBlock = new MaterialPropertyBlock();
        // 교체할 컬러
        IceColor = mate.GetColor(Shader.PropertyToID("_EmissionColor"));
        // 재질 블럭
        materialBlock.SetColor(Shader.PropertyToID("_EmissionColor"), IceColor);

        // 중력 값, 크기 교체
        particleMain = RF_SparksEffect.main;
        originalEffectGravityModifier = particleMain.gravityModifier;
        originalEffectStartSize = particleMain.startSize;

        // 얼음 큐브면
        if (gameObject.CompareTag("IceCube"))
        {
            // 이펙트 재질 교체
            RF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
            RB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
            LF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
            LB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
            // 중력 값, 크기 교체
            particleMain = RF_SparksEffect.main;
            particleMain.gravityModifier = iceEffectGravity;
            particleMain.startSize = iceEffectSize;
            particleMain = RB_SparksEffect.main;
            particleMain.gravityModifier = iceEffectGravity;
            particleMain.startSize = iceEffectSize;
            particleMain = LF_SparksEffect.main;
            particleMain.gravityModifier = iceEffectGravity;
            particleMain.startSize = iceEffectSize;
            particleMain = LB_SparksEffect.main;
            particleMain.gravityModifier = iceEffectGravity;
            particleMain.startSize = iceEffectSize;
        }
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


    // 큐브 이펙트 처리
    private void EffectProcess(Vector3 dir)
    {
        Vector3 effectAngle;    // 이펙트 rotation 각도
        RaycastHit rayHit;      // 레이 충돌한 물체
        ParticleSystem.MainModule particleMain;     // 파티클 메인

        //------------------------------------------
        // 이펙트의 방향을
        // 파라미터 dir의 방향에 맞게 변경합니다
        //------------------------------------------
        if (dir == Vector3.right)
        {
            effectAngle.x = 0f;
            effectAngle.y = 270f;
            effectAngle.z = 0f;
        }
        else if (dir == Vector3.left)
        {
            effectAngle.x = 0f;
            effectAngle.y = 90f;
            effectAngle.z = 0f;
        }
        else if (dir == Vector3.forward)
        {
            effectAngle.x = 0f;
            effectAngle.y = 180f;
            effectAngle.z = 0f;
        }
        else
        {
            effectAngle.x = 0f;
            effectAngle.y = 0f;
            effectAngle.z = 0f;
        }
        // 방향 적용
        RF_SparksEffect.transform.eulerAngles = effectAngle;
        RB_SparksEffect.transform.eulerAngles = effectAngle;
        LF_SparksEffect.transform.eulerAngles = effectAngle;
        LB_SparksEffect.transform.eulerAngles = effectAngle;

        //------------------------------------------
        // 이펙트 켜고 끄기
        // 바닥에 큐브가있다면 마찰을 표현하려고
        // 이펙트가 켜짐
        // 없다면 끔
        //------------------------------------------

        //------------------------------
        // RF
        // 있음
        //------------------------------
        if (Physics.Raycast(RF_SparksEffect.transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 아이스 큐브가 아니면
            if (!gameObject.CompareTag("IceCube"))
            {
                // 바닥이 아이스 큐브면
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 이펙트 재질 교체
                    RF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
                    // 중력 값, 크기 교체
                    particleMain = RF_SparksEffect.main;
                    particleMain.gravityModifier = iceEffectGravity;
                    particleMain.startSize = iceEffectSize;
                }
                // 바닥이 아이스 큐브가 아니면
                else
                {
                    // 이펙트 재질 원상태로 복구
                    RF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(null);
                    // 중력 값, 크기 복구
                    particleMain = RF_SparksEffect.main;
                    particleMain.gravityModifier = originalEffectGravityModifier;
                    particleMain.startSize = originalEffectStartSize;
                }
            }
            // 이펙트 재생
            RF_SparksEffect.Play();
        }
        // 없음
        else
        {
            // 이펙트 재생 정지
            RF_SparksEffect.Stop();
        }

        //------------------------------
        // RB
        // 있음
        //------------------------------
        if (Physics.Raycast(RB_SparksEffect.transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 아이스 큐브가 아니면
            if (!gameObject.CompareTag("IceCube"))
            {
                // 바닥이 아이스 큐브면
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 이펙트 재질 교체
                    RB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
                    // 중력 값, 크기 교체
                    particleMain = RB_SparksEffect.main;
                    particleMain.gravityModifier = iceEffectGravity;
                    particleMain.startSize = iceEffectSize;
                }
                // 바닥이 아이스 큐브가 아니면
                else
                {
                    // 이펙트 재질 원상태로 복구
                    RB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(null);
                    // 중력 값, 크기 복구
                    particleMain = RB_SparksEffect.main;
                    particleMain.gravityModifier = originalEffectGravityModifier;
                    particleMain.startSize = originalEffectStartSize;
                }
            }
            // 이펙트 재생
            RB_SparksEffect.Play();
        }
        // 없음
        else
        {
            // 이펙트 재생 정지
            RB_SparksEffect.Stop();
        }

        //------------------------------
        // LF
        // 있음
        //------------------------------
        if (Physics.Raycast(LF_SparksEffect.transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 아이스 큐브가 아니면
            if (!gameObject.CompareTag("IceCube"))
            {
                // 바닥이 아이스 큐브면
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 이펙트 재질 교체
                    LF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
                    // 중력 값, 크기 교체
                    particleMain = LF_SparksEffect.main;
                    particleMain.gravityModifier = iceEffectGravity;
                    particleMain.startSize = iceEffectSize;
                }
                // 바닥이 아이스 큐브가 아니면
                else
                {
                    // 이펙트 재질 원상태로 복구
                    LF_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(null);
                    // 중력 값, 크기 복구
                    particleMain = LF_SparksEffect.main;
                    particleMain.gravityModifier = originalEffectGravityModifier;
                    particleMain.startSize = originalEffectStartSize;
                }
            }
            // 이펙트 재생
            LF_SparksEffect.Play();
        }
        // 없음
        else
        {
            // 이펙트 재생 정지
            LF_SparksEffect.Stop();
        }

        //------------------------------
        // LB
        // 있음
        //------------------------------
        if (Physics.Raycast(LB_SparksEffect.transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 아이스 큐브가 아니면
            if (!gameObject.CompareTag("IceCube"))
            {
                // 바닥이 아이스 큐브면
                if (rayHit.transform.gameObject.CompareTag("IceCube"))
                {
                    // 이펙트 재질 교체
                    LB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
                    // 중력 값, 크기 교체
                    particleMain = LB_SparksEffect.main;
                    particleMain.gravityModifier = iceEffectGravity;
                    particleMain.startSize = iceEffectSize;
                }
                // 바닥이 아이스 큐브가 아니면
                else
                {
                    // 이펙트 재질 원상태로 복구
                    LB_SparksEffect.gameObject.GetComponent<Renderer>().SetPropertyBlock(null);
                    // 중력 값, 크기 복구
                    particleMain = LB_SparksEffect.main;
                    particleMain.gravityModifier = originalEffectGravityModifier;
                    particleMain.startSize = originalEffectStartSize;
                }
            }
            // 이펙트 재생
            LB_SparksEffect.Play();
        }
        // 없음
        else
        {
            // 이펙트 재생 정지
            LB_SparksEffect.Stop();
        }
    }


    // 떨어지는 상황 체크
    private void GravityCheck()
    {
        Vector3 ray;                        // 레이 시작점
        Vector3 rayDir;                     // 레이 방향
        Vector3 check;                      // 체크할 위치
        Vector3 box;                        // 박스 크기
        Vector3 transAngle;                 // 큐브 Rotation
        RaycastHit rayHit;                  // 레이 충돌한 물체
        CubeMovement otherCubeMovement;     // 큐브 스크립트

        // 중력 영향을 받지않으면
        if (!isGravity)
        {
            return;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        check.x = transform.position.x;
        check.y = transform.position.y - 1f;
        check.z = transform.position.z;

        // 초기화
        actionDelay = 0f;
        // 연결된 큐브 딜레이 초기화
        chainActionDelay = 0f;

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

            otherCubeMovement = rayHit.transform.gameObject.GetComponent<CubeMovement>();

            // 큐브가 떨어질 준비 중
            if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
            {
                // 큐브 흔들기
                cubeMoveState = CubeMoveState.SHAKE;
                // 연결된 큐브 딜레이 가져옴
                chainActionDelay = otherCubeMovement.actionDelay;
                return;
            }
            // 큐브가 아래로 떨어지는 중
            else if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN)
            {
                // 큐브 아래로
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                return;
            }

            // 이동하지 않음
            cubeMoveState = CubeMoveState.IDLE;
            return;
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

            otherCubeMovement = rayHit.transform.gameObject.GetComponent<CubeMovement>();

            // 큐브가 떨어질 준비 중
            if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
            {
                // 큐브 흔들기
                cubeMoveState = CubeMoveState.SHAKE;
                // 연결된 큐브 딜레이 가져옴
                chainActionDelay = otherCubeMovement.actionDelay;
                return;
            }
            // 큐브가 아래로 떨어지는 중
            else if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN)
            {
                // 큐브 아래로
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                return;
            }

            // 이동하지 않음
            cubeMoveState = CubeMoveState.IDLE;
            return;
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

            otherCubeMovement = rayHit.transform.gameObject.GetComponent<CubeMovement>();

            // 큐브가 떨어질 준비 중
            if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
            {
                // 큐브 흔들기
                cubeMoveState = CubeMoveState.SHAKE;
                // 연결된 큐브 딜레이 가져옴
                chainActionDelay = otherCubeMovement.actionDelay;
                return;
            }
            // 큐브가 아래로 떨어지는 중
            else if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN)
            {
                // 큐브 아래로
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                return;
            }

            // 이동하지 않음
            cubeMoveState = CubeMoveState.IDLE;
            return;
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

            otherCubeMovement = rayHit.transform.gameObject.GetComponent<CubeMovement>();

            // 큐브가 떨어질 준비 중
            if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
            {
                // 큐브 흔들기
                cubeMoveState = CubeMoveState.SHAKE;
                // 연결된 큐브 딜레이 가져옴
                chainActionDelay = otherCubeMovement.actionDelay;
                return;
            }
            // 큐브가 아래로 떨어지는 중
            else if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN)
            {
                // 큐브 아래로
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                return;
            }

            // 이동하지 않음
            cubeMoveState = CubeMoveState.IDLE;
            return;
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

            otherCubeMovement = rayHit.transform.gameObject.GetComponent<CubeMovement>();

            // 큐브가 떨어질 준비 중
            if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
            {
                // 큐브 흔들기
                cubeMoveState = CubeMoveState.SHAKE;
                // 연결된 큐브 딜레이 가져옴
                chainActionDelay = otherCubeMovement.actionDelay;
                return;
            }
            // 큐브가 아래로 떨어지는 중
            else if (otherCubeMovement.cubeMoveState == CubeMoveState.DOWN)
            {
                // 큐브 아래로
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                return;
            }

            // 이동하지 않음
            cubeMoveState = CubeMoveState.IDLE;
            return;
        }

        // 처음 떨어지고 있는 중임
        if (cubeMoveState != CubeMoveState.DOWN)
        {
            cubeMoveState = CubeMoveState.DOWN_READY;
        }
        
        return;
    }

    private void MoveProcess()
    {
        Vector3 transAngle;     // 큐브 Rotation
        Vector3 tempPosition;   // 임시 큐브 좌표
        RaycastHit rayHit;      // 레이 충돌한 물체

        switch (cubeMoveState) {
            case CubeMoveState.DOWN_READY:
                // 큐브 떨어짐 준비

                actionDelay = actionDelay + Time.deltaTime;

                // 대기
                if (actionDelay < downDelay)
                {
                    // 큐브 흔들기
                    CubeShake();
                    break;
                }
                // 아래로 떨어짐
                cubeMoveState = CubeMoveState.DOWN;
                // 큐브 Rotation 원상태로 복구
                transAngle.x = 0f;
                transAngle.y = 0f;
                transAngle.z = 0f;
                transform.eulerAngles = transAngle;
                tickToken = 0;
                downTick = 0f;

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

                // 이펙트 처리
                EffectProcess(Vector3.right);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= transform.position.x) {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        SlideRight();
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
                                SlideRight();
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

                // 이펙트 처리
                EffectProcess(Vector3.left);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= transform.position.x)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        SlideLeft();
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
                                SlideLeft();
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

                // 이펙트 처리
                EffectProcess(Vector3.forward);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= transform.position.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        SlideForward();
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
                                SlideForward();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            case CubeMoveState.BACK:
                // 뒤쪽 이동
                transform.position = transform.position + (Vector3.back * horizontalSpeed) * Time.deltaTime;

                // 이펙트 처리
                EffectProcess(Vector3.back);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= transform.position.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 미끄러지는 중임
                    if (slideEvent)
                    {
                        SlideBack();
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
                                SlideBack();
                                break;
                            }
                        }

                        // 큐브 정지
                        cubeMoveState = CubeMoveState.IDLE;
                    }
                }
                break;
            case CubeMoveState.PULL_RIGHT:
                // 오른쪽 당겨짐
                tempPosition = transform.position + (Vector3.right * pullSpeed) * Time.deltaTime;

                // 이펙트 처리
                EffectProcess(Vector3.right);

                //---------------------------
                // 소수점 올림
                // dest <= tempPosition
                //   1  <=  0.999
                // 위와 같은 상황 방지
                //---------------------------
                tempPosition.x = Mathf.Ceil(tempPosition.x * 1000) / 1000;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= tempPosition.x)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 큐브 정지
                    cubeMoveState = CubeMoveState.IDLE;
                }
                else
                {
                    // 큐브 좌표 갱신
                    transform.position = tempPosition;
                }
                break;
            case CubeMoveState.PULL_LEFT:
                // 왼쪽 당겨짐
                tempPosition = transform.position + (Vector3.left * pullSpeed) * Time.deltaTime;

                // 이펙트 처리
                EffectProcess(Vector3.left);

                //---------------------------
                // 소수점 버림
                // dest <= tempPosition
                //   1  <=  1.001
                // 위와 같은 상황 방지
                //---------------------------
                tempPosition.x = Mathf.Floor(tempPosition.x * 1000) / 1000;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= tempPosition.x)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 큐브 정지
                    cubeMoveState = CubeMoveState.IDLE;
                }
                else
                {
                    // 큐브 좌표 갱신
                    transform.position = tempPosition;
                }
                break;
            case CubeMoveState.PULL_FORWARD:
                // 앞쪽 당겨짐
                tempPosition = transform.position + (Vector3.forward * pullSpeed) * Time.deltaTime;

                // 이펙트 처리
                EffectProcess(Vector3.forward);

                //---------------------------
                // 소수점 올림
                // dest <= tempPosition
                //   1  <=  0.99999
                // 위와 같은 상황 방지
                //---------------------------
                tempPosition.z = Mathf.Ceil(tempPosition.z * 1000) / 1000;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= tempPosition.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 큐브 정지
                    cubeMoveState = CubeMoveState.IDLE;
                }
                else
                {
                    // 큐브 좌표 갱신
                    transform.position = tempPosition;
                }
                break;
            case CubeMoveState.PULL_BACK:
                // 뒤쪽 당겨짐
                tempPosition = transform.position + (Vector3.back * pullSpeed) * Time.deltaTime;

                // 이펙트 처리
                EffectProcess(Vector3.back);

                //---------------------------
                // 소수점 버림
                // dest <= tempPosition
                //   1  <=  1.001
                // 위와 같은 상황 방지
                //---------------------------
                tempPosition.z = Mathf.Floor(tempPosition.z * 1000) / 1000;

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= transform.position.z)
                {
                    // 위치 맞추기
                    transform.position = destPos;
                    // 큐브 정지
                    cubeMoveState = CubeMoveState.IDLE;
                }
                else
                {
                    // 큐브 좌표 갱신
                    transform.position = tempPosition;
                }
                break;
            case CubeMoveState.SHAKE:
                // 큐브 흔들기
                CubeShake();
                // 떨어지는 상황인지 확인
                GravityCheck();
                break;
            default:
                // 이펙트 끄기
                RF_SparksEffect.Stop();
                RB_SparksEffect.Stop();
                LF_SparksEffect.Stop();
                LB_SparksEffect.Stop();
                // 떨어지는 상황인지 확인
                GravityCheck();
                break;
        }
    }


    // 큐브 흔들기
    private void CubeShake()
    {
        Vector3 transAngle;     // 큐브 Rotation

        transAngle.x = 0f;
        transAngle.y = 0f;
        transAngle.z = 0f;

        // 큐브 흔들기
        switch (tickToken)
        {
            case 0:
                downTick = downTick + Time.deltaTime;
                if (downTick < TICK_DELAY)
                {
                    break;
                }
                tickToken = 1;
                downTick = 0f;
                transAngle.x = -0.5f;
                transAngle.z = -1f;
                transform.eulerAngles = transAngle;
                break;
            case 1:
                downTick = downTick + Time.deltaTime;
                if (downTick < TICK_DELAY)
                {
                    break;
                }
                tickToken = 2;
                downTick = 0f;
                transAngle.x = 0.5f;
                transAngle.y = -0.5f;
                transAngle.z = 1f;
                transform.eulerAngles = transAngle;
                break;
            case 2:
                downTick = downTick + Time.deltaTime;
                if (downTick < TICK_DELAY)
                {
                    break;
                }
                tickToken = 3;
                downTick = 0f;
                transAngle.x = -0.5f;
                transAngle.y = 1f;
                transAngle.z = -0.5f;
                transform.eulerAngles = transAngle;
                break;
            case 3:
                downTick = downTick + Time.deltaTime;
                if (downTick < TICK_DELAY)
                {
                    break;
                }
                tickToken = 0;
                downTick = 0f;
                transAngle.x = 1f;
                transAngle.y = -0.5f;
                transAngle.z = 0.5f;
                transform.eulerAngles = transAngle;
                break;
        }
    }
}
