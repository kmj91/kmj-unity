using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput playerInput;
    private Animator animator;
    private Camera followCam;
    private GameManager gameManager;
    private Transform centerTrans;
    private LayerMask layerMaskCube;

    // 캐릭터 스피드
    public float speed = 4f;
    // 수직 이동
    public float jumpVelocity = 2f;
    // 속도 지연시간 값
    public float speedSmoothTime = 0.1f;
    // 방향을 바꾸는 스무스 지연시간 값
    public float turnSmoothTime = 0.1f;
    // magnitude 벡터의 길이를 반환
    // 지면상에서의 현제 속도
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude * 3;
    // 현제 y축 속도
    //public float currentY


    // 값의 연속적인 변화량을 기록하기 위한 변수
    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    // 캐릭터의 Y방향에 대한 속도
    private float currentVelocityY;
    // 캐릭터 컬라이더의 이동 속도
    private float animationSpeedPercent;
    // 등반 플래그
    private bool climbingFlag;
    // 위로 점프 애니메이션
    private bool climbingUpAnime;
    // 아래로 점프 애니메이션
    private bool climbingDownAnime;
    // 마우스 클릭
    private bool mouseClick;
    // 상호작용 애니메이션 시작
    private bool interactionAnimeStart;
    // 상호작용 애니메이션 끝
    private bool interactionAnimeEnd;
    // 캐릭터 이동 목표 좌표
    private Vector3 targetPos;
    // 상하좌우 이동 값
    private Vector2 moveKeyValue;

    private enum MoveState {
        IDLE,                       // 대기
        R_IDLE_CLIMBING,            // 오른쪽 매달림 대기
        L_IDLE_CLIMBING,            // 왼쪽 매달림 대기
        F_IDLE_CLIMBING,            // 앞쪽 매달림 대기
        B_IDLE_CLIMBING,            // 뒤쪽 매달림 대기
        R_IDLE_INTERACTION,         // 오른쪽 상호작용 대기
        L_IDLE_INTERACTION,         // 왼쪽 상호작용 대기
        F_IDLE_INTERACTION,         // 앞쪽 상호작용 대기
        B_IDLE_INTERACTION,         // 뒤쪽 상호작용 대기
        R_MOVE,                     // 오른쪽 이동
        L_MOVE,                     // 왼쪽 이동
        F_MOVE,                     // 앞쪽 이동
        B_MOVE,                     // 뒤쪽 이동
        R_UP,                       // 오른쪽 위
        L_UP,                       // 왼쪽 위
        F_UP,                       // 앞쪽 위
        B_UP,                       // 뒤쪽 위
        R_INTERACTION_PUSH,         // 오른쪽 밀기
        R_INTERACTION_PULL,         // 오른쪽 당김
        R_INTERACTION_PULL_CLIMBING, // 오른쪽 당기고 매달림
        L_INTERACTION_PUSH,         // 왼쪽 밀기
        L_INTERACTION_PULL,         // 왼쪽 당김
        L_INTERACTION_PULL_CLIMBING, // 왼쪽 당기고 매달림
        F_INTERACTION_PUSH,         // 앞쪽 밀기
        F_INTERACTION_PULL,         // 앞쪽 당김
        F_INTERACTION_PULL_CLIMBING, // 앞쪽 당기고 매달림
        B_INTERACTION_PUSH,         // 뒤쪽 밀기
        B_INTERACTION_PULL,         // 뒤쪽 당김
        B_INTERACTION_PULL_CLIMBING, // 뒤쪽 당기고 매달림
        R_CLIMBING,                 // 오른쪽 등반
        L_CLIMBING,                 // 왼쪽 등반
        F_CLIMBING,                 // 앞쪽 등반
        B_CLIMBING,                 // 뒤쪽 등반
        RR_CLIMBING_MOVE,           // 오른쪽 등반 오른쪽 이동
        RL_CLIMBING_MOVE,           // 오른쪽 등반 왼쪽 이동
        LR_CLIMBING_MOVE,           // 왼쪽 등반 오른쪽 이동 
        LL_CLIMBING_MOVE,           // 왼쪽 등반 왼쪽 이동
        FR_CLIMBING_MOVE,           // 앞쪽 등반 오른쪽 이동
        FL_CLIMBING_MOVE,           // 앞쪽 등반 왼쪽 이동
        BR_CLIMBING_MOVE,           // 뒤쪽 등반 오른쪽 이동
        BL_CLIMBING_MOVE,           // 뒤쪽 등반 왼쪽 이동
        RR_FL_CHANGE_CLIMBING,      // 오른쪽에서 앞쪽으로 방향 전환
        RL_BL_CHANGE_CLIMBING,      // 오른쪽에서 뒤쪽으로 방향 전환
        LR_BR_CHANGE_CLIMBING,      // 왼쪽에서 뒤쪽으로 방향 전환
        LL_FR_CHANGE_CLIMBING,      // 왼쪽에서 앞쪽으로 방향 전환
        FR_RL_CHANGE_CLIMBING,      // 앞쪽에서 오른쪽으로 방향 전환
        FL_LR_CHANGE_CLIMBING,      // 앞쪽에서 왼쪽으로 방향 전환
        BR_RR_CHANGE_CLIMBING,      // 뒤쪽에서 오른쪽으로 방향 전환
        BL_LL_CHANGE_CLIMBING       // 뒤쪽에서 왼쪽으로 방향 전환
    }

    // 상태
    private MoveState playerMoveState = MoveState.IDLE;


    private void Start()
    {
        // 플레이어 입력
        playerInput = GetComponent<PlayerInput>();
        // 플레이어 움직임 애니메이션
        animator = GetComponent<Animator>();
        // 실제 움직임을 적용할 컴포넌트
        characterController = GetComponent<CharacterController>();
        // 카메라
        followCam = Camera.main;
        // 게임 매니저
        GameObject gameobject = GameObject.Find("GameManager") as GameObject;
        gameManager = gameobject.GetComponent<GameManager>();

        centerTrans = transform.Find("Center");
        // 레이어 마스크
        layerMaskCube = LayerMask.NameToLayer("Cube");
    }

    private void FixedUpdate()
    {
        if (playerInput.click)
        {
            Rotate();
            mouseClick = true;
        }
        else {
            mouseClick = false;
        }

        // 플레이어 이동 처리
        MoveProcess(playerInput.moveInput);
    }

    private void Update()
    {
        Vector2 move;

        switch (playerMoveState)
        {
            case MoveState.L_MOVE:
                move.x = -1;
                move.y = 0;
                break;
            case MoveState.R_MOVE:
                move.x = 1;
                move.y = 0;
                break;
            case MoveState.B_MOVE:
                move.x = 0;
                move.y = -1;
                break;
            case MoveState.F_MOVE:
                move.x = 0;
                move.y = 1;
                break;
            case MoveState.RL_CLIMBING_MOVE:
            case MoveState.LL_CLIMBING_MOVE:
            case MoveState.FR_CLIMBING_MOVE:
            case MoveState.BL_CLIMBING_MOVE:
            case MoveState.RL_BL_CHANGE_CLIMBING:
            case MoveState.LL_FR_CHANGE_CLIMBING:
            case MoveState.FR_RL_CHANGE_CLIMBING:
            case MoveState.BL_LL_CHANGE_CLIMBING:
                move.x = -1;
                move.y = 0;
                break;
            case MoveState.RR_CLIMBING_MOVE:
            case MoveState.LR_CLIMBING_MOVE:
            case MoveState.FL_CLIMBING_MOVE:
            case MoveState.BR_CLIMBING_MOVE:
            case MoveState.RR_FL_CHANGE_CLIMBING:
            case MoveState.LR_BR_CHANGE_CLIMBING:
            case MoveState.FL_LR_CHANGE_CLIMBING:
            case MoveState.BR_RR_CHANGE_CLIMBING:
                move.x = 1;
                move.y = 0;
                break;
            default:
                move.x = 0;
                move.y = 0;
                break;
        }

        UpdateAnimation(move);
    }

    //--------------------------------------------
    // 플레이어 이동 처리
    // moveInput : 입력받은 이동 키값 -1 ~ 1
    //--------------------------------------------
    public void MoveProcess(Vector2 moveInput) {
        Vector3 ray;        // 레이 시작점
        Vector3 rayDir;     // 레이 방향
        Vector3 check;      // 체크할 위치
        Vector3 box;        // 박스 크기
        RaycastHit rayHit;  // 레이 충돌한 물체

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;

        //------------------------------------------------
        // 이동 처리
        // 각각의 대기 상태 (서있을 때 매달려있을 때)에만
        // 키 입력 처리를 합니다
        // 그 외에는 입력된 키의 행동 수행
        //------------------------------------------------
        switch (playerMoveState)
        {
            case MoveState.IDLE:
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (mouseClick)
                {
                    moveKeyValue = Vector2.zero;

                    // 밀기
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 캐릭터가 바라보는 방향으로 큐브가 있나?
                    if (Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        //------------------------------------------------
                        // 해당 방향으로 큐브가 있으면 상호작용 상태로
                        //------------------------------------------------
                        // 정면 보고 있음
                        if ((int)transform.eulerAngles.y == 0)
                        {
                            playerMoveState = MoveState.F_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            break;
                        }
                        // 오른쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 90)
                        {
                            playerMoveState = MoveState.R_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            break;
                        }
                        // 뒤쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 180)
                        {
                            playerMoveState = MoveState.B_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            break;
                        }
                        // 왼쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 270)
                        {
                            playerMoveState = MoveState.L_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            break;
                        }
                    }
                }

                //------------------------------------------------
                // 기본 대기 상태일 때의 키 입력 처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x < 0)
                {
                    if ((int)transform.eulerAngles.y != 270)
                    {
                        transform.eulerAngles = new Vector3(0, 270, 0);
                    }

                    // ← 쪽으로 강하게 키 눌름
                    if (moveInput.x <= -0.3)
                    {
                        ray = centerTrans.position;
                        rayDir = Vector3.down;
                        // 정면 큐브 정보
                        if (!Physics.Raycast(ray, rayDir, out rayHit, 1f))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        targetPos.x = rayHit.transform.position.x - 1f;
                        targetPos.y = rayHit.transform.position.y + 1f;
                        targetPos.z = rayHit.transform.position.z;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // ← 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            //   ？
                            // ★■
                            //--------------------------------
                            // 왼쪽 위쪽 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y + 1;
                            check.z = targetPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // 위쪽 이동
                                targetPos.y = targetPos.y + 1;
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.left;
                                // 왼쪽 이동 등반 상태
                                playerMoveState = MoveState.L_UP;
                                // 애니메이션
                                climbingUpAnime = true;
                            }
                        }
                        // ← 방향 없음
                        else
                        {
                            //--------------------------------
                            // 아래쪽 검사
                            //   ★
                            // ？■
                            //--------------------------------
                            // 왼쪽 아래 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y - 1;
                            check.z = targetPos.z;
                            // 있다
                            if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.left;
                                // 왼쪽 이동 상태
                                playerMoveState = MoveState.L_MOVE;
                            }
                            // 없다
                            else
                            {
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                //   ★
                                //   ■
                                // ？
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                check.y = check.y - 1;
                                // 아래쪽 이동
                                targetPos.y = targetPos.y - 1;
                                // 있다
                                if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 왼쪽 이동 상태
                                    playerMoveState = MoveState.L_MOVE;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 왼쪽 이동 매달림
                                    playerMoveState = MoveState.L_CLIMBING;
                                    // 애니메이션
                                    climbingDownAnime = true;
                                }
                            }
                        }
                    }
                }
                // 입력 키 값 →
                else if (moveInput.x > 0)
                {
                    if ((int)transform.eulerAngles.y != 90)
                    {
                        transform.eulerAngles = new Vector3(0, 90, 0);
                    }

                    // → 쪽으로 강하게 키 눌름
                    if (moveInput.x >= 0.3)
                    {
                        ray = centerTrans.position;
                        rayDir = Vector3.down;
                        // 정면 큐브 정보
                        if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        targetPos.x = rayHit.transform.position.x + 1f;
                        targetPos.y = rayHit.transform.position.y + 1f;
                        targetPos.z = rayHit.transform.position.z;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // → 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            //   ？
                            // ★■
                            //--------------------------------
                            // 오른쪽 위 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y + 1;
                            check.z = targetPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.right;
                                // 오른쪽 이동 등반 상태
                                playerMoveState = MoveState.R_UP;
                                // 애니메이션
                                climbingUpAnime = true;
                            }
                        }
                        // → 방향 없음
                        else
                        {
                            //--------------------------------
                            // 아래쪽 검사
                            //   ★
                            // ？■
                            //--------------------------------
                            // 오른쪽 아래 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y - 1;
                            check.z = targetPos.z;
                            // 있다
                            if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.right;
                                // 오른쪽 이동 상태
                                playerMoveState = MoveState.R_MOVE;
                            }
                            // 없다
                            else
                            {
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                //   ★
                                //   ■
                                // ？
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                check.y = check.y - 1;
                                // 아래쪽 이동
                                targetPos.y = targetPos.y - 1;
                                // 있다
                                if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.right;
                                    // 오른쪽 이동 상태
                                    playerMoveState = MoveState.R_MOVE;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.right;
                                    // 오른쪽 이동 매달림
                                    playerMoveState = MoveState.R_CLIMBING;
                                    // 애니메이션
                                    climbingDownAnime = true;
                                }
                            }
                        }
                    }
                }
                // 입력 키 값 ↓
                else if (moveInput.y < 0)
                {

                    // 뒤
                    if ((int)transform.eulerAngles.y != 180)
                    {
                        transform.eulerAngles = new Vector3(0, 180, 0);
                    }

                    // ↓ 쪽으로 강하게 키 눌름
                    if (moveInput.y <= -0.3)
                    {
                        ray = centerTrans.position;
                        rayDir = Vector3.down;
                        // 정면 큐브 정보
                        if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        targetPos.x = rayHit.transform.position.x;
                        targetPos.y = rayHit.transform.position.y + 1f;
                        targetPos.z = rayHit.transform.position.z - 1f;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // ↓ 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            //   ？
                            // ★■
                            //--------------------------------
                            // 뒤쪽 위 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y + 1;
                            check.z = targetPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.down;
                                // 뒤쪽 이동 등반 상태
                                playerMoveState = MoveState.B_UP;
                                // 애니메이션
                                climbingUpAnime = true;
                            }
                        }
                        // ↓ 방향 없음
                        else
                        {
                            //--------------------------------
                            // 아래쪽 검사
                            //   ★
                            // ？■
                            //--------------------------------
                            // 뒤쪽 아래 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y - 1;
                            check.z = targetPos.z;
                            // 있다
                            if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.down;
                                // 뒤쪽 이동 상태
                                playerMoveState = MoveState.B_MOVE;
                            }
                            // 없다
                            else
                            {
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                //   ★
                                //   ■
                                // ？
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                check.y = check.y - 1;
                                // 아래쪽 이동
                                targetPos.y = targetPos.y - 1;
                                // 있다
                                if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 상태
                                    playerMoveState = MoveState.B_MOVE;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 매달림
                                    playerMoveState = MoveState.B_CLIMBING;
                                    // 애니메이션
                                    climbingDownAnime = true;
                                }
                            }
                        }


                    }
                }
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    // 앞
                    if ((int)transform.eulerAngles.y != 0)
                    {
                        transform.eulerAngles = new Vector3(0, 0, 0);
                    }

                    // ↑ 쪽으로 강하게 키 눌름
                    if (moveInput.y >= 0.3)
                    {
                        ray = centerTrans.position;
                        rayDir = Vector3.down;
                        // 정면 큐브 정보
                        if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        targetPos.x = rayHit.transform.position.x;
                        targetPos.y = rayHit.transform.position.y + 1f;
                        targetPos.z = rayHit.transform.position.z + 1f;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // ↑ 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            //   ？
                            // ★■
                            //--------------------------------
                            // 앞쪽 위 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y + 1;
                            check.z = targetPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.up;
                                // 앞쪽 이동 등반 상태
                                playerMoveState = MoveState.F_UP;
                                // 애니메이션
                                climbingUpAnime = true;
                            }
                        }
                        // ↑ 방향 없음
                        else
                        {
                            //--------------------------------
                            // 아래쪽 검사
                            //   ★
                            // ？■
                            //--------------------------------
                            // 앞쪽 아래 검사
                            check.x = targetPos.x;
                            check.y = targetPos.y - 1;
                            check.z = targetPos.z;
                            // 있다
                            if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                // Move 함수에서 처리할 키 값
                                moveKeyValue = Vector2.up;
                                // 앞쪽 이동 상태
                                playerMoveState = MoveState.F_MOVE;
                            }
                            // 없다
                            else
                            {
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                //   ★
                                //   ■
                                // ？
                                //--------------------------------
                                // 2칸 아래쪽 검사
                                check.y = check.y - 1;
                                // 아래쪽 이동
                                targetPos.y = targetPos.y - 1;
                                // 있다
                                if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 상태
                                    playerMoveState = MoveState.F_MOVE;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 매달림
                                    playerMoveState = MoveState.F_CLIMBING;
                                    // 애니메이션
                                    climbingDownAnime = true;
                                }
                            }
                        }
                    }
                }
                break;
            case MoveState.R_IDLE_CLIMBING:
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x + 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z - 1f;
                    // ↓ 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음
                        //   ↓
                        // ■★
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.RL_CLIMBING_MOVE;
                    }
                    // ↓ 방향 없음
                    else
                    {
                        //--------------------------------
                        // ← 방향 검사
                        // ■★
                        // ？←
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x - 1;
                        check.y = targetPos.y;
                        check.z = targetPos.z;

                        // ← 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.RL_CLIMBING_MOVE;
                        }
                        // ← 방향 없음
                        else
                        {
                            // ← 방향 이동
                            targetPos.x = targetPos.x - 1;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.RL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x + 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z + 1f;
                    // ↑ 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음
                        //   ■
                        // ■★
                        //   ↑
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.RR_CLIMBING_MOVE;
                    }
                    // ↑ 방향 없음
                    else
                    {
                        //--------------------------------
                        // ← 방향 검사
                        // ？←
                        // ■★
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x - 1;
                        check.y = targetPos.y;
                        check.z = targetPos.z;

                        // ← 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.RR_CLIMBING_MOVE;
                        }
                        // ← 방향 없음
                        else
                        {
                            // ← 방향 이동
                            targetPos.x = targetPos.x - 1;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.RR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↓
                else if (moveInput.y < 0)
                {

                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y + 1f;
                    targetPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.L_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                    }
                }
                break;
            case MoveState.L_IDLE_CLIMBING:
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x - 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z + 1f;
                    // ↑ 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음
                        // ■
                        // ★■
                        // ↑
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.LL_CLIMBING_MOVE;
                    }
                    // ↑ 방향 없음
                    else
                    {
                        //--------------------------------
                        // → 방향 검사
                        // →？
                        // ★■
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x + 1;
                        check.y = targetPos.y;
                        check.z = targetPos.z;

                        // → 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.LL_CLIMBING_MOVE;
                        }
                        // → 방향 없음
                        else
                        {
                            // → 방향 이동
                            targetPos.x = targetPos.x + 1;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.LL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x - 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z - 1f;
                    // ↓ 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음
                        // ↓
                        // ★■
                        // ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.LR_CLIMBING_MOVE;
                    }
                    // ↓ 방향 없음
                    else
                    {
                        //--------------------------------
                        // → 방향 검사
                        // ★■
                        // →？
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x + 1;
                        check.y = targetPos.y;
                        check.z = targetPos.z;

                        // → 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.LR_CLIMBING_MOVE;
                        }
                        // → 방향 없음
                        else
                        {
                            // → 방향 이동
                            targetPos.x = targetPos.x + 1;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.LR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↓
                else if (moveInput.y < 0)
                {

                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y + 1f;
                    targetPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.R_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                    }
                }
                break;
            case MoveState.F_IDLE_CLIMBING:
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x - 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z + 1f;
                    // ← 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음 
                        // ■★←
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.FL_CLIMBING_MOVE;
                    }
                    // ← 방향 없음
                    else
                    {
                        //--------------------------------
                        // ↓ 방향 검사
                        // ↓★
                        // ？■
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y;
                        check.z = targetPos.z - 1f;

                        // ↓ 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.FL_CLIMBING_MOVE;
                        }
                        // ↓방향 없음
                        else
                        {
                            // ↓ 방향 이동
                            targetPos.z = targetPos.z - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.FL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                if (moveInput.x >= 0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x + 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z + 1f;
                    // → 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        // →★■
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.FL_CLIMBING_MOVE;
                    }
                    // → 방향 없음
                    else
                    {
                        //--------------------------------
                        // ↓ 방향 검사
                        // ★↓
                        // ■？
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y;
                        check.z = targetPos.z - 1f;

                        // ↓ 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.FR_CLIMBING_MOVE;
                        }
                        // ↓방향 없음
                        else
                        {
                            // ↓ 방향 이동
                            targetPos.z = targetPos.z - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.FR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↓
                else if (moveInput.y < 0)
                {

                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y + 1f;
                    targetPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.B_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                    }
                }
                break;
            case MoveState.B_IDLE_CLIMBING:
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x - 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z - 1f;
                    // ← 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음 
                        //   ■
                        // ■★←
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.BL_CLIMBING_MOVE;
                    }
                    // ← 방향 없음
                    else
                    {
                        //--------------------------------
                        // ↑ 방향 검사
                        // ？■
                        // ↑★
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y;
                        check.z = targetPos.z + 1f;

                        // ↑ 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.BL_CLIMBING_MOVE;
                        }
                        // ↑방향 없음
                        else
                        {
                            // ↑ 방향 이동
                            targetPos.z = targetPos.z + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.BL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                if (moveInput.x >= 0.3)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x + 1f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z - 1f;
                    // → 방향 있음
                    if (Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        //   ■
                        // →★■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.FL_CLIMBING_MOVE;
                    }
                    // → 방향 없음
                    else
                    {
                        //--------------------------------
                        // ↑ 방향 검사
                        // ■？
                        // ★↑
                        //--------------------------------
                        // 앞쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y;
                        check.z = targetPos.z + 1f;

                        // ↑ 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.BR_CLIMBING_MOVE;
                        }
                        // ↑방향 없음
                        else
                        {
                            // ↑ 방향 이동
                            targetPos.z = targetPos.z + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerMoveState = MoveState.BR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↓
                else if (moveInput.y < 0)
                {

                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    ray = centerTrans.position;
                    rayDir = transform.forward;
                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //---------------------------------------------
                    // 매달린 상태에서는 캐릭터가 중앙에 있지 않음
                    //---------------------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y + 1f;
                    targetPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube)) {
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.F_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                    }
                }
                break;
            case MoveState.R_IDLE_INTERACTION:
                // 오른쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick) {
                    moveKeyValue = Vector2.zero;
                    playerMoveState = MoveState.IDLE;
                    interactionAnimeEnd = true;
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3) {
                    // 당김
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube)) {
                        // 에러
                        break;
                    }

                    //--------------------------------
                    // 왼쪽 검사
                    // ？★■
                    //--------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x - 2f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z;
                    
                    // 없다
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        //   ★■
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y - 1f;
                        check.z = targetPos.z;

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 오른쪽 상호작용 당김
                            playerMoveState = MoveState.R_INTERACTION_PULL;
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 오른쪽 상호작용 당김
                            playerMoveState = MoveState.R_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            targetPos.y = targetPos.y - 1f;
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        
                    }
                }
                // 입력 키 값 →
                else if (moveInput.x >= 0.3) {
                    // 밀기
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 오른쪽 검사
                    // ★■？
                    //--------------------------------
                    // 오른쪽 검사
                    check.x = targetPos.x + 1f;
                    check.y = targetPos.y;
                    check.z = targetPos.z;
                    
                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ★■
                        //   ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 오른쪽 상호작용 밀기
                            playerMoveState = MoveState.R_INTERACTION_PUSH;
                        }
                    }
                }
                break;
            case MoveState.L_IDLE_INTERACTION:
                // 왼쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    playerMoveState = MoveState.IDLE;
                    interactionAnimeEnd = true;
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 밀기
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 왼쪽 검사
                    // ？■★
                    //--------------------------------
                    // 왼쪽 검사
                    check.x = targetPos.x - 1f;
                    check.y = targetPos.y;
                    check.z = targetPos.z;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 오른쪽 상호작용 밀기
                            playerMoveState = MoveState.L_INTERACTION_PUSH;
                        }
                    }
                }
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    // 당김
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //--------------------------------
                    // 오른쪽 검사
                    // ■★？
                    //--------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x + 2f;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z;
                    
                    // 없다
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        //     ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y - 1f;
                        check.z = targetPos.z;

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 왼쪽 상호작용 당김
                            playerMoveState = MoveState.L_INTERACTION_PULL;
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 왼쪽 상호작용 당기고 매달림
                            playerMoveState = MoveState.L_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            targetPos.y = targetPos.y - 1f;
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                    }
                }
                break;
            case MoveState.F_IDLE_INTERACTION:
                // 앞쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    playerMoveState = MoveState.IDLE;
                    interactionAnimeEnd = true;
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                if (moveInput.y >= 0.3)
                {
                    // 밀기
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 앞쪽 검사
                    // ？
                    // ■
                    // ★
                    //--------------------------------
                    // 왼쪽 검사
                    check.x = targetPos.x;
                    check.y = targetPos.y;
                    check.z = targetPos.z + 1f;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.z = targetPos.z;
                        check.y = targetPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 앞쪽 상호작용 밀기
                            playerMoveState = MoveState.F_INTERACTION_PUSH;
                        }
                    }
                }
                // 입력 키 값 ↓
                else if (moveInput.y >= -0.3)
                {
                    // 당김
                    ray = centerTrans.position;
                    rayDir = transform.forward;

                    // 정면 큐브 정보
                    if (!Physics.Raycast(ray, rayDir, out rayHit, 1f, 1 << layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    //--------------------------------
                    // 뒤쪽 검사
                    // ■
                    // ★
                    // ？
                    //--------------------------------
                    // 목표 이동 위치
                    targetPos.x = rayHit.transform.position.x;
                    targetPos.y = rayHit.transform.position.y;
                    targetPos.z = rayHit.transform.position.z - 2f;

                    // 없다
                    if (!Physics.CheckBox(targetPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        //     ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = targetPos.x;
                        check.y = targetPos.y - 1;
                        check.z = targetPos.z;

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 앞쪽 상호작용 당김
                            playerMoveState = MoveState.F_INTERACTION_PULL;
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 앞쪽 상호작용 당기고 매달림
                            playerMoveState = MoveState.F_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            targetPos.y = targetPos.y - 1;
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                    }
                }
                break;
            case MoveState.B_IDLE_INTERACTION:
                // 뒤쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    playerMoveState = MoveState.IDLE;
                    interactionAnimeEnd = true;
                }
                break;
            case MoveState.R_MOVE:
                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 바닥에 닿고 상태 변환
                    if (characterController.isGrounded)
                    {
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerMoveState = MoveState.IDLE;
                    }
                }
                break;
            case MoveState.L_MOVE:
                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 바닥에 닿고 상태 변환
                    if (characterController.isGrounded)
                    {
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerMoveState = MoveState.IDLE;
                    }
                }
                break;
            case MoveState.F_MOVE:
                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 바닥에 닿고 상태 변환
                    if (characterController.isGrounded) {
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerMoveState = MoveState.IDLE;
                    }
                }
                break;
            case MoveState.B_MOVE:
                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 바닥에 닿고 상태 변환
                    if (characterController.isGrounded) {
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerMoveState = MoveState.IDLE;
                    }
                }
                break;
            case MoveState.R_UP:
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 위로 이동함
                    currentVelocityY = jumpVelocity;
                    // 애니메이션 플래그
                    //climbingUpAnime = true;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.IDLE;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.L_UP:
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 위로 이동함
                    currentVelocityY = jumpVelocity;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.IDLE;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.F_UP:
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 위로 이동함
                    currentVelocityY = jumpVelocity;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.IDLE;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.B_UP:
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 위로 이동함
                    currentVelocityY = jumpVelocity;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.IDLE;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.R_INTERACTION_PUSH:
                // 오른쪽 밀기
                break;
            case MoveState.R_INTERACTION_PULL:
                // 오른쪽 당김
                break;
            case MoveState.R_INTERACTION_PULL_CLIMBING:
                // 오른쪽 당기고 매달림
                break;
            case MoveState.L_INTERACTION_PUSH:
                // 왼쪽 밀기
                break;
            case MoveState.L_INTERACTION_PULL:
                // 왼쪽 당김

                break;
            case MoveState.L_INTERACTION_PULL_CLIMBING:
                // 왼쪽 당기고 매달림

                // 바닥에 닿아있지 않음
                if (!characterController.isGrounded)
                {
                    // x,z 이동 멈춤
                    moveKeyValue = Vector2.zero;
                    climbingDownAnime = true;
                }
                else
                {
                    moveKeyValue = Vector2.right;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.y >= centerTrans.position.y)
                {
                    // 매달림
                    climbingFlag = true;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.F_INTERACTION_PUSH:
                // 앞쪽 밀기
                break;
            case MoveState.F_INTERACTION_PULL:
                // 앞쪽 당김
                break;
            case MoveState.F_INTERACTION_PULL_CLIMBING:
                // 앞쪽 당기고 매달림
                break;
            case MoveState.B_INTERACTION_PUSH:
                // 뒤쪽 밀기
                break;
            case MoveState.B_INTERACTION_PULL:
                // 뒤쪽 당김
                break;
            case MoveState.B_INTERACTION_PULL_CLIMBING:
                // 뒤쪽 당기고 매달림
                break;
            case MoveState.R_CLIMBING:
                // 바닥에 닿아있지 않음
                if (!characterController.isGrounded)
                {
                    // x,z 이동 멈춤
                    moveKeyValue = Vector2.zero;
                    // 방향 바꿈
                    if ((int)transform.eulerAngles.y != 270)
                    {
                        transform.eulerAngles = new Vector3(0, 270, 0);
                    }
                }
                else
                {
                    moveKeyValue = Vector2.right;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.y >= centerTrans.position.y)
                {
                    // 매달림
                    climbingFlag = true;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.L_CLIMBING:
                // 바닥에 닿아있지 않음
                if (!characterController.isGrounded)
                {
                    // x,z 이동 멈춤
                    moveKeyValue = Vector2.zero;
                    // 방향 바꿈
                    if ((int)transform.eulerAngles.y != 90)
                    {
                        transform.eulerAngles = new Vector3(0, 90, 0);
                    }
                }
                else
                {
                    moveKeyValue = Vector2.left;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.y >= centerTrans.position.y)
                {
                    // 매달림
                    climbingFlag = true;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.F_CLIMBING:
                // 바닥에 닿아있지 않음
                if (!characterController.isGrounded)
                {
                    // x,z 이동 멈춤
                    moveKeyValue = Vector2.zero;
                    // 방향 바꿈
                    if ((int)transform.eulerAngles.y != 180)
                    {
                        transform.eulerAngles = new Vector3(0, 180, 0);
                    }
                }
                else
                {
                    moveKeyValue = Vector2.up;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.y >= centerTrans.position.y)
                {
                    // 매달림
                    climbingFlag = true;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.B_CLIMBING:
                // 바닥에 닿아있지 않음
                if (!characterController.isGrounded)
                {
                    // x,z 이동 멈춤
                    moveKeyValue = Vector2.zero;
                    // 방향 바꿈
                    if ((int)transform.eulerAngles.y != 0)
                    {
                        transform.eulerAngles = new Vector3(0, 0, 0);
                    }
                }
                else
                {
                    moveKeyValue = Vector2.down;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.y >= centerTrans.position.y)
                {
                    // 매달림
                    climbingFlag = true;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.RR_CLIMBING_MOVE:
                //------------------------------
                // 오른쪽 매달림 오른쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 Z축에 막힘
                    //   ■
                    // ■★
                    //   ↑
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ←★
                    // ■↑
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerMoveState = MoveState.RR_FL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.RL_CLIMBING_MOVE:
                //------------------------------
                // 오른쪽 매달림 왼쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 Z축에 막힘
                    //   ↓
                    // ■★
                    //   ■
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 왼쪽 이동중에 없음
                    // ■↓
                    // ←★
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerMoveState = MoveState.RL_BL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.LR_CLIMBING_MOVE:
                //------------------------------
                // 왼쪽 매달림 오른쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 Z축에 막힘
                    // ↓
                    // ★■
                    // ■
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ↓■
                    // ★→
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerMoveState = MoveState.LR_BR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.LL_CLIMBING_MOVE:
                //------------------------------
                // 왼쪽 매달림 왼쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 Z축에 막힘
                    //   ■
                    // ■★
                    //   ↑
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 왼쪽 이동중에 없음
                    // ★→
                    // ↑■
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerMoveState = MoveState.LL_FR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.FR_CLIMBING_MOVE:
                //------------------------------
                // 앞쪽 매달림 오른쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 X축에 막힘
                    // →★■
                    //   ■
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // →★
                    // ■↓
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerMoveState = MoveState.FR_RL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.FL_CLIMBING_MOVE:
                //------------------------------
                // 앞쪽 매달림 왼쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 X축에 막힘
                    // ■★←
                    //   ■
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ★←
                    // ↓■
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerMoveState = MoveState.FL_LR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.BR_CLIMBING_MOVE:
                //------------------------------
                // 뒤쪽 매달림 오른쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 X축에 막힘
                    //   ■
                    // →★■
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ■↑
                    // →★
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerMoveState = MoveState.BR_RR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.BL_CLIMBING_MOVE:
                //------------------------------
                // 앞쪽 매달림 왼쪽 이동
                //------------------------------
                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 X축에 막힘
                    //   ■
                    // ■★←
                    //--------------------------------
                }

                ray = centerTrans.position;
                rayDir = transform.forward;

                // 앞쪽에 없음
                if (!Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ↑■
                    // ★←
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerMoveState = MoveState.BL_LL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.RR_FL_CHANGE_CLIMBING:
                //------------------------------
                // 오른쪽에서 앞쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 뭔가 거치적거려서 멈춤
                    //   ★←
                    // ■
                    //--------------------------------
                    moveKeyValue = Vector2.up;
                }
                // 거치적 거리지 않음
                else {
                    moveKeyValue = Vector2.left;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.RL_BL_CHANGE_CLIMBING:
                //------------------------------
                // 오른쪽에서 뒤쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 왼쪽 이동중에 뭔가 거치적거려서 멈춤
                    // ■
                    //   ★←
                    //--------------------------------
                    moveKeyValue = Vector2.down;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.left;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.LR_BR_CHANGE_CLIMBING:
                //------------------------------
                // 왼쪽에서 뒤쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 뭔가 거치적거려서 멈춤
                    // →★
                    //     ■
                    //--------------------------------
                    moveKeyValue = Vector2.up;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.right;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.LL_FR_CHANGE_CLIMBING:
                //------------------------------
                // 왼쪽에서 앞쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 뭔가 거치적거려서 멈춤
                    // →★
                    //     ■
                    //--------------------------------
                    moveKeyValue = Vector2.up;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.right;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.FR_RL_CHANGE_CLIMBING:
                //------------------------------
                // 앞쪽에서 오른쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 아래쪽 이동중에 뭔가 거치적거려서 멈춤
                    //   ↓
                    //   ★  
                    // ■
                    //--------------------------------
                    moveKeyValue = Vector2.right;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.down;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.FL_LR_CHANGE_CLIMBING:
                //------------------------------
                // 앞쪽에서 왼쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 아래쪽 이동중에 뭔가 거치적거려서 멈춤
                    // ↓
                    // ★  
                    //   ■
                    //--------------------------------
                    moveKeyValue = Vector2.left;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.down;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.BR_RR_CHANGE_CLIMBING:
                //------------------------------
                // 뒤쪽에서 오른쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 위쪽 이동중에 뭔가 거치적거려서 멈춤
                    // ■
                    //   ★
                    //   ↑
                    //--------------------------------
                    moveKeyValue = Vector2.right;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.up;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case MoveState.BL_LL_CHANGE_CLIMBING:
                //------------------------------
                // 뒤쪽에서 왼쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 위쪽 이동중에 뭔가 거치적거려서 멈춤
                    //   ■
                    // ★
                    // ↑
                    //--------------------------------
                    moveKeyValue = Vector2.left;
                }
                // 거치적 거리지 않음
                else
                {
                    moveKeyValue = Vector2.up;
                }

                // 이동 거리만큼 이동 했는가
                if (targetPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
        }// switch(playerMoveState)

        Debug.Log("playerMoveState : " + playerMoveState);
        Debug.Log("moveKeyValue : " + moveKeyValue);
        //Debug.Log(mouseClick);
        //Debug.Log(interactionAnimeStart);
        //Debug.Log("--------------------------------");
        Move(moveKeyValue);
    }

    private void Move(Vector2 moveInput) {
        float targetSpeed = speed * moveInput.magnitude;
        
        // Normalize 벡터의 크기를 1로 정규화 하는 함수
        // x, z 평면
        //var moveDiection = Vector3.Normalize(transform.forward * moveInput.y + transform.forward * moveInput.x);
        var moveDiection = new Vector3(moveInput.x, 0, moveInput.y);
        // 목표 까지 스무스하게
        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
        // 매달리는 중이 아닐때만 중력 처리
        if (!climbingFlag)
        {
            // 중력 값
            currentVelocityY = currentVelocityY + (Time.deltaTime * Physics.gravity.y);
        }
        else {
            // 매달리는 중임
            currentVelocityY = 0f;
        }
        // y 축
        var velocity = moveDiection * targetSpeed + Vector3.up * currentVelocityY;
        //var velocity = (moveDiection + Vector3.up * currentVelocityY);
        // 월드 스페이스 이동
        characterController.Move(velocity * Time.deltaTime);
        // 바닥에 닿아있다면
        if (characterController.isGrounded)
        {
            // 다음 프레임을 위해 0으로 초기화
            currentVelocityY = 0f;
        }
    }

    private void Rotate() {
        var targetRotaion = followCam.transform.eulerAngles.y;

        targetRotaion = Mathf.SmoothDampAngle(targetRotaion, transform.eulerAngles.y,
            ref turnSmoothVelocity, turnSmoothTime);

        //Camera.main.transform.eulerAngles = Vector3.up * targetRotaion;
        //GameObject.Find("Follow Cam").transform.GetComponent<CinemachineFreeLook>().LookAt = GameObject.Find("Cube (4)").transform;
        GameObject.Find("Follow Cam").transform.eulerAngles = transform.eulerAngles;

       // Debug.Log("eulerAngles" + GameObject.Find("Follow Cam").transform.eulerAngles);
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        animationSpeedPercent = currentSpeed / speed;
        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
        if (climbingUpAnime)
        {
            animator.SetTrigger("Climbing Up");
            climbingUpAnime = false;
        }

        if (climbingDownAnime) {
            animator.SetTrigger("Climbing Down");
            climbingDownAnime = false;
        }

        if (interactionAnimeStart)
        {
            animator.SetTrigger("Idle_Interaction_Start");
            interactionAnimeStart = false;
        }

        if (interactionAnimeEnd)
        {
            animator.SetTrigger("Idle_Interaction_End");
            interactionAnimeEnd = false;
        }
    }
}
