﻿using System;
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
    private GameObject moveCube;

    // 캐릭터 스피드
    public float speed = 2f;
    // 수직 이동
    public float jumpVelocity = 2.5f;
    // 속도 지연시간 값
    public float speedSmoothTime = 0.01f;
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
    private float jumpSmoothHorizontal;
    private float jumpSmoothVertical;
    // 캐릭터의 Y방향에 대한 속도
    private float currentVelocityY;
    // 캐릭터 컬라이더의 이동 속도
    private float animationSpeedPercent;
    // 딜레이
    private float actionDelay = 0;
    // 캐릭터 스피드 저장
    private float saveSpeed;
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
    private Vector3 destPos;
    // 큐브 이동 목표 좌표
    private Vector3 cubeDestPos;
    // 상하좌우 이동 값
    private Vector2 moveKeyValue;
    // 상태
    private MoveState playerMoveState = MoveState.IDLE;

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
        INTERACTION_PUSH,           // 상호작용 밀기
        R_INTERACTION_PULL,         // 오른쪽 당김
        R_INTERACTION_PULL_CLIMBING, // 오른쪽 당기고 매달림
        L_INTERACTION_PULL,         // 왼쪽 당김
        L_INTERACTION_PULL_CLIMBING, // 왼쪽 당기고 매달림
        F_INTERACTION_PULL,         // 앞쪽 당김
        F_INTERACTION_PULL_CLIMBING, // 앞쪽 당기고 매달림
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

        // 플레이어 애니메이션
        UpdateAnimation();
        // 플레이어 이동 처리
        MoveProcess();
    }

    //--------------------------------------------
    // 플레이어 이동 처리
    // moveInput : 입력받은 이동 키값 -1 ~ 1
    //--------------------------------------------
    public void MoveProcess() {
        Vector2 moveInput;      // 카메라 뱡향에 다라 변화된 키 값
        Vector3 ray;            // 레이 시작점
        Vector3 rayDir;         // 레이 방향
        Vector3 check;          // 체크할 위치
        Vector3 box;            // 박스 크기
        Vector3 moveValue;      // 위치
        RaycastHit rayHit;      // 레이 충돌한 물체
        float followCamAngleY;  // 카메라 방향

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;
        moveValue.x = 0f;
        moveValue.y = 0f;
        moveValue.z = 0f;
        followCamAngleY = followCam.transform.eulerAngles.y;

        switch (playerMoveState) {
            case MoveState.R_IDLE_CLIMBING:
            case MoveState.L_IDLE_CLIMBING:
            case MoveState.F_IDLE_CLIMBING:
            case MoveState.B_IDLE_CLIMBING:
                // 매달려 있을 때는 키입력 그대로
                moveInput = playerInput.moveInput;
                break;
            default:
                //------------------------------------------------
                // 카메라 방향에 따른 키 입력 값 변화
                //------------------------------------------------
                // 오른쪽 →
                if (45 <= followCamAngleY && followCamAngleY < 135)
                {
                    // 오른쪽 방향에서의 y 값은 x 값
                    moveInput.x = playerInput.moveInput.y;
                    // 키입력 x 값이 음수는 +y, 양수는 -y 변환
                    moveInput.y = -playerInput.moveInput.x;
                }
                // 뒤쪽 ↓
                else if (135 <= followCamAngleY && followCamAngleY < 225)
                {
                    // 뒤쪽 방향에서의 키입력은 전부 뒤집어진다
                    moveInput.x = -playerInput.moveInput.x;
                    moveInput.y = -playerInput.moveInput.y;
                }
                // 왼쪽 ←
                else if (225 <= followCamAngleY && followCamAngleY < 315)
                {
                    // 키입력 y 값이 음수는 +x, 양수는 -x 변환
                    moveInput.x = -playerInput.moveInput.y;
                    // 왼쪽 방향에서의 x 값은 y 값
                    moveInput.y = playerInput.moveInput.x;
                }
                // 앞쪽 ↑
                //else if (315 <= followCamAngleY && 360|| followCamAngleY< 45)
                else
                {
                    // 정방향에서는 같다
                    moveInput = playerInput.moveInput;
                }
                break;
        }

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
                            // 큐브에 바싹 붙음
                            moveValue.z = 0.25f;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 오른쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 90)
                        {
                            playerMoveState = MoveState.R_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            // 큐브에 바싹 붙음
                            moveValue.x = 0.25f;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 뒤쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 180)
                        {
                            playerMoveState = MoveState.B_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            // 큐브에 바싹 붙음
                            moveValue.z = -0.25f;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 왼쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 270)
                        {
                            playerMoveState = MoveState.L_IDLE_INTERACTION;
                            interactionAnimeStart = true;
                            // 큐브에 바싹 붙음
                            moveValue.x = -0.25f;
                            characterController.Move(moveValue);
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
                        destPos.x = rayHit.transform.position.x - 1f;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // ← 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            // ？
                            // ■★
                            //--------------------------------
                            // 왼쪽 위쪽 검사
                            check.x = destPos.x;
                            check.y = destPos.y + 1f;
                            check.z = destPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                //   ？
                                // ■★
                                //--------------------------------
                                // 위쪽 검사
                                check.x = check.x + 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 왼쪽 이동 등반 상태
                                    playerMoveState = MoveState.L_UP;
                                    // 애니메이션
                                    climbingUpAnime = true;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
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
                            check.x = destPos.x;
                            check.y = destPos.y - 1f;
                            check.z = destPos.z;
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
                                check.y = check.y - 1f;
                                // 아래쪽 이동
                                destPos.y = destPos.y - 1f;
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
                        destPos.x = rayHit.transform.position.x + 1f;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z;

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
                            check.x = destPos.x;
                            check.y = destPos.y + 1f;
                            check.z = destPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                // ？
                                // ★■
                                //--------------------------------
                                // 위쪽 검사
                                check.x = check.x - 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    //moveKeyValue = Vector2.right;
                                    // 오른쪽 이동 등반 상태
                                    playerMoveState = MoveState.R_UP;
                                    // 애니메이션
                                    climbingUpAnime = true;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
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
                            check.x = destPos.x;
                            check.y = destPos.y - 1f;
                            check.z = destPos.z;
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
                                check.y = check.y - 1f;
                                // 아래쪽 이동
                                destPos.y = destPos.y - 1f;
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
                        destPos.x = rayHit.transform.position.x;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z - 1f;

                        ray = centerTrans.position;
                        rayDir = transform.forward;

                        // ↓ 방향 있음
                        if (Physics.Raycast(ray, rayDir, 1f, 1 << layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            // ？
                            // ■★
                            //--------------------------------
                            // 뒤쪽 위 검사
                            check.x = destPos.x;
                            check.y = destPos.y + 1f;
                            check.z = destPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                //   ？
                                // ■★
                                //--------------------------------
                                // 위쪽 검사
                                check.z = check.z + 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 등반 상태
                                    playerMoveState = MoveState.B_UP;
                                    // 애니메이션
                                    climbingUpAnime = true;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
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
                            check.x = destPos.x;
                            check.y = destPos.y - 1f;
                            check.z = destPos.z;
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
                                check.y = check.y - 1f;
                                // 아래쪽 이동
                                destPos.y = destPos.y - 1f;
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
                        destPos.x = rayHit.transform.position.x;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z + 1f;

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
                            check.x = destPos.x;
                            check.y = destPos.y + 1;
                            check.z = destPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                // ？
                                // ★■
                                //--------------------------------
                                // 위쪽 검사
                                check.z = check.z - 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                                {
                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 등반 상태
                                    playerMoveState = MoveState.F_UP;
                                    // 애니메이션
                                    climbingUpAnime = true;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
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
                            check.x = destPos.x;
                            check.y = destPos.y - 1f;
                            check.z = destPos.z;
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
                                check.y = check.y - 1f;
                                // 아래쪽 이동
                                destPos.y = destPos.y - 1f;
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
                    destPos.x = rayHit.transform.position.x + 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z - 1f;
                    // ↓ 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x - 1;
                        check.y = destPos.y;
                        check.z = destPos.z;

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
                            destPos.x = destPos.x - 1;
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
                    destPos.x = rayHit.transform.position.x + 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z + 1f;
                    // ↑ 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x - 1;
                        check.y = destPos.y;
                        check.z = destPos.z;

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
                            destPos.x = destPos.x - 1;
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y + 1f;
                    destPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.L_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
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
                    destPos.x = rayHit.transform.position.x - 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z + 1f;
                    // ↑ 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x + 1;
                        check.y = destPos.y;
                        check.z = destPos.z;

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
                            destPos.x = destPos.x + 1;
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
                    destPos.x = rayHit.transform.position.x - 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z - 1f;
                    // ↓ 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x + 1;
                        check.y = destPos.y;
                        check.z = destPos.z;

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
                            destPos.x = destPos.x + 1;
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y + 1f;
                    destPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.R_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
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
                    destPos.x = rayHit.transform.position.x + 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z + 1f;
                    // → 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        // →★■
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.FR_CLIMBING_MOVE;
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
                        check.x = destPos.x;
                        check.y = destPos.y;
                        check.z = destPos.z - 1f;

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
                            destPos.z = destPos.z - 1f;
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
                    destPos.x = rayHit.transform.position.x - 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z + 1f;
                    // ← 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x;
                        check.y = destPos.y;
                        check.z = destPos.z - 1f;

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
                            destPos.z = destPos.z - 1f;
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y + 1f;
                    destPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.B_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
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
                    destPos.x = rayHit.transform.position.x - 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z - 1f;
                    // ← 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
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
                        check.x = destPos.x;
                        check.y = destPos.y;
                        check.z = destPos.z + 1f;

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
                            destPos.z = destPos.z + 1f;
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
                    destPos.x = rayHit.transform.position.x + 1f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z - 1f;
                    // → 방향 있음
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        //   ■
                        // →★■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerMoveState = MoveState.BR_CLIMBING_MOVE;
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
                        check.x = destPos.x;
                        check.y = destPos.y;
                        check.z = destPos.z + 1f;

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
                            destPos.z = destPos.z + 1f;
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y + 1f;
                    destPos.z = rayHit.transform.position.z;
                    // ↑ 방향 없음
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube)) {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 앞쪽 이동 등반 상태
                        playerMoveState = MoveState.F_UP;
                        // 애니메이션
                        climbingUpAnime = true;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
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
                    // 원 위치로 돌아감
                    moveValue.x = -0.25f;
                    characterController.Move(moveValue);
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
                    destPos.x = rayHit.transform.position.x - 2f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;
                    
                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        //   ★■
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;
                        check.z = destPos.z;

                        // 큐브 이동 목표 좌표
                        cubeDestPos.x = destPos.x + 1f;
                        cubeDestPos.y = destPos.y;
                        cubeDestPos.z = destPos.z;
                        // 이동할 큐브 오브젝트
                        moveCube = rayHit.transform.gameObject;

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
                            destPos.y = destPos.y - 1f;
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 오른쪽 검사
                    // ★■？
                    //--------------------------------
                    // 오른쪽 검사
                    check.x = destPos.x + 1f;
                    check.y = destPos.y;
                    check.z = destPos.z;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ★■
                        //   ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 오른쪽 상호작용 밀기
                            playerMoveState = MoveState.INTERACTION_PUSH;
                            // 큐브 이동 처리
                            rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveRight();
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
                    // 원 위치로 돌아감
                    moveValue.x = 0.25f;
                    characterController.Move(moveValue);
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 왼쪽 검사
                    // ？■★
                    //--------------------------------
                    // 왼쪽 검사
                    check.x = destPos.x - 1f;
                    check.y = destPos.y;
                    check.z = destPos.z;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 오른쪽 상호작용 밀기
                            playerMoveState = MoveState.INTERACTION_PUSH;
                            // 큐브 이동 처리
                            rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveLeft();
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
                    destPos.x = rayHit.transform.position.x + 2f;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;
                    
                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        //     ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;
                        check.z = destPos.z;

                        // 큐브 이동 목표 좌표
                        cubeDestPos.x = destPos.x - 1f;
                        cubeDestPos.y = destPos.y;
                        cubeDestPos.z = destPos.z;
                        // 이동할 큐브 오브젝트
                        moveCube = rayHit.transform.gameObject;

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
                            destPos.y = destPos.y - 1f;
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
                    // 원 위치로 돌아감
                    moveValue.z = -0.25f;
                    characterController.Move(moveValue);
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 앞쪽 검사
                    // ？
                    // ■
                    // ★
                    //--------------------------------
                    // 앞쪽 검사
                    check.x = destPos.x;
                    check.y = destPos.y;
                    check.z = destPos.z + 1f;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.z = destPos.z;
                        check.y = destPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 앞쪽 상호작용 밀기
                            playerMoveState = MoveState.INTERACTION_PUSH;
                            // 큐브 이동 처리
                            rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveForward();
                        }
                    }
                }
                // 입력 키 값 ↓
                else if (moveInput.y <= -0.3)
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z - 2f;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        //     ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;
                        check.z = destPos.z;

                        // 큐브 이동 목표 좌표
                        cubeDestPos.x = destPos.x;
                        cubeDestPos.y = destPos.y;
                        cubeDestPos.z = destPos.z + 1f;
                        // 이동할 큐브 오브젝트
                        moveCube = rayHit.transform.gameObject;

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
                            destPos.y = destPos.y - 1f;
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
                    // 원 위치로 돌아감
                    moveValue.z = 0.25f;
                    characterController.Move(moveValue);
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                if (moveInput.y >= 0.3)
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
                    // 앞쪽 검사
                    // ？
                    // ★
                    // ■
                    //--------------------------------
                    // 목표 이동 위치
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z + 2f;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        //     ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.x = destPos.x;
                        check.y = destPos.y - 1f;
                        check.z = destPos.z;

                        // 큐브 이동 목표 좌표
                        cubeDestPos.x = destPos.x;
                        cubeDestPos.y = destPos.y;
                        cubeDestPos.z = destPos.z - 1f;
                        // 이동할 큐브 오브젝트
                        moveCube = rayHit.transform.gameObject;

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 뒤쪽 상호작용 당김
                            playerMoveState = MoveState.B_INTERACTION_PULL;
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 앞쪽 상호작용 당기고 매달림
                            playerMoveState = MoveState.B_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            destPos.y = destPos.y - 1f;
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                    }
                }
                // 입력 키 값 ↓
                else if (moveInput.y <= -0.3)
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
                    destPos.x = rayHit.transform.position.x;
                    destPos.y = rayHit.transform.position.y;
                    destPos.z = rayHit.transform.position.z;

                    //--------------------------------
                    // 뒤쪽 검사
                    // ★
                    // ■
                    // ？
                    //--------------------------------
                    // 앞쪽 검사
                    check.x = destPos.x;
                    check.y = destPos.y;
                    check.z = destPos.z - 1f;

                    // 없다
                    if (!Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                    {
                        //--------------------------------
                        // 아래쪽 검사
                        // ■★
                        // ？
                        //--------------------------------
                        // 아래쪽 검사
                        check.z = destPos.z;
                        check.y = destPos.y - 1f;

                        // 있다
                        if (Physics.CheckBox(check, box, Quaternion.identity, 1 << layerMaskCube))
                        {
                            // 앞쪽 상호작용 밀기
                            playerMoveState = MoveState.INTERACTION_PUSH;
                            // 큐브 이동 처리
                            rayHit.transform.gameObject.GetComponent<CubeMovement>().MoveBack();
                        }
                    }
                }
                break;
            case MoveState.R_MOVE:
                // 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
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
                if (destPos.x >= centerTrans.position.x)
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
                if (destPos.z <= centerTrans.position.z)
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
                if (destPos.z >= centerTrans.position.z)
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
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < 0.15f)
                {
                    break;
                }
                else {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.right;
                }

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, speedSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, speedSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                break;
            case MoveState.L_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < 0.15f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.left;
                }

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, speedSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, speedSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                break;
            case MoveState.F_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < 0.15f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.up;
                }

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, speedSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, speedSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                break;
            case MoveState.B_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < 0.15f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.down;
                }

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, speedSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, speedSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                break;
            case MoveState.INTERACTION_PUSH:
                // 상호작용 밀기

                break;
            case MoveState.R_INTERACTION_PULL:
                // 오른쪽 당김
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.left * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x >= moveCube.transform.position.x)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    moveCube.transform.position = cubeDestPos;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerMoveState = MoveState.R_IDLE_INTERACTION;
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerMoveState = MoveState.IDLE;
                        // 애니메이션 종료
                        interactionAnimeEnd = true;
                        // 원 위치로 돌아감
                        moveValue.x = -0.25f;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case MoveState.R_INTERACTION_PULL_CLIMBING:
                // 오른쪽 당기고 매달림
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.left * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x >= moveCube.transform.position.x)
                {
                    // 이동 완료
                    moveCube.transform.position = cubeDestPos;
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        climbingDownAnime = true;
                        // 상태 변경
                        playerMoveState = MoveState.L_CLIMBING;
                    }
                }
                break;
            case MoveState.L_INTERACTION_PULL:
                // 왼쪽 당김
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.right * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x <= moveCube.transform.position.x)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    moveCube.transform.position = cubeDestPos;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerMoveState = MoveState.L_IDLE_INTERACTION;
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerMoveState = MoveState.IDLE;
                        // 애니메이션 종료
                        interactionAnimeEnd = true;
                        // 원 위치로 돌아감
                        moveValue.x = 0.25f;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case MoveState.L_INTERACTION_PULL_CLIMBING:
                // 왼쪽 당기고 매달림
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.right * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x <= moveCube.transform.position.x)
                {
                    // 이동 완료
                    moveCube.transform.position = cubeDestPos;
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        climbingDownAnime = true;
                        // 상태 변경
                        playerMoveState = MoveState.R_CLIMBING;
                    }
                }
                break;
            case MoveState.F_INTERACTION_PULL:
                // 앞쪽 당김
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.back * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z >= moveCube.transform.position.z)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    moveCube.transform.position = cubeDestPos;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerMoveState = MoveState.F_IDLE_INTERACTION;
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerMoveState = MoveState.IDLE;
                        // 애니메이션 종료
                        interactionAnimeEnd = true;
                        // 원 위치로 돌아감
                        moveValue.z = -0.25f;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case MoveState.F_INTERACTION_PULL_CLIMBING:
                // 앞쪽 당기고 매달림
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.back * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z >= moveCube.transform.position.z)
                {
                    // 이동 완료
                    moveCube.transform.position = cubeDestPos;
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        climbingDownAnime = true;
                        // 상태 변경
                        playerMoveState = MoveState.B_CLIMBING;
                    }
                }
                break;
            case MoveState.B_INTERACTION_PULL:
                // 뒤쪽 당김
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.forward * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z <= moveCube.transform.position.z)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    moveCube.transform.position = cubeDestPos;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerMoveState = MoveState.B_IDLE_INTERACTION;
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerMoveState = MoveState.IDLE;
                        // 애니메이션 종료
                        interactionAnimeEnd = true;
                        // 원 위치로 돌아감
                        moveValue.z = 0.25f;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case MoveState.B_INTERACTION_PULL_CLIMBING:
                // 뒤쪽 당기고 매달림
                // 큐브 이동
                moveCube.transform.position = moveCube.transform.position + (Vector3.forward * speed) * Time.deltaTime;
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z <= moveCube.transform.position.z)
                {
                    // 이동 완료
                    moveCube.transform.position = cubeDestPos;
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        climbingDownAnime = true;
                        // 상태 변경
                        playerMoveState = MoveState.F_CLIMBING;
                    }
                }
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
                if (destPos.y >= centerTrans.position.y)
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
                if (destPos.y >= centerTrans.position.y)
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
                if (destPos.y >= centerTrans.position.y)
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
                if (destPos.y >= centerTrans.position.y)
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
                    // 오른쪽으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerMoveState = MoveState.BR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
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
                if (destPos.z <= centerTrans.position.z)
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
                    // 오른쪽으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerMoveState = MoveState.FR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
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
                if (destPos.z >= centerTrans.position.z)
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
                    // 왼쪽으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerMoveState = MoveState.FL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
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
                if (destPos.z >= centerTrans.position.z)
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
                    // ■
                    // ★■
                    // ↑
                    //--------------------------------
                    // 왼쪽으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerMoveState = MoveState.BL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
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
                if (destPos.z <= centerTrans.position.z)
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
                    // 앞쪽으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerMoveState = MoveState.LL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
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
                if (destPos.x <= centerTrans.position.x)
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
                    // 앞쪽으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerMoveState = MoveState.RR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
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
                if (destPos.x >= centerTrans.position.x)
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
                    // 뒤쪽으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerMoveState = MoveState.LR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
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
                if (destPos.x <= centerTrans.position.x)
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
                    // 뒤쪽으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerMoveState = MoveState.RL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
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
                if (destPos.x >= centerTrans.position.x)
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
                if (destPos.x >= centerTrans.position.x)
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
                if (destPos.x >= centerTrans.position.x)
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
                if (destPos.x <= centerTrans.position.x)
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
                if (destPos.x <= centerTrans.position.x)
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
                if (destPos.z >= centerTrans.position.z)
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
                if (destPos.z >= centerTrans.position.z)
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
                if (destPos.z <= centerTrans.position.z)
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
                if (destPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerMoveState = MoveState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
        }// switch(playerMoveState)

        //Debug.Log("playerMoveState : " + playerMoveState);
        //Debug.Log("moveKeyValue : " + moveKeyValue);
        //Debug.Log(mouseClick);
        //Debug.Log(interactionAnimeStart);
        //Debug.Log(followCam.transform.eulerAngles);
        //Debug.Log("--------------------------------");
        Move(moveKeyValue);
    }

    // 캐릭터 이동
    private void Move(Vector2 moveInput) {
        float targetSpeed = speed * moveInput.magnitude;
        // Normalize 벡터의 크기를 1로 정규화 하는 함수
        // x, z 평면
        //var moveDiection = Vector3.Normalize(transform.forward * moveInput.y + transform.forward * moveInput.x);
        var moveDiection = new Vector3(moveInput.x, 0, moveInput.y);
        // 목표 까지 스무스하게
        //targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime, 2.0f);
        //Debug.Log("targetSpeed : " + targetSpeed);
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
        //Debug.Log("velocity : " + moveDiection );
        //Debug.Log("--------------------------------");
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

    private void UpdateAnimation()
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
            case MoveState.R_INTERACTION_PULL:
            case MoveState.R_INTERACTION_PULL_CLIMBING:
            case MoveState.L_INTERACTION_PULL:
            case MoveState.L_INTERACTION_PULL_CLIMBING:
            case MoveState.F_INTERACTION_PULL:
            case MoveState.F_INTERACTION_PULL_CLIMBING:
            case MoveState.B_INTERACTION_PULL:
            case MoveState.B_INTERACTION_PULL_CLIMBING:
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
            case MoveState.INTERACTION_PUSH:
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

        animationSpeedPercent = currentSpeed / speed;
        animator.SetFloat("Vertical Move", move.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", move.x * animationSpeedPercent, 0.05f, Time.deltaTime);
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

    // 캐릭터 상태를 대기 상태로 변경
    public void UpdateStateToIdle() {
        playerMoveState = MoveState.IDLE;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log(hit.gameObject.layer);
    }
}
