using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

using static CubeMovement;

public class PlayerMovement : MonoBehaviour
{
    //--------------------------------
    // public 변수
    //--------------------------------

    // 캐릭터 스피드
    public float speed;
    // 수직 이동
    public float jumpVelocity;
    // 수직 등반 이동
    public float climbVelocity;
    // 위로 점프할 때 속도 지연시간 값
    public float upSmoothTime;
    // 아래로 점프할 때 속도 지연시간 값
    public float downSmoothTime;
    // 기상할 때 속도 지연시간 값
    public float standSmoothTime;
    // 방향을 바꾸는 스무스 지연시간 값
    public float turnSmoothTime;
    // magnitude 벡터의 길이를 반환
    // 지면상에서의 현제 속도
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude * 3;
    // 플레이어 상태
    public PlayerState playerState { get; private set; }
    // 캐릭터 머리 트랜스폼
    public Transform headTrans;
    // 캐릭터 중심 트랜스폼
    public Transform centerTrans;
    // 캐릭터 발 트랜스폼
    public Transform footTrans;
    // 플레이어 사망 플래그
    public bool isDeath { get; private set; }

    //--------------------------------
    // private 변수
    //--------------------------------

    private AnimationSwitch animeSwitch;                // 애니메이션
    private CharacterController characterController;    // 캐릭터 컨트롤러
    private PlayerInput playerInput;                    // 플레이어 입력 스크립트
    private Animator animator;                          // 애니메이터
    private Camera followCam;                           // 카메라
    private GameManager gameManager;                    // 게임 매니저 스크립트
    private LayerMask layerMaskCube;                    // 큐브 레이어 마스크
    private GameObject cubeObject;                      // 이동할 큐브 오브젝트


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
    private float actionDelay;
    // 캐릭터 스피드 저장
    private float saveSpeed;
    // 떨어졌을 때의 캐릭터 높이
    private float dropPlayerY;
    // 등반 플래그
    private bool climbingFlag;
    // 마우스 클릭
    private bool mouseClick;
    // 머리 충돌 체크 옵션
    private bool checkHeadCollisionOption;
    // 캐릭터 이동 목표 좌표
    private Vector3 destPos;
    // 큐브 이동 목표 좌표
    private Vector3 cubeDestPos;
    // 상하좌우 이동 값
    private Vector2 moveKeyValue;


    //--------------------------------
    // enum
    //--------------------------------

    public enum PlayerState {
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
        MOVE_FLINCH,                // 이동 움찔
        R_MOVE_COLLISION,           // 오른쪽 이동 충돌
        R_MOVE_COLLISION_END,       // 오른쪽 이동 충돌 끝
        L_MOVE_COLLISION,           // 왼쪽 이동 충돌
        L_MOVE_COLLISION_END,       // 왼쪽 이동 충돌 끝
        F_MOVE_COLLISION,           // 앞쪽 이동 충돌
        F_MOVE_COLLISION_END,       // 앞쪽 이동 충돌 끝
        B_MOVE_COLLISION,           // 뒤쪽 이동 충돌
        B_MOVE_COLLISION_END,       // 뒤쪽 이동 충돌 끝
        R_SLIDE,                    // 오른쪽 미끄러짐
        L_SLIDE,                    // 왼쪽 미끄러짐
        F_SLIDE,                    // 앞쪽 미끄러짐
        B_SLIDE,                    // 뒤쪽 미끄러짐
        R_UP,                       // 오른쪽 위
        L_UP,                       // 왼쪽 위
        F_UP,                       // 앞쪽 위
        B_UP,                       // 뒤쪽 위
        UP_FLINCH,                  // 점프 움찔
        R_UP_COLLISION,             // 오른쪽 위 충돌
        R_UP_COLLISION_END,         // 오른쪽 위 충돌 끝
        R_DOWN,                     // 오른쪽 아래
        L_DOWN,                     // 왼쪽 아래
        F_DOWN,                     // 앞쪽 아래
        B_DOWN,                     // 뒤쪽 아래
        R_INTERACTION_PUSH_READY,   // 오른쪽 밀기 준비
        L_INTERACTION_PUSH_READY,   // 왼쪽 밀기 준비
        F_INTERACTION_PUSH_READY,   // 앞쪽 밀기 준비
        B_INTERACTION_PUSH_READY,   // 뒤쪽 밀기 준비
        R_INTERACTION_PUSH,         // 오른쪽 밀기
        L_INTERACTION_PUSH,         // 왼쪽 밀기
        F_INTERACTION_PUSH,         // 앞쪽 밀기
        B_INTERACTION_PUSH,         // 뒤쪽 밀기
        R_INTERACTION_PUSH_END,     // 오른쪽 밀기 끝
        L_INTERACTION_PUSH_END,     // 왼쪽 밀기 끝
        F_INTERACTION_PUSH_END,     // 앞쪽 밀기 끝
        B_INTERACTION_PUSH_END,     // 뒤쪽 밀기 끝
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
        BL_LL_CHANGE_CLIMBING,      // 뒤쪽에서 왼쪽으로 방향 전환
        R_DROP,                     // 오른쪽 떨어짐
        L_DROP,                     // 왼쪽 떨어짐
        F_DROP,                     // 앞쪽 떨어짐
        B_DROP,                     // 뒤쪽 떨어짐
        R_DROP_HIGH,                // 오른쪽 높은 떨어짐
        L_DROP_HIGH,                // 왼쪽 높은 떨어짐
        F_DROP_HIGH,                // 앞쪽 높은 떨어짐
        B_DROP_HIGH,                // 뒤쪽 높은 떨어짐
        R_DROP_LOW,                 // 오른쪽 낮은 떨어짐
        L_DROP_LOW,                 // 왼쪽 낮은 떨어짐
        F_DROP_LOW,                 // 앞쪽 낮은 떨어짐
        B_DROP_LOW,                 // 뒤;쪽 낮은 떨어짐
        R_DROP_CLING,               // 오른쪽 떨어짐 매달림
        L_DROP_CLING,               // 왼쪽 떨어짐 매달림
        F_DROP_CLING,               // 앞쪽 떨어짐 매달림
        B_DROP_CLING,               // 뒤쪽 떨어짐 매달림
        R_DROP_CLIMBING,            // 오른쪽 떨어짐 등반
        L_DROP_CLIMBING,            // 왼쪽 떨어짐 등반
        F_DROP_CLIMBING,            // 앞쪽 떨어짐 등반
        B_DROP_CLIMBING,            // 뒤쪽 떨어짐 등반
        CRUSHED_TO_DEATH,           // 큐브에 깔려 죽음
        DEATH,                      // 사망
        EMPTY
    }

    // 애니메이션 스위치
    private enum AnimationSwitch
    {
        IDLE,
        MOVE_FLINCH,
        MOVE_COLLISION,
        UP_COLLISION,
        UP_COLLISION_END,
        JUMP,
        CLIMBING,
        CLIMBING_END,
        INTERACTION_START,
        INTERACTION_END,
        PUSH_IDLE,
        PUSH,
        PUSH_END,
        SLIDE,
        SLIDE_END,
        DROP,
        DROP_HIGH,
        DROP_LOW,
        DROP_LOW_END,
        DROP_CLIMBING,
        CRUSHED_TO_DEATH,
        DROP_TO_DEATH
    }

    //--------------------------------
    // 상수
    //--------------------------------

    private const float INTERACTION_MOVE_VALUE = 0.25f;
    private const float JUMP_DELAY = 0.15f;
    private const float PUSH_DELAY = 0.5f;
    private const float PUSH_END_DELAY = 0.15f;
    private const float CUBE_HALF_LENGTH = 0.5f;

    //--------------------------------
    // public 함수
    //--------------------------------

    // 캐릭터 상태를 대기 상태로 변경
    public void UpdateStateToIdle()
    {
        playerState = PlayerState.IDLE;
    }


    //--------------------------------
    // private 함수
    //--------------------------------

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

        // 레이어 마스크
        layerMaskCube = 1 << LayerMask.NameToLayer("Cube");
        // 딜레이
        actionDelay = 0f;
        // 플레이어 상태
        playerState = PlayerState.IDLE;
        // 플레이어 사망 플래그
        isDeath = false;
        // 애니메이션
        animeSwitch = AnimationSwitch.IDLE;
        // 머리 충돌 체크 옵션
        checkHeadCollisionOption = false;

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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Floor"))
        {
            switch (playerState)
            {
                case PlayerState.R_DROP:
                case PlayerState.L_DROP:
                case PlayerState.F_DROP:
                case PlayerState.B_DROP:
                case PlayerState.IDLE:
                    // 사망 플래그
                    isDeath = true;
                    playerState = PlayerState.DEATH;
                    animeSwitch = AnimationSwitch.DROP_TO_DEATH;
                    break;
            }
        }
    }

    //--------------------------------------------
    // 플레이어 이동 처리
    // moveInput : 입력받은 이동 키값 -1 ~ 1
    //--------------------------------------------
    private void MoveProcess() {
        float playerTime;           // 거리 / 속도를 한 플레이어의 시간 값
        float cubeTime;             // 거리 / 속도를 한 큐브의 시간 값
        float followCamAngleY;      // 카메라 방향
        Vector2 moveInput;          // 카메라 뱡향에 다라 변화된 키 값
        Vector3 check;              // 체크할 위치
        Vector3 box;                // 박스 크기
        Vector3 moveValue;          // 이동 값
        RaycastHit rayHit;          // 레이 충돌한 물체
        CubeMovement cubeMovement;  // 큐브 스크립트
        

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;
        moveValue.x = 0f;
        moveValue.y = 0f;
        moveValue.z = 0f;
        followCamAngleY = followCam.transform.eulerAngles.y;

        switch (playerState) {
            case PlayerState.R_IDLE_CLIMBING:
            case PlayerState.L_IDLE_CLIMBING:
            case PlayerState.F_IDLE_CLIMBING:
            case PlayerState.B_IDLE_CLIMBING:
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
        switch (playerState)
        {
            case PlayerState.IDLE:
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (mouseClick)
                {
                    moveKeyValue = Vector2.zero;

                    // 캐릭터가 바라보는 방향으로 큐브가 있나?
                    if (Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
                    {
                        //------------------------------------------------
                        // 해당 방향으로 큐브가 있으면 상호작용 상태로
                        //------------------------------------------------
                        // 정면 보고 있음
                        if ((int)transform.eulerAngles.y == 0)
                        {
                            // 앞쪽 상호작용
                            playerState = PlayerState.F_IDLE_INTERACTION;
                            // 애니메이션 상호작용
                            animeSwitch = AnimationSwitch.INTERACTION_START;
                            // 큐브에 바싹 붙음
                            moveValue.z = INTERACTION_MOVE_VALUE;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 오른쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 90)
                        {
                            // 오른쪽 상호작용
                            playerState = PlayerState.R_IDLE_INTERACTION;
                            // 애니메이션 상호작용
                            animeSwitch = AnimationSwitch.INTERACTION_START;
                            // 큐브에 바싹 붙음
                            moveValue.x = INTERACTION_MOVE_VALUE;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 뒤쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 180)
                        {
                            // 뒤쪽 상호작용
                            playerState = PlayerState.B_IDLE_INTERACTION;
                            // 애니메이션 상호작용
                            animeSwitch = AnimationSwitch.INTERACTION_START;
                            // 큐브에 바싹 붙음
                            moveValue.z = -INTERACTION_MOVE_VALUE;
                            characterController.Move(moveValue);
                            break;
                        }
                        // 왼쪽 보고 있음
                        else if ((int)transform.eulerAngles.y == 270)
                        {
                            // 왼쪽 상호작용
                            playerState = PlayerState.L_IDLE_INTERACTION;
                            // 애니메이션 상호작용
                            animeSwitch = AnimationSwitch.INTERACTION_START;
                            // 큐브에 바싹 붙음
                            moveValue.x = -INTERACTION_MOVE_VALUE;
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
                        // 바닥 큐브 정보
                        if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        destPos.x = rayHit.transform.position.x - 1f;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z;

                        // ← 방향 있음
                        if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                            if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                //   ？
                                // ■★
                                //--------------------------------
                                // 위쪽 검사
                                check.x = check.x + 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    //--------------------------------
                                    // 더 위쪽 검사
                                    //   ？
                                    // 
                                    // ■★
                                    //--------------------------------
                                    // 이동 목적지 위쪽에 큐브가 있다
                                    if (Physics.Raycast(check, Vector3.up, out rayHit, 1f, layerMaskCube))
                                    {
                                        cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                        // 위쪽 큐브가 내려오는 중
                                        if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                        {
                                            //------------------------------------
                                            // 위에서 큐브가 떨어져서 행동 불가
                                            //------------------------------------
                                            playerState = PlayerState.UP_FLINCH;
                                            break;
                                        }
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                        // 위쪽 큐브가 내려올 준비
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 왼쪽 이동 등반 상태
                                    playerState = PlayerState.L_UP;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
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
                            // 위쪽 검사
                            // ？
                            //   ★
                            //--------------------------------
                            // 이동 목적지 위쪽에 큐브가 있다
                            if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                            {
                                cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                // 위쪽 큐브가 내려오는 중
                                if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                {
                                    //------------------------------------
                                    // 위에서 큐브가 떨어져서 행동 불가
                                    //------------------------------------
                                    playerState = PlayerState.MOVE_FLINCH;
                                    break;
                                }
                                // 위쪽 큐브가 연쇄적으로 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = (headTrans.position.x - (destPos.x + CUBE_HALF_LENGTH)) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.L_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.left;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                                // 위쪽 큐브가 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = (headTrans.position.x - (destPos.x + CUBE_HALF_LENGTH)) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.L_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.left;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                            }

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
                            if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 빙판 검사
                                //   ★
                                // ■□
                                //--------------------------------
                                // 미끄러짐 검사
                                if (CheckSlide())
                                {
                                    // 빙판임
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 오른쪽 미끄러짐 상태
                                    playerState = PlayerState.L_SLIDE;
                                    // 애니메이션 미끄러짐
                                    animeSwitch = AnimationSwitch.SLIDE;
                                    // 속도 변화
                                    saveSpeed = speed;
                                    speed = speed * 1.5f;
                                }
                                else
                                {
                                    // 빙판이 아님
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 오른쪽 이동 상태
                                    playerState = PlayerState.L_MOVE;
                                }
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
                                if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    // 왼쪽 이동 상태
                                    playerState = PlayerState.L_DOWN;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.left;
                                    // 왼쪽 이동 매달림
                                    playerState = PlayerState.L_CLIMBING;
                                    // 애니메이션 아래 점프
                                    animeSwitch = AnimationSwitch.CLIMBING;
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
                        // 바닥 큐브 정보
                        if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        destPos.x = rayHit.transform.position.x + 1f;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z;

                        // → 방향 있음
                        if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                            if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                // ？
                                // ★■
                                //--------------------------------
                                // 위쪽 검사
                                check.x = check.x - 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    //--------------------------------
                                    // 더 위쪽 검사
                                    // ？
                                    // 
                                    // ★■
                                    //--------------------------------
                                    // 이동 목적지 위쪽에 큐브가 있다
                                    if (Physics.Raycast(check, Vector3.up, out rayHit, 1f, layerMaskCube))
                                    {
                                        cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                        // 위쪽 큐브가 내려오는 중
                                        if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                        {
                                            //------------------------------------
                                            // 위에서 큐브가 떨어져서 행동 불가
                                            //------------------------------------
                                            playerState = PlayerState.UP_FLINCH;
                                            break;
                                        }
                                        // 위쪽 큐브가 연쇄적으로 내려올 준비
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 플레이어 대기 시간을 더함
                                            playerTime = playerTime + JUMP_DELAY;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();
                                            Debug.Log("체인");
                                            Debug.Log("playerTime : " + playerTime + ", cubeTime : " + cubeTime);
                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                //------------------------------------
                                                // 위에서 큐브가 떨어져서 행동 불가
                                                //------------------------------------
                                                // 이동 목적지는 현제 위치
                                                destPos = transform.position;
                                                // 약간 위
                                                destPos.y = destPos.y + 0.5f;
                                                // 오른쪽 이동 등반 상태
                                                playerState = PlayerState.R_UP_COLLISION;
                                                // 애니메이션 점프
                                                animeSwitch = AnimationSwitch.UP_COLLISION;
                                                // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                                actionDelay = 0f;
                                                // 캐릭터 속도 관련 셋팅
                                                saveSpeed = speed;
                                                speed = 0.5f;
                                                // 머리 충돌 체크 옵션
                                                checkHeadCollisionOption = true;
                                                break;
                                            }
                                        }
                                        // 위쪽 큐브가 내려올 준비
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetDownDelayTime();
                                            Debug.Log("체인");
                                            Debug.Log("playerTime : " + playerTime + ", cubeTime : " + cubeTime);
                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    Debug.Log("통과할수있다고?");
                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // 오른쪽 이동 등반 상태
                                    playerState = PlayerState.R_UP;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
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
                            // 위쪽 검사
                            //   ？
                            // ★
                            //--------------------------------
                            // 이동 목적지 위쪽에 큐브가 있다
                            if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                            {
                                cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                // 위쪽 큐브가 내려오는 중
                                if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                {
                                    //------------------------------------
                                    // 위에서 큐브가 떨어져서 행동 불가
                                    //------------------------------------
                                    playerState = PlayerState.MOVE_FLINCH;
                                    break;
                                }
                                // 위쪽 큐브가 연쇄적으로 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = ((destPos.x - CUBE_HALF_LENGTH) - headTrans.position.x) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.R_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.right;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                                // 위쪽 큐브가 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = ((destPos.x - CUBE_HALF_LENGTH) - headTrans.position.x) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.R_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.right;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                            }

                            //--------------------------------
                            // 아래쪽 검사
                            // ★
                            // ■？
                            //--------------------------------
                            // 오른쪽 아래 검사
                            check.x = destPos.x;
                            check.y = destPos.y - 1f;
                            check.z = destPos.z;
                            // 있다
                            if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 빙판 검사
                                // ★
                                // □■
                                //--------------------------------
                                // 미끄러짐 검사
                                if (CheckSlide())
                                {
                                    // 빙판임
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.right;
                                    // 오른쪽 미끄러짐 상태
                                    playerState = PlayerState.R_SLIDE;
                                    // 애니메이션 미끄러짐
                                    animeSwitch = AnimationSwitch.SLIDE;
                                    // 속도 변화
                                    saveSpeed = speed;
                                    speed = speed * 1.5f;
                                }
                                else
                                {
                                    // 빙판이 아님
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.right;
                                    // 오른쪽 이동 상태
                                    playerState = PlayerState.R_MOVE;
                                }
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
                                if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    // 오른쪽 아래로 이동 상태
                                    playerState = PlayerState.R_DOWN;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.right;
                                    // 오른쪽 이동 매달림
                                    playerState = PlayerState.R_CLIMBING;
                                    // 애니메이션 아래 점프
                                    animeSwitch = AnimationSwitch.CLIMBING;
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
                        // 정면 큐브 정보
                        if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        destPos.x = rayHit.transform.position.x;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z - 1f;

                        // ↓ 방향 있음
                        if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                            if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                //   ？
                                // ■★
                                //--------------------------------
                                // 위쪽 검사
                                check.z = check.z + 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    //--------------------------------
                                    // 더 위쪽 검사
                                    //   ？
                                    // 
                                    // ■★
                                    //--------------------------------
                                    // 이동 목적지 위쪽에 큐브가 있다
                                    if (Physics.Raycast(check, Vector3.up, out rayHit, 1f, layerMaskCube))
                                    {
                                        cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                        // 위쪽 큐브가 내려오는 중
                                        if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                        {
                                            //------------------------------------
                                            // 위에서 큐브가 떨어져서 행동 불가
                                            //------------------------------------
                                            playerState = PlayerState.UP_FLINCH;
                                            break;
                                        }
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                        // 위쪽 큐브가 내려올 준비
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 등반 상태
                                    playerState = PlayerState.B_UP;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
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
                            // 위쪽 검사
                            // ？
                            //   ★
                            //--------------------------------
                            // 이동 목적지 위쪽에 큐브가 있다
                            if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                            {
                                cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                // 위쪽 큐브가 내려오는 중
                                if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                {
                                    //------------------------------------
                                    // 위에서 큐브가 떨어져서 행동 불가
                                    //------------------------------------
                                    playerState = PlayerState.MOVE_FLINCH;
                                    break;
                                }
                                // 위쪽 큐브가 연쇄적으로 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = (headTrans.position.z - (destPos.z + CUBE_HALF_LENGTH)) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.B_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.down;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                                // 위쪽 큐브가 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = (headTrans.position.z - (destPos.z + CUBE_HALF_LENGTH)) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.B_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.down;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                            }

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
                            if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 빙판 검사
                                //   ★
                                // ■□
                                //--------------------------------
                                // 미끄러짐 검사
                                if (CheckSlide())
                                {
                                    // 빙판임
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 미끄러짐 상태
                                    playerState = PlayerState.B_SLIDE;
                                    // 애니메이션 미끄러짐
                                    animeSwitch = AnimationSwitch.SLIDE;
                                    // 속도 변화
                                    saveSpeed = speed;
                                    speed = speed * 1.5f;
                                }
                                else
                                {
                                    // 빙판이 아님
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 상태
                                    playerState = PlayerState.B_MOVE;
                                }
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
                                if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    // 뒤쪽 이동 상태
                                    playerState = PlayerState.B_DOWN;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.down;
                                    // 뒤쪽 이동 매달림
                                    playerState = PlayerState.B_CLIMBING;
                                    // 애니메이션 아래 점프
                                    animeSwitch = AnimationSwitch.CLIMBING;
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
                        // 정면 큐브 정보
                        if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                        {
                            // 에러
                            break;
                        }

                        // 목표 이동 위치
                        destPos.x = rayHit.transform.position.x;
                        destPos.y = rayHit.transform.position.y + 1f;
                        destPos.z = rayHit.transform.position.z + 1f;

                        // ↑ 방향 있음
                        if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                        {
                            //--------------------------------
                            // 위쪽 검사
                            //   ？
                            // ★■
                            //--------------------------------
                            // 앞쪽 위 검사
                            check.x = destPos.x;
                            check.y = destPos.y + 1f;
                            check.z = destPos.z;
                            // 없다
                            if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 캐릭터 위쪽 검사
                                // ？
                                // ★■
                                //--------------------------------
                                // 위쪽 검사
                                check.z = check.z - 1f;
                                // 없다
                                if (!Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    //--------------------------------
                                    // 더 위쪽 검사
                                    // ？
                                    // 
                                    // ★■
                                    //--------------------------------
                                    // 이동 목적지 위쪽에 큐브가 있다
                                    if (Physics.Raycast(check, Vector3.up, out rayHit, 1f, layerMaskCube))
                                    {
                                        cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                        // 위쪽 큐브가 내려오는 중
                                        if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                        {
                                            //------------------------------------
                                            // 위에서 큐브가 떨어져서 행동 불가
                                            //------------------------------------
                                            playerState = PlayerState.UP_FLINCH;
                                            break;
                                        }
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                        // 위쪽 큐브가 내려올 준비
                                        else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                        {
                                            //-------------------------------------------------------
                                            // 위에서 큐브가 떨어질 준비를 하고 있음
                                            // 떨어지는 큐브보다 빠르게 지나갈 수 있으면 지나가고
                                            // 지나갈 수 없다면 그에 따른 행동 필요
                                            //-------------------------------------------------------
                                            // 플레이어 거리 / 속도
                                            playerTime = (check.y - footTrans.position.y) / jumpVelocity;
                                            // 큐브 거리 / 속도
                                            cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - check.y) / cubeMovement.verticalSpeed;
                                            // 큐브의 대기 시간을 더함
                                            cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                            // 플레이어가 더 늦게 도착한다면
                                            if (playerTime > cubeTime)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    // 위쪽 이동
                                    destPos.y = destPos.y + 0.5f;
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 등반 상태
                                    playerState = PlayerState.F_UP;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
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
                            // 위쪽 검사
                            //   ？
                            // ★
                            //--------------------------------
                            // 이동 목적지 위쪽에 큐브가 있다
                            if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                            {
                                cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                                // 위쪽 큐브가 내려오는 중
                                if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                                {
                                    //------------------------------------
                                    // 위에서 큐브가 떨어져서 행동 불가
                                    //------------------------------------
                                    playerState = PlayerState.MOVE_FLINCH;
                                    break;
                                }
                                // 위쪽 큐브가 연쇄적으로 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.SHAKE)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = ((destPos.z - CUBE_HALF_LENGTH) - headTrans.position.z) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetChainDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.F_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.up;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                                // 위쪽 큐브가 내려올 준비
                                else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                                {
                                    // 플레이어 거리 / 속도
                                    playerTime = ((destPos.z - CUBE_HALF_LENGTH) - headTrans.position.z) / speed;
                                    // 큐브 거리 / 속도
                                    cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                    // 큐브의 대기 시간을 더함
                                    cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                    // 플레이어가 더 늦게 도착한다면
                                    if (playerTime > cubeTime)
                                    {
                                        //------------------------------------
                                        // 위에서 큐브가 떨어져서 행동 불가
                                        //------------------------------------
                                        playerState = PlayerState.F_MOVE_COLLISION;
                                        // Move 함수에서 처리할 키 값
                                        moveKeyValue = Vector2.up;
                                        // 이동 목적지는 현제 위치
                                        destPos = transform.position;
                                        break;
                                    }
                                }
                            }

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
                            if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                            {
                                //--------------------------------
                                // 빙판 검사
                                // ★
                                // □■
                                //--------------------------------
                                // 미끄러짐 검사
                                if (CheckSlide())
                                {
                                    // 빙판임
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 미끄러짐 상태
                                    playerState = PlayerState.F_SLIDE;
                                    // 애니메이션 미끄러짐
                                    animeSwitch = AnimationSwitch.SLIDE;
                                    // 속도 변화
                                    saveSpeed = speed;
                                    speed = speed * 1.5f;
                                }
                                else
                                {
                                    // 빙판이 아님
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 상태
                                    playerState = PlayerState.F_MOVE;
                                }
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
                                if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                                {
                                    // 앞쪽 이동 상태
                                    playerState = PlayerState.F_DOWN;
                                    // 애니메이션 점프
                                    animeSwitch = AnimationSwitch.JUMP;
                                    // 점프 애니메이션은 약간의 딜레이가 필요합니다
                                    actionDelay = 0f;
                                    // 캐릭터 속도 관련 셋팅
                                    saveSpeed = speed;
                                    speed = 0.5f;
                                }
                                // 없다
                                else
                                {
                                    // Move 함수에서 처리할 키 값
                                    moveKeyValue = Vector2.up;
                                    // 앞쪽 이동 매달림
                                    playerState = PlayerState.F_CLIMBING;
                                    // 애니메이션 아래 점프
                                    animeSwitch = AnimationSwitch.CLIMBING;
                                }
                            }
                        }
                    }
                }
                break;
            case PlayerState.R_IDLE_CLIMBING:
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                        playerState = PlayerState.RL_CLIMBING_MOVE;
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
                        check.x = destPos.x - 1f;
                        check.y = destPos.y;
                        check.z = destPos.z;

                        // ← 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.RL_CLIMBING_MOVE;
                        }
                        // ← 방향 없음
                        else
                        {
                            // ← 방향 이동
                            destPos.x = destPos.x - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.RL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                        playerState = PlayerState.RR_CLIMBING_MOVE;
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
                        check.x = destPos.x - 1f;
                        check.y = destPos.y;
                        check.z = destPos.z;

                        // ← 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.RR_CLIMBING_MOVE;
                        }
                        // ← 방향 없음
                        else
                        {
                            // ← 방향 이동
                            destPos.x = destPos.x - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.RR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 앞쪽 이동 등반 상태
                        playerState = PlayerState.L_UP;
                        // 애니메이션 점프
                        animeSwitch = AnimationSwitch.CLIMBING_END;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                }
                //------------------------------------------------
                // 오른쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 마우스 클릭
                else if (mouseClick)
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.R_DROP;
                    // 애니메이션 떨어짐
                    animeSwitch = AnimationSwitch.DROP;
                    // 매달림 상태 해제
                    climbingFlag = false;
                    // 떨어질 때의 플레이어 높이
                    dropPlayerY = centerTrans.position.y;
                }
                break;
            case PlayerState.L_IDLE_CLIMBING:
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                        playerState = PlayerState.LL_CLIMBING_MOVE;
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
                        check.x = destPos.x + 1f;
                        check.y = destPos.y;
                        check.z = destPos.z;

                        // → 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.LL_CLIMBING_MOVE;
                        }
                        // → 방향 없음
                        else
                        {
                            // → 방향 이동
                            destPos.x = destPos.x + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.up;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.LL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
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
                        playerState = PlayerState.LR_CLIMBING_MOVE;
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
                        check.x = destPos.x + 1f;
                        check.y = destPos.y;
                        check.z = destPos.z;

                        // → 방향 있음
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.LR_CLIMBING_MOVE;
                        }
                        // → 방향 없음
                        else
                        {
                            // → 방향 이동
                            destPos.x = destPos.x + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.down;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.LR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 앞쪽 이동 등반 상태
                        playerState = PlayerState.R_UP;
                        // 애니메이션 점프
                        animeSwitch = AnimationSwitch.CLIMBING_END;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                }
                //------------------------------------------------
                // 왼쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 마우스 클릭
                else if (mouseClick)
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.L_DROP;
                    // 애니메이션 떨어짐
                    animeSwitch = AnimationSwitch.DROP;
                    // 매달림 상태 해제
                    climbingFlag = false;
                    // 떨어질 때의 플레이어 높이
                    dropPlayerY = centerTrans.position.y;
                }
                break;
            case PlayerState.F_IDLE_CLIMBING:
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        // →★■
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerState = PlayerState.FR_CLIMBING_MOVE;
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
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.FR_CLIMBING_MOVE;
                        }
                        // ↓방향 없음
                        else
                        {
                            // ↓ 방향 이동
                            destPos.z = destPos.z - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.FR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                if (moveInput.x >= 0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음 
                        // ■★←
                        //   ■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 왼쪽 이동 매달림
                        playerState = PlayerState.FL_CLIMBING_MOVE;
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
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.FL_CLIMBING_MOVE;
                        }
                        // ↓방향 없음
                        else
                        {
                            // ↓ 방향 이동
                            destPos.z = destPos.z - 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.FL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 앞쪽 이동 등반 상태
                        playerState = PlayerState.B_UP;
                        // 애니메이션 점프
                        animeSwitch = AnimationSwitch.CLIMBING_END;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                }
                //------------------------------------------------
                // 앞쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 마우스 클릭
                else if (mouseClick)
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.F_DROP;
                    // 애니메이션 떨어짐
                    animeSwitch = AnimationSwitch.DROP;
                    // 매달림 상태 해제
                    climbingFlag = false;
                    // 떨어질 때의 플레이어 높이
                    dropPlayerY = centerTrans.position.y;
                }
                break;
            case PlayerState.B_IDLE_CLIMBING:
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 왼쪽 이동 방향에 벽 있음 
                        //   ■
                        // ■★←
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 왼쪽 이동 매달림
                        playerState = PlayerState.BL_CLIMBING_MOVE;
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
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.BL_CLIMBING_MOVE;
                        }
                        // ↑방향 없음
                        else
                        {
                            // ↑ 방향 이동
                            destPos.z = destPos.z + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.left;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.BL_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 →
                if (moveInput.x >= 0.3)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 오른쪽 이동 방향에 벽 있음 
                        //   ■
                        // →★■
                        //--------------------------------
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 왼쪽 이동 매달림
                        playerState = PlayerState.BR_CLIMBING_MOVE;
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
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.BR_CLIMBING_MOVE;
                        }
                        // ↑방향 없음
                        else
                        {
                            // ↑ 방향 이동
                            destPos.z = destPos.z + 1f;
                            // Move 함수에서 처리할 키 값
                            moveKeyValue = Vector2.right;
                            // 왼쪽 이동 매달림
                            playerState = PlayerState.BR_CLIMBING_MOVE;
                        }
                    }
                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                else if (moveInput.y > 0)
                {
                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube)) {
                        // 실제 이동 높이
                        destPos.y = destPos.y - 0.5f;
                        // 매달림 상태 해제
                        climbingFlag = false;
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 앞쪽 이동 등반 상태
                        playerState = PlayerState.F_UP;
                        // 애니메이션 점프
                        animeSwitch = AnimationSwitch.CLIMBING_END;
                        // 점프 애니메이션은 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                }
                //------------------------------------------------
                // 뒤쪽 매달림 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 마우스 클릭
                else if (mouseClick)
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.B_DROP;
                    // 애니메이션 떨어짐
                    animeSwitch = AnimationSwitch.DROP;
                    // 매달림 상태 해제
                    climbingFlag = false;
                    // 떨어질 때의 플레이어 높이
                    dropPlayerY = centerTrans.position.y;
                }
                break;
            case PlayerState.R_IDLE_INTERACTION:
                // 오른쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick) {
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 종료
                    animeSwitch = AnimationSwitch.INTERACTION_END;
                    // 원 위치로 돌아감
                    moveValue.x = -INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3) {
                    // 당김

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube)) {
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

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 위쪽 검사
                        // ？
                        //   ★■
                        //--------------------------------
                        // 이동 목적지 위쪽에 큐브가 있다
                        if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                        {
                            cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                            // 위쪽 큐브가 내려오는 중
                            if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                            {
                                break;
                            }
                            // 위쪽 큐브가 내려올 준비
                            else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                            {
                                // 플레이어 거리 / 속도
                                playerTime = (headTrans.position.x - (destPos.x + CUBE_HALF_LENGTH)) / speed;
                                // 큐브 거리 / 속도
                                cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                // 큐브의 대기 시간을 더함
                                cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                // 플레이어가 더 늦게 도착한다면
                                if (playerTime > cubeTime)
                                {
                                    break;
                                }
                            }
                        }

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

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // 오른쪽 상호작용 당김
                            playerState = PlayerState.R_INTERACTION_PULL;
                            // 큐브 왼쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullLeft(speed);
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 오른쪽 상호작용 당김
                            playerState = PlayerState.R_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            destPos.y = destPos.y - 1f;
                            // 큐브 왼쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullLeft(speed);
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                    }
                }
                // 입력 키 값 →
                else if (moveInput.x >= 0.3) {
                    // 밀기

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;
                    // 오른쪽 상호작용 밀기 준비
                    playerState = PlayerState.R_INTERACTION_PUSH_READY;
                    // 애니메이션 밀기 대기
                    animeSwitch = AnimationSwitch.PUSH_IDLE;
                }
                break;
            case PlayerState.L_IDLE_INTERACTION:
                // 왼쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 종료
                    animeSwitch = AnimationSwitch.INTERACTION_END;
                    // 원 위치로 돌아감
                    moveValue.x = INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 밀기

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;
                    // 왼쪽 상호작용 밀기 준비
                    playerState = PlayerState.L_INTERACTION_PUSH_READY;
                    // 애니메이션 밀기 대기
                    animeSwitch = AnimationSwitch.PUSH_IDLE;
                }
                // 입력 키 값 →
                else if (moveInput.x >= 0.3)
                {
                    // 당김

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 위쪽 검사
                        //     ？
                        // ■★
                        //--------------------------------
                        // 이동 목적지 위쪽에 큐브가 있다
                        if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                        {
                            cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                            // 위쪽 큐브가 내려오는 중
                            if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                            {
                                break;
                            }
                            // 위쪽 큐브가 내려올 준비
                            else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                            {
                                // 플레이어 거리 / 속도
                                playerTime = ((destPos.x - CUBE_HALF_LENGTH) - headTrans.position.x) / speed;
                                // 큐브 거리 / 속도
                                cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                // 큐브의 대기 시간을 더함
                                cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                // 플레이어가 더 늦게 도착한다면
                                if (playerTime > cubeTime)
                                {
                                    break;
                                }
                            }
                        }

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

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // 왼쪽 상호작용 당김
                            playerState = PlayerState.L_INTERACTION_PULL;
                            // 큐브 오른쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullRight(speed);
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 왼쪽 상호작용 당기고 매달림
                            playerState = PlayerState.L_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            destPos.y = destPos.y - 1f;
                            // 큐브 오른쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullRight(speed);
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                    }
                }
                break;
            case PlayerState.F_IDLE_INTERACTION:
                // 앞쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 종료
                    animeSwitch = AnimationSwitch.INTERACTION_END;
                    // 원 위치로 돌아감
                    moveValue.z = -INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                if (moveInput.y >= 0.3)
                {
                    // 밀기

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;
                    // 앞쪽 상호작용 밀기 준비
                    playerState = PlayerState.F_INTERACTION_PUSH_READY;
                    // 애니메이션 밀기 대기
                    animeSwitch = AnimationSwitch.PUSH_IDLE;
                }
                // 입력 키 값 ↓
                else if (moveInput.y <= -0.3)
                {
                    // 당김

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 위쪽 검사
                        // ？
                        //   ★■
                        //--------------------------------
                        // 이동 목적지 위쪽에 큐브가 있다
                        if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                        {
                            cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                            // 위쪽 큐브가 내려오는 중
                            if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                            {
                                break;
                            }
                            // 위쪽 큐브가 내려올 준비
                            else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                            {
                                // 플레이어 거리 / 속도
                                playerTime = (headTrans.position.z - (destPos.z + CUBE_HALF_LENGTH)) / speed;
                                // 큐브 거리 / 속도
                                cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                // 큐브의 대기 시간을 더함
                                cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                // 플레이어가 더 늦게 도착한다면
                                if (playerTime > cubeTime)
                                {
                                    break;
                                }
                            }
                        }

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
                        cubeDestPos.x = destPos.x;
                        cubeDestPos.y = destPos.y;
                        cubeDestPos.z = destPos.z + 1f;

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // 앞쪽 상호작용 당김
                            playerState = PlayerState.F_INTERACTION_PULL;
                            // 큐브 뒤쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullBack(speed);
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 앞쪽 상호작용 당기고 매달림
                            playerState = PlayerState.F_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            destPos.y = destPos.y - 1f;
                            // 큐브 뒤쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullBack(speed);
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                    }
                }
                break;
            case PlayerState.B_IDLE_INTERACTION:
                // 뒤쪽 상호작용 대기
                //------------------------------------------------
                // 기본 대기 상태일 때의 마우스 클릭 입력 처리
                //------------------------------------------------
                if (!mouseClick)
                {
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 종료
                    animeSwitch = AnimationSwitch.INTERACTION_END;
                    // 원 위치로 돌아감
                    moveValue.z = INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }

                //------------------------------------------------
                // 상호작용 대기 상태 키처리
                //------------------------------------------------
                // 입력 키 값 ↑
                if (moveInput.y >= 0.3)
                {
                    // 당김

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
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

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;

                    // 없다
                    if (!Physics.CheckBox(destPos, box, Quaternion.identity, layerMaskCube))
                    {
                        //--------------------------------
                        // 위쪽 검사
                        //     ？
                        // ■★
                        //--------------------------------
                        // 이동 목적지 위쪽에 큐브가 있다
                        if (Physics.Raycast(destPos, Vector3.up, out rayHit, 1f, layerMaskCube))
                        {
                            cubeMovement = rayHit.transform.GetComponent<CubeMovement>();

                            // 위쪽 큐브가 내려오는 중
                            if (cubeMovement.cubeMoveState == CubeMoveState.DOWN)
                            {
                                break;
                            }
                            // 위쪽 큐브가 내려올 준비
                            else if (cubeMovement.cubeMoveState == CubeMoveState.DOWN_READY)
                            {
                                // 플레이어 거리 / 속도
                                playerTime = ((destPos.z - CUBE_HALF_LENGTH) - headTrans.position.z) / speed;
                                // 큐브 거리 / 속도
                                cubeTime = ((rayHit.transform.position.y - CUBE_HALF_LENGTH) - headTrans.position.y) / cubeMovement.verticalSpeed;
                                // 큐브의 대기 시간을 더함
                                cubeTime = cubeTime + cubeMovement.GetDownDelayTime();

                                // 플레이어가 더 늦게 도착한다면
                                if (playerTime > cubeTime)
                                {
                                    break;
                                }
                            }
                        }

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

                        // 있다
                        // 당김
                        if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                        {
                            // 뒤쪽 상호작용 당김
                            playerState = PlayerState.B_INTERACTION_PULL;
                            // 큐브 앞쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullForward(speed);
                        }
                        // 없다
                        // 당기고 매달림
                        else
                        {
                            // 앞쪽 상호작용 당기고 매달림
                            playerState = PlayerState.B_INTERACTION_PULL_CLIMBING;
                            // 아래쪽에 매달림
                            destPos.y = destPos.y - 1f;
                            // 큐브 앞쪽으로 당겨짐
                            cubeObject.GetComponent<CubeMovement>().PullForward(speed);
                        }

                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                    }
                }
                // 입력 키 값 ↓
                else if (moveInput.y <= -0.3)
                {
                    // 밀기

                    // 정면 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, transform.forward, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 이동할 큐브 오브젝트
                    cubeObject = rayHit.transform.gameObject;
                    // 뒤쪽 상호작용 밀기 준비
                    playerState = PlayerState.B_INTERACTION_PUSH_READY;
                    // 애니메이션 밀기 대기
                    animeSwitch = AnimationSwitch.PUSH_IDLE;
                }
                break;
            case PlayerState.R_MOVE:
                // 오른쪽 이동

                // 미끄러짐 검사
                if (CheckSlide())
                {
                    // 오른쪽 미끄러짐 상태
                    playerState = PlayerState.R_SLIDE;
                    // 애니메이션 미끄러짐
                    animeSwitch = AnimationSwitch.SLIDE;
                    // 속도 변화
                    saveSpeed = speed;
                    speed = speed * 1.5f;
                    break;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.IDLE;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_MOVE:
                // 왼쪽 이동

                // 미끄러짐 검사
                if (CheckSlide())
                {
                    // 왼쪽 미끄러짐 상태
                    playerState = PlayerState.L_SLIDE;
                    // 애니메이션 미끄러짐
                    animeSwitch = AnimationSwitch.SLIDE;
                    // 속도 변화
                    saveSpeed = speed;
                    speed = speed * 1.5f;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.IDLE;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_MOVE:
                // 앞쪽 이동

                // 미끄러짐 검사
                if (CheckSlide())
                {
                    // 앞쪽 미끄러짐 상태
                    playerState = PlayerState.F_SLIDE;
                    // 애니메이션 미끄러짐
                    animeSwitch = AnimationSwitch.SLIDE;
                    // 속도 변화
                    saveSpeed = speed;
                    speed = speed * 1.5f;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.IDLE;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_MOVE:
                // 뒤쪽 이동

                // 미끄러짐 검사
                if (CheckSlide())
                {
                    // 뒤쪽 미끄러짐 상태
                    playerState = PlayerState.B_SLIDE;
                    // 애니메이션 미끄러짐
                    animeSwitch = AnimationSwitch.SLIDE;
                    // 속도 변화
                    saveSpeed = speed;
                    speed = speed * 1.5f;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 멈춤
                    moveKeyValue = Vector2.zero;
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.IDLE;
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.MOVE_FLINCH:
                // 이동 움찔

                // 애니메이션이 끝날 때까지 기다림
                playerState = playerState = PlayerState.EMPTY;
                // 애니메이션 이동 움찔
                animeSwitch = AnimationSwitch.MOVE_FLINCH;
                break;
            case PlayerState.R_MOVE_COLLISION:
                // 오른쪽 이동 충돌

                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 충돌 끝
                    playerState = PlayerState.R_MOVE_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.MOVE_COLLISION;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 캐릭터 속도 관련 셋팅
                    saveSpeed = speed;
                    speed = 0.5f;
                }
                break;
            case PlayerState.R_MOVE_COLLISION_END:
                // 오른쪽 이동 충돌 끝
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.left;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_MOVE_COLLISION:
                // 왼쪽 이동 충돌

                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 충돌 끝
                    playerState = PlayerState.L_MOVE_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.MOVE_COLLISION;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 캐릭터 속도 관련 셋팅
                    saveSpeed = speed;
                    speed = 0.5f;
                }
                break;
            case PlayerState.L_MOVE_COLLISION_END:
                // 왼쪽 이동 충돌 끝
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.right;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_MOVE_COLLISION:
                // 앞쪽 이동 충돌

                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 충돌 끝
                    playerState = PlayerState.F_MOVE_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.MOVE_COLLISION;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 캐릭터 속도 관련 셋팅
                    saveSpeed = speed;
                    speed = 0.5f;
                }
                break;
            case PlayerState.F_MOVE_COLLISION_END:
                // 앞쪽 이동 충돌 끝
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.down;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_MOVE_COLLISION:
                // 뒤쪽 이동 충돌

                // 이동하는 중인데 벽에 부딪힘
                if ((currentSpeed / speed) == 0)
                {
                    // 충돌 끝
                    playerState = PlayerState.B_MOVE_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.MOVE_COLLISION;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 캐릭터 속도 관련 셋팅
                    saveSpeed = speed;
                    speed = 0.5f;
                }
                break;
            case PlayerState.B_MOVE_COLLISION_END:
                // 뒤쪽 이동 충돌 끝
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.up;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.R_SLIDE:
                // 오른쪽 미끄러짐

                // 이동중에 미끄러짐 경계선 검사
                if (!CheckSlide())
                {
                    // 미끄러지지 않음 오른쪽 이동
                    playerState = PlayerState.R_MOVE;
                    // 애니메이션 미끄러짐 끝
                    animeSwitch = AnimationSwitch.SLIDE_END;
                    // 속도 복구
                    speed = saveSpeed;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 연속적인 미끄러짐 검사
                    if (!CheckSlide(Vector3.right))
                    {
                        // 멈춤
                        moveKeyValue = Vector2.zero;
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerState = PlayerState.IDLE;
                        // 애니메이션 미끄러짐 끝
                        animeSwitch = AnimationSwitch.SLIDE_END;
                        // 속도 복구
                        speed = saveSpeed;
                        // 플레이어 위치 맞추기
                        moveValue.x = destPos.x - transform.position.x;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.L_SLIDE:
                // 왼쪽 미끄러짐

                // 이동중에 미끄러짐 경계선 검사
                if (!CheckSlide())
                {
                    // 미끄러지지 않음 왼쪽 이동
                    playerState = PlayerState.L_MOVE;
                    // 애니메이션 미끄러짐 끝
                    animeSwitch = AnimationSwitch.SLIDE_END;
                    // 속도 복구
                    speed = saveSpeed;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 연속적인 미끄러짐 검사
                    if (!CheckSlide(Vector3.left))
                    {
                        // 멈춤
                        moveKeyValue = Vector2.zero;
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerState = PlayerState.IDLE;
                        // 애니메이션 미끄러짐 끝
                        animeSwitch = AnimationSwitch.SLIDE_END;
                        // 속도 복구
                        speed = saveSpeed;
                        // 플레이어 위치 맞추기
                        moveValue.x = destPos.x - transform.position.x;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.F_SLIDE:
                // 앞쪽 미끄러짐

                // 이동중에 미끄러짐 경계선 검사
                if (!CheckSlide())
                {
                    // 미끄러지지 않음 앞쪽 이동
                    playerState = PlayerState.F_MOVE;
                    // 애니메이션 미끄러짐 끝
                    animeSwitch = AnimationSwitch.SLIDE_END;
                    // 속도 복구
                    speed = saveSpeed;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 연속적인 미끄러짐 검사
                    if (!CheckSlide(Vector3.forward))
                    {
                        // 멈춤
                        moveKeyValue = Vector2.zero;
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerState = PlayerState.IDLE;
                        // 애니메이션 미끄러짐 끝
                        animeSwitch = AnimationSwitch.SLIDE_END;
                        // 속도 복구
                        speed = saveSpeed;
                        // 플레이어 위치 맞추기
                        moveValue.z = destPos.z - transform.position.z;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.B_SLIDE:
                // 뒤쪽 미끄러짐

                // 이동중에 미끄러짐 경계선 검사
                if (!CheckSlide())
                {
                    // 미끄러지지 않음 뒤쪽 이동
                    playerState = PlayerState.B_MOVE;
                    // 애니메이션 미끄러짐 끝
                    animeSwitch = AnimationSwitch.SLIDE_END;
                    // 속도 복구
                    speed = saveSpeed;
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 연속적인 미끄러짐 검사
                    if (!CheckSlide(Vector3.back))
                    {
                        // 멈춤
                        moveKeyValue = Vector2.zero;
                        // 이동을 끝마쳤으니 상태를 대기로 변경
                        playerState = PlayerState.IDLE;
                        // 애니메이션 미끄러짐 끝
                        animeSwitch = AnimationSwitch.SLIDE_END;
                        // 속도 복구
                        speed = saveSpeed;
                        // 플레이어 위치 맞추기
                        moveValue.z = destPos.z - transform.position.z;
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.R_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
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
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, upSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, upSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // 테스트
                    characterController.center = characterController.center + Vector3.up;
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.left;
                }

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, upSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, upSmoothTime);
                    // 수직 이동하지 않음
                    currentVelocityY = 0f;
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                    // 테스트
                    characterController.center = new Vector3(0f, 0.7f, 0f);
                }
                break;
            case PlayerState.F_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
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
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, upSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, upSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_UP:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
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
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, upSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, upSmoothTime);
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.UP_FLINCH:
                // 점프 움찔
                // 지금은 MOVE_FLINCH와 동일

                // 애니메이션이 끝날 때까지 기다림
                playerState = playerState = PlayerState.EMPTY;
                // 애니메이션 이동 움찔
                animeSwitch = AnimationSwitch.MOVE_FLINCH;
                break;
            case PlayerState.R_UP_COLLISION:
                // 점프 충돌
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.right;
                }
                
                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, jumpVelocity, ref jumpSmoothVertical, upSmoothTime);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed * 1.5f, ref jumpSmoothHorizontal, upSmoothTime);
                }

                if (!checkHeadCollisionOption)
                {
                    Debug.Log("부딪힘");
                    // 충돌 끝
                    playerState = PlayerState.R_UP_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.UP_COLLISION_END;
                    // 밀려남
                    currentVelocityY = -jumpVelocity * 3f;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                /*
                // 이동하는 중인데 벽에 부딪힘
                else if ((currentSpeed / speed) == 0)
                {
                    // 충돌 끝
                    playerState = PlayerState.R_UP_COLLISION_END;
                    // 애니메이션 이동 충돌
                    animeSwitch = AnimationSwitch.UP_COLLISION_END;
                    // 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                }
                */

                /*
                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                */
                break;
            case PlayerState.R_UP_COLLISION_END:
                // 점프 충돌 끝

                break;
            case PlayerState.R_DOWN:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.right;
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                // 처음에는 느리게, 서서히 원래의 이동속도로 수평 이동함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, downSmoothTime);
                }
                break;
            case PlayerState.L_DOWN:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.left;
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                // 처음에는 느리게, 서서히 원래의 이동속도로 수평 이동함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, downSmoothTime);
                }
                break;
            case PlayerState.F_DOWN:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.up;
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                // 처음에는 느리게, 서서히 원래의 이동속도로 수평 이동함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, downSmoothTime);
                }
                break;
            case PlayerState.B_DOWN:
                actionDelay = actionDelay + Time.deltaTime;

                // 점프 준비 동작때문에 약 0.15 대기합니다
                if (actionDelay < JUMP_DELAY)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.down;
                }

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                // 처음에는 느리게, 서서히 원래의 이동속도로 수평 이동함
                else
                {
                    // 캐릭터 이동 속도를 빠르게
                    speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, downSmoothTime);
                }
                break;
            case PlayerState.R_INTERACTION_PUSH_READY:
                // 오른쪽 밀기 준비

                // 오른쪽 밀기 상태
                playerState = PlayerState.R_INTERACTION_PUSH;
                // 애니메이션 밀기
                animeSwitch = AnimationSwitch.PUSH;
                // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                actionDelay = 0f;
                break;
            case PlayerState.L_INTERACTION_PUSH_READY:
                // 왼쪽 밀기 준비

                // 왼쪽 밀기 상태
                playerState = PlayerState.L_INTERACTION_PUSH;
                // 애니메이션 밀기
                animeSwitch = AnimationSwitch.PUSH;
                // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                actionDelay = 0f;
                break;
            case PlayerState.F_INTERACTION_PUSH_READY:
                // 앞쪽 밀기 준비

                // 앞쪽 밀기 상태
                playerState = PlayerState.F_INTERACTION_PUSH;
                // 애니메이션 밀기
                animeSwitch = AnimationSwitch.PUSH;
                // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                actionDelay = 0f;
                break;
            case PlayerState.B_INTERACTION_PUSH_READY:
                // 뒤쪽 밀기 준비

                // 뒤쪽 밀기 상태
                playerState = PlayerState.B_INTERACTION_PUSH;
                // 애니메이션 밀기
                animeSwitch = AnimationSwitch.PUSH;
                // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                actionDelay = 0f;
                break;
            case PlayerState.R_INTERACTION_PUSH:
                // 오른쪽 밀기

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_DELAY)
                {
                    break;
                }
                else
                {
                    // 밀기 상태
                    playerState = PlayerState.R_INTERACTION_PUSH_END;
                    // 큐브 오른쪽 이동 처리
                    cubeObject.GetComponent<CubeMovement>().MoveRight();
                    // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                }
                break;
            case PlayerState.L_INTERACTION_PUSH:
                // 왼쪽 밀기

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_DELAY)
                {
                    break;
                }
                else
                {
                    // 밀기 상태
                    playerState = PlayerState.L_INTERACTION_PUSH_END;
                    // 큐브 오른쪽 이동 처리
                    cubeObject.GetComponent<CubeMovement>().MoveLeft();
                    // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                }
                break;
            case PlayerState.F_INTERACTION_PUSH:
                // 앞쪽 밀기

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_DELAY)
                {
                    break;
                }
                else
                {
                    // 밀기 상태
                    playerState = PlayerState.F_INTERACTION_PUSH_END;
                    // 큐브 오른쪽 이동 처리
                    cubeObject.GetComponent<CubeMovement>().MoveForward();
                    // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                }
                break;
            case PlayerState.B_INTERACTION_PUSH:
                // 뒤쪽 밀기

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_DELAY)
                {
                    break;
                }
                else
                {
                    // 밀기 상태
                    playerState = PlayerState.B_INTERACTION_PUSH_END;
                    // 큐브 오른쪽 이동 처리
                    cubeObject.GetComponent<CubeMovement>().MoveBack();
                    // 밀기 애니메이션은 약간의 딜레이가 필요합니다
                    actionDelay = 0f;
                }
                break;
            case PlayerState.R_INTERACTION_PUSH_END:
                // 오른쪽 밀기 끝

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_END_DELAY)
                {
                    break;
                }
                else
                {
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 밀기 끝
                    animeSwitch = AnimationSwitch.PUSH_END;
                    // 원 위치로 돌아감
                    moveValue.x = -INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_INTERACTION_PUSH_END:
                // 왼쪽 밀기 끝

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_END_DELAY)
                {
                    break;
                }
                else
                {
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 밀기 끝
                    animeSwitch = AnimationSwitch.PUSH_END;
                    // 원 위치로 돌아감
                    moveValue.x = INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_INTERACTION_PUSH_END:
                // 앞쪽 밀기 끝

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_END_DELAY)
                {
                    break;
                }
                else
                {
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 밀기 끝
                    animeSwitch = AnimationSwitch.PUSH_END;
                    // 원 위치로 돌아감
                    moveValue.z = -INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_INTERACTION_PUSH_END:
                // 뒤쪽 밀기 끝

                actionDelay = actionDelay + Time.deltaTime;

                if (actionDelay < PUSH_END_DELAY)
                {
                    break;
                }
                else
                {
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 밀기 끝
                    animeSwitch = AnimationSwitch.PUSH_END;
                    // 원 위치로 돌아감
                    moveValue.z = INTERACTION_MOVE_VALUE;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.R_INTERACTION_PULL:
                // 오른쪽 당김
                
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x >= cubeObject.transform.position.x)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerState = PlayerState.R_IDLE_INTERACTION;
                        // 플레이어 위치 맞추기
                        moveValue.x = moveValue.x + INTERACTION_MOVE_VALUE;
                        characterController.Move(moveValue);
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerState = PlayerState.IDLE;
                        // 애니메이션 종료
                        animeSwitch = AnimationSwitch.INTERACTION_END;
                        // 원 위치로 돌아감
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.R_INTERACTION_PULL_CLIMBING:
                // 오른쪽 당기고 매달림
                
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x >= cubeObject.transform.position.x)
                {
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        animeSwitch = AnimationSwitch.CLIMBING;
                        // 상태 변경
                        playerState = PlayerState.L_CLIMBING;
                    }
                }
                break;
            case PlayerState.L_INTERACTION_PULL:
                // 왼쪽 당김

                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x <= cubeObject.transform.position.x)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerState = PlayerState.L_IDLE_INTERACTION;
                        // 플레이어 위치 맞추기
                        moveValue.x = moveValue.x - INTERACTION_MOVE_VALUE;
                        characterController.Move(moveValue);
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerState = PlayerState.IDLE;
                        // 애니메이션 종료
                        animeSwitch = AnimationSwitch.INTERACTION_END;
                        // 원 위치로 돌아감
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.L_INTERACTION_PULL_CLIMBING:
                // 왼쪽 당기고 매달림
                
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.x <= cubeObject.transform.position.x)
                {
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        animeSwitch = AnimationSwitch.CLIMBING;
                        // 상태 변경
                        playerState = PlayerState.R_CLIMBING;
                    }
                }
                break;
            case PlayerState.F_INTERACTION_PULL:
                // 앞쪽 당김
                
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z >= cubeObject.transform.position.z)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerState = PlayerState.F_IDLE_INTERACTION;
                        // 플레이어 위치 맞추기
                        moveValue.z = moveValue.z + INTERACTION_MOVE_VALUE;
                        characterController.Move(moveValue);
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerState = PlayerState.IDLE;
                        // 애니메이션 종료
                        animeSwitch = AnimationSwitch.INTERACTION_END;
                        // 원 위치로 돌아감
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.F_INTERACTION_PULL_CLIMBING:
                // 앞쪽 당기고 매달림
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z >= cubeObject.transform.position.z)
                {
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        animeSwitch = AnimationSwitch.CLIMBING;
                        // 상태 변경
                        playerState = PlayerState.B_CLIMBING;
                    }
                }
                break;
            case PlayerState.B_INTERACTION_PULL:
                // 뒤쪽 당김

                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z <= cubeObject.transform.position.z)
                {
                    // 이동 완료
                    moveKeyValue = Vector2.zero;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;

                    // 마우스를 계속 클릭 중이라면
                    if (mouseClick)
                    {
                        // 마우스 클릭 중
                        // 상호작용 대기 상태
                        playerState = PlayerState.B_IDLE_INTERACTION;
                        // 플레이어 위치 맞추기
                        moveValue.z = moveValue.z - INTERACTION_MOVE_VALUE;
                        characterController.Move(moveValue);
                    }
                    else
                    {
                        // 마우스 클릭 중이 아님
                        // 대기 상태
                        playerState = PlayerState.IDLE;
                        // 애니메이션 종료
                        animeSwitch = AnimationSwitch.INTERACTION_END;
                        // 원 위치로 돌아감
                        characterController.Move(moveValue);
                    }
                }
                break;
            case PlayerState.B_INTERACTION_PULL_CLIMBING:
                // 뒤쪽 당기고 매달림
                
                // 큐브가 이동 거리만큼 이동 했는가
                if (cubeDestPos.z <= cubeObject.transform.position.z)
                {
                    // 바닥에 닿아있지 않음
                    if (!characterController.isGrounded)
                    {
                        // x,z 이동 멈춤
                        moveKeyValue = Vector2.zero;
                        // 애니메이션
                        animeSwitch = AnimationSwitch.CLIMBING;
                        // 상태 변경
                        playerState = PlayerState.F_CLIMBING;
                    }
                }
                break;
            case PlayerState.R_CLIMBING:
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
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.L_CLIMBING:
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
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.F_CLIMBING:
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
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.B_CLIMBING:
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
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.RR_CLIMBING_MOVE:
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
                    playerState = PlayerState.BR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ←★
                    // ■↑
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerState = PlayerState.RR_FL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.RL_CLIMBING_MOVE:
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
                    playerState = PlayerState.FR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 왼쪽 이동중에 없음
                    // ■↓
                    // ←★
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.left;
                    // 이동 상태 변경
                    playerState = PlayerState.RL_BL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.LR_CLIMBING_MOVE:
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
                    playerState = PlayerState.FL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ↓■
                    // ★→
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerState = PlayerState.LR_BR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.LL_CLIMBING_MOVE:
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
                    playerState = PlayerState.BL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 왼쪽 이동중에 없음
                    // ★→
                    // ↑■
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.right;
                    // 이동 상태 변경
                    playerState = PlayerState.LL_FR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.FR_CLIMBING_MOVE:
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
                    playerState = PlayerState.LL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // →★
                    // ■↓
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerState = PlayerState.FR_RL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.FL_CLIMBING_MOVE:
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
                    playerState = PlayerState.RR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ★←
                    // ↓■
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.down;
                    // 이동 상태 변경
                    playerState = PlayerState.FL_LR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.BR_CLIMBING_MOVE:
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
                    playerState = PlayerState.LR_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ■↑
                    // →★
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerState = PlayerState.BR_RR_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.BL_CLIMBING_MOVE:
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
                    playerState = PlayerState.RL_CLIMBING_MOVE;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 270, 0);
                }

                // 앞쪽에 없음
                if (!Physics.Raycast(centerTrans.position, transform.forward, 1f, layerMaskCube))
                {
                    //--------------------------------
                    // 오른쪽 이동중에 없음
                    // ↑■
                    // ★←
                    //--------------------------------
                    // 앞으로 이동
                    moveKeyValue = Vector2.up;
                    // 이동 상태 변경
                    playerState = PlayerState.BL_LL_CHANGE_CLIMBING;
                    // 방향 바꿈
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }

                // 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 이동을 끝마쳤으니 상태를 대기로 변경
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.RR_FL_CHANGE_CLIMBING:
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
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.RL_BL_CHANGE_CLIMBING:
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
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.LR_BR_CHANGE_CLIMBING:
                //------------------------------
                // 왼쪽에서 뒤쪽으로 방향 전환
                //------------------------------
                // 이동하는 중인데 뭔가 거치적거려서 멈춤
                if ((currentSpeed / speed) == 0)
                {
                    //--------------------------------
                    // 오른쪽 이동중에 뭔가 거치적거려서 멈춤
                    //     ■
                    // →★
                    //--------------------------------
                    moveKeyValue = Vector2.down;
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
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.LL_FR_CHANGE_CLIMBING:
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
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.FR_RL_CHANGE_CLIMBING:
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
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.FL_LR_CHANGE_CLIMBING:
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
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.BR_RR_CHANGE_CLIMBING:
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
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.BL_LL_CHANGE_CLIMBING:
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
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.R_DROP:
                //------------------------------------------------
                // 오른쪽 떨어짐 상태 키처리
                //------------------------------------------------

                // 바닥에 닿아있다면
                if (characterController.isGrounded)
                {
                    // 바닥 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 높은 높이에서 떨어짐
                    if (dropPlayerY - centerTrans.position.y > 2f)
                    {
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 높은 곳에서 떨어짐
                        playerState = PlayerState.R_DROP_HIGH;
                        // 애니메이션 높은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_HIGH;
                        // 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    // 낮은 높이에서 떨어짐
                    else
                    {
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.right;
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 낮은 곳에서 떨어짐
                        playerState = PlayerState.R_DROP_LOW;
                        // 애니메이션 낮은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_LOW;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    break;
                }


                // 입력 키 값 ←
                if (moveInput.x <= -0.3)
                {
                    // 발 기준 왼쪽
                    if (!Physics.Raycast(footTrans.position, Vector3.left, out rayHit, 1f, layerMaskCube))
                    {
                        //없다
                        break;
                    }

                    //--------------------------------
                    // 위쪽 검사
                    // ？
                    // ■★
                    //--------------------------------
                    check = rayHit.transform.position;
                    check.y = check.y + 1f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    //--------------------------------
                    // 아래쪽 검사
                    // ■★
                    //   ？
                    //--------------------------------
                    check.x = check.x + 1f;
                    check.y = check.y - 2f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    // 이동 좌표
                    destPos.x = check.x;
                    destPos.y = check.y + 1f;
                    destPos.z = check.z;
                    // 오른쪽 매달림
                    playerState = PlayerState.R_DROP_CLING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.DROP_CLIMBING;
                }
                break;
            case PlayerState.L_DROP:
                //------------------------------------------------
                // 왼쪽 떨어짐 상태 키처리
                //------------------------------------------------

                // 바닥에 닿아있다면
                if (characterController.isGrounded)
                {
                    // 바닥 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 높은 높이에서 떨어짐
                    if (dropPlayerY - centerTrans.position.y > 2f)
                    {
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 높은 곳에서 떨어짐
                        playerState = PlayerState.L_DROP_HIGH;
                        // 애니메이션 높은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_HIGH;
                        // 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    // 낮은 높이에서 떨어짐
                    else
                    {
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.left;
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 낮은 곳에서 떨어짐
                        playerState = PlayerState.L_DROP_LOW;
                        // 애니메이션 낮은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_LOW;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    break;
                }

                // 입력 키 값 →
                if (moveInput.x >= 0.3)
                {
                    // 발 기준 왼쪽
                    if (!Physics.Raycast(footTrans.position, Vector3.right, out rayHit, 1f, layerMaskCube))
                    {
                        //없다
                        break;
                    }

                    //--------------------------------
                    // 위쪽 검사
                    //   ？
                    // ★■
                    //--------------------------------
                    check = rayHit.transform.position;
                    check.y = check.y + 1f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    //--------------------------------
                    // 아래쪽 검사
                    // ★■
                    // ？
                    //--------------------------------
                    check.x = check.x - 1f;
                    check.y = check.y - 2f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    // 이동 좌표
                    destPos.x = check.x;
                    destPos.y = check.y + 1f;
                    destPos.z = check.z;
                    // 왼쪽 매달림
                    playerState = PlayerState.L_DROP_CLING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.DROP_CLIMBING;
                }
                break;
            case PlayerState.F_DROP:
                //------------------------------------------------
                // 앞쪽 떨어짐 상태 키처리
                //------------------------------------------------

                // 바닥에 닿아있다면
                if (characterController.isGrounded)
                {
                    // 바닥 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 높은 높이에서 떨어짐
                    if (dropPlayerY - centerTrans.position.y > 2f)
                    {
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 높은 곳에서 떨어짐
                        playerState = PlayerState.F_DROP_HIGH;
                        // 애니메이션 높은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_HIGH;
                        // 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    // 낮은 높이에서 떨어짐
                    else
                    {
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.up;
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 낮은 곳에서 떨어짐
                        playerState = PlayerState.F_DROP_LOW;
                        // 애니메이션 낮은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_LOW;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    break;
                }

                // 입력 키 값 ↓
                if (moveInput.y <= -0.3)
                {
                    // 발 기준 왼쪽
                    if (!Physics.Raycast(footTrans.position, Vector3.back, out rayHit, 1f, layerMaskCube))
                    {
                        //없다
                        break;
                    }

                    //--------------------------------
                    // 위쪽 검사
                    // ？
                    // ■★
                    //--------------------------------
                    check = rayHit.transform.position;
                    check.y = check.y + 1f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    //--------------------------------
                    // 아래쪽 검사
                    // ■★
                    //   ？
                    //--------------------------------
                    check.z = check.z + 1f;
                    check.y = check.y - 2f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    // 이동 좌표
                    destPos.x = check.x;
                    destPos.y = check.y + 1f;
                    destPos.z = check.z;
                    // 앞쪽 매달림
                    playerState = PlayerState.F_DROP_CLING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.DROP_CLIMBING;
                }
                break;
            case PlayerState.B_DROP:
                //------------------------------------------------
                // 뒤쪽 떨어짐 상태 키처리
                //------------------------------------------------

                // 바닥에 닿아있다면
                if (characterController.isGrounded)
                {
                    // 바닥 큐브 정보
                    if (!Physics.Raycast(centerTrans.position, Vector3.down, out rayHit, 1f, layerMaskCube))
                    {
                        // 에러
                        break;
                    }

                    // 높은 높이에서 떨어짐
                    if (dropPlayerY - centerTrans.position.y > 2f)
                    {
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 높은 곳에서 떨어짐
                        playerState = PlayerState.B_DROP_HIGH;
                        // 애니메이션 높은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_HIGH;
                        // 약간의 딜레이가 필요합니다
                        actionDelay = 0f;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    // 낮은 높이에서 떨어짐
                    else
                    {
                        // Move 함수에서 처리할 키 값
                        moveKeyValue = Vector2.down;
                        // 목표 이동 위치
                        destPos = rayHit.transform.position;
                        // 낮은 곳에서 떨어짐
                        playerState = PlayerState.B_DROP_LOW;
                        // 애니메이션 낮은 떨어짐
                        animeSwitch = AnimationSwitch.DROP_LOW;
                        // 캐릭터 속도 관련 셋팅
                        saveSpeed = speed;
                        speed = 0.5f;
                    }
                    break;
                }

                // 입력 키 값 ↑
                if (moveInput.y >= 0.3)
                {
                    // 발 기준 왼쪽
                    if (!Physics.Raycast(footTrans.position, Vector3.forward, out rayHit, 1f, layerMaskCube))
                    {
                        //없다
                        break;
                    }

                    //--------------------------------
                    // 위쪽 검사
                    //   ？
                    // ★■
                    //--------------------------------
                    check = rayHit.transform.position;
                    check.y = check.y + 1f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    //--------------------------------
                    // 아래쪽 검사
                    // ★■
                    // ？
                    //--------------------------------
                    check.z = check.z - 1f;
                    check.y = check.y - 2f;

                    // 있다
                    if (Physics.CheckBox(check, box, Quaternion.identity, layerMaskCube))
                    {
                        break;
                    }

                    // 이동 좌표
                    destPos.x = check.x;
                    destPos.y = check.y + 1f;
                    destPos.z = check.z;
                    // 뒤쪽 매달림
                    playerState = PlayerState.B_DROP_CLING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.DROP_CLIMBING;
                }
                break;
            case PlayerState.R_DROP_HIGH:
                //------------------------------------------------
                // 오른쪽 높은 떨어짐
                //------------------------------------------------
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.right;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_DROP_HIGH:
                //------------------------------------------------
                // 왼쪽 높은 떨어짐
                //------------------------------------------------
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.left;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_DROP_HIGH:
                //------------------------------------------------
                // 앞쪽 높은 떨어짐
                //------------------------------------------------
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.up;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_DROP_HIGH:
                //------------------------------------------------
                // 뒤쪽 높은 떨어짐
                //------------------------------------------------
                actionDelay = actionDelay + Time.deltaTime;

                // 준비 동작때문에 약 2.5 대기합니다
                if (actionDelay < 2.5f)
                {
                    break;
                }
                else
                {
                    // Move 함수에서 처리할 키 값
                    moveKeyValue = Vector2.down;
                }

                // 캐릭터 이동 속도를 빠르게
                speed = Mathf.SmoothDamp(currentSpeed, saveSpeed, ref jumpSmoothHorizontal, standSmoothTime);

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 캐릭터의 상태 변화는 애니메이션 클립에서 이벤트를 통해 함수를 호출해서 변경함
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 애니메이션이 끝날 때까지 기다림
                    playerState = PlayerState.EMPTY;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.R_DROP_LOW:
                //------------------------------------------------
                // 오른쪽 낮은 떨어짐
                //------------------------------------------------

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x <= centerTrans.position.x)
                {
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 낮은 떨어짐 종료
                    animeSwitch = AnimationSwitch.DROP_LOW_END;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_DROP_LOW:
                //------------------------------------------------
                // 왼쪽 낮은 떨어짐
                //------------------------------------------------

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.x >= centerTrans.position.x)
                {
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 낮은 떨어짐 종료
                    animeSwitch = AnimationSwitch.DROP_LOW_END;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.x = destPos.x - transform.position.x;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_DROP_LOW:
                //------------------------------------------------
                // 앞쪽 낮은 떨어짐
                //------------------------------------------------

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z <= centerTrans.position.z)
                {
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 낮은 떨어짐 종료
                    animeSwitch = AnimationSwitch.DROP_LOW_END;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_DROP_LOW:
                //------------------------------------------------
                // 뒤쪽 낮은 떨어짐
                //------------------------------------------------

                // 수평 이동 거리만큼 이동 했는가
                if (destPos.z >= centerTrans.position.z)
                {
                    // 이동 정지
                    moveKeyValue = Vector2.zero;
                    // 대기 상태
                    playerState = PlayerState.IDLE;
                    // 애니메이션 낮은 떨어짐 종료
                    animeSwitch = AnimationSwitch.DROP_LOW_END;
                    // 이동 속도 원상 복구
                    speed = saveSpeed;
                    // 플레이어 위치 맞추기
                    moveValue.z = destPos.z - transform.position.z;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.R_DROP_CLING:
                //------------------------------------------------
                // 오른쪽 떨어짐 매달림
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 했는가
                if ((destPos.y - 0.3f) >= centerTrans.position.y)
                {
                    // 오른쪽 떨어짐 등반 상태
                    playerState = PlayerState.R_DROP_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.L_DROP_CLING:
                //------------------------------------------------
                // 왼쪽 떨어짐 매달림
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 했는가
                if ((destPos.y - 0.3f) >= centerTrans.position.y)
                {
                    // 오른쪽 떨어짐 등반 상태
                    playerState = PlayerState.L_DROP_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.F_DROP_CLING:
                //------------------------------------------------
                // 앞쪽 떨어짐 매달림
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 했는가
                if ((destPos.y - 0.3f) >= centerTrans.position.y)
                {
                    // 오른쪽 떨어짐 등반 상태
                    playerState = PlayerState.F_DROP_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.B_DROP_CLING:
                //------------------------------------------------
                // 뒤쪽 떨어짐 매달림
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 했는가
                if ((destPos.y - 0.3f) >= centerTrans.position.y)
                {
                    // 오른쪽 떨어짐 등반 상태
                    playerState = PlayerState.B_DROP_CLIMBING;
                    moveKeyValue = Vector2.zero;
                }
                break;
            case PlayerState.R_DROP_CLIMBING:
                //------------------------------------------------
                // 오른쪽 떨어짐 등반
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, climbVelocity, ref jumpSmoothVertical, 0.001f);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 매달림
                    climbingFlag = true;
                    // 오른쪽 매달림 대기 상태
                    playerState = PlayerState.R_IDLE_CLIMBING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.CLIMBING;
                    // 플레이어 위치 맞추기
                    moveValue.y = destPos.y - centerTrans.position.y;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.L_DROP_CLIMBING:
                //------------------------------------------------
                // 왼쪽 떨어짐 등반
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, climbVelocity, ref jumpSmoothVertical, 0.001f);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 매달림
                    climbingFlag = true;
                    // 왼쪽 매달림 대기 상태
                    playerState = PlayerState.L_IDLE_CLIMBING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.CLIMBING;
                    // 플레이어 위치 맞추기
                    moveValue.y = destPos.y - centerTrans.position.y;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.F_DROP_CLIMBING:
                //------------------------------------------------
                // 앞쪽 떨어짐 등반
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, climbVelocity, ref jumpSmoothVertical, 0.001f);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 매달림
                    climbingFlag = true;
                    // 앞쪽 매달림 대기 상태
                    playerState = PlayerState.F_IDLE_CLIMBING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.CLIMBING;
                    // 플레이어 위치 맞추기
                    moveValue.y = destPos.y - centerTrans.position.y;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.B_DROP_CLIMBING:
                //------------------------------------------------
                // 뒤쪽 떨어짐 등반
                //------------------------------------------------

                // 수직 이동 거리만큼 이동 하지 못했나
                if (destPos.y > centerTrans.position.y)
                {
                    // 위로 이동함
                    currentVelocityY = Mathf.SmoothDamp(currentSpeed, climbVelocity, ref jumpSmoothVertical, 0.001f);
                }
                // 수직 이동 거리만큼 이동 함
                else
                {
                    // 매달림
                    climbingFlag = true;
                    // 뒤쪽 매달림 대기 상태
                    playerState = PlayerState.B_IDLE_CLIMBING;
                    // 애니메이션 매달림
                    animeSwitch = AnimationSwitch.CLIMBING;
                    // 플레이어 위치 맞추기
                    moveValue.y = destPos.y - centerTrans.position.y;
                    characterController.Move(moveValue);
                }
                break;
            case PlayerState.EMPTY:
                // 의도적으로 아무것도 하지않음
                break;
            case PlayerState.CRUSHED_TO_DEATH:
                // 큐브에 깔려 죽음
                moveKeyValue = Vector2.zero;
                // 캐릭터 이동 처리를 하지 않고 함수를 빠져 나옴
                return;
            case PlayerState.DEATH:
                // 플레이어 사망
                moveKeyValue = Vector2.zero;
                break;
            default:
                break;
        }// switch(playerState)

        //Debug.Log("playerState : " + playerState);
        //Debug.Log("playerPos : " + transform.position);
        //Debug.Log("destPos : " + destPos);
        //Debug.Log("moveKeyValue : " + moveKeyValue);
        //Debug.Log(mouseClick);
        //Debug.Log(followCam.transform.eulerAngles);
        //Debug.Log("--------------------------------");
        // 캐릭터 이동
        Move(moveKeyValue);
        // 피해 체크
        CheckDamage();
    }

    // 캐릭터 이동
    private void Move(Vector2 moveInput) {
        float targetSpeed = speed * moveInput.magnitude;
        // Normalize 벡터의 크기를 1로 정규화 하는 함수
        // x, z 평면
        //var moveDiection = Vector3.Normalize(transform.forward * moveInput.y + transform.forward * moveInput.x);
        var moveDiection = new Vector3(moveInput.x, 0, moveInput.y);

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

    // 플레이어 애니메이션 업데이트
    private void UpdateAnimation()
    {
        Vector2 move;


        switch (animeSwitch)
        {
            case AnimationSwitch.MOVE_FLINCH:
                animator.SetTrigger("Move Flinch");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.MOVE_COLLISION:
                animator.SetTrigger("Move Collision");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.UP_COLLISION:
                animator.SetTrigger("Up Collision");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.UP_COLLISION_END:
                animator.SetTrigger("Up Collision End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.JUMP:
                animator.SetTrigger("Jump");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.CLIMBING:
                animator.SetTrigger("Climbing");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.CLIMBING_END:
                animator.SetTrigger("Climbing End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.INTERACTION_START:
                animator.SetTrigger("Idle Interaction Start");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.INTERACTION_END:
                animator.SetTrigger("Idle Interaction End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.PUSH_IDLE:
                animator.SetTrigger("Push Idle");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.PUSH:
                animator.SetTrigger("Push");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.PUSH_END:
                animator.SetTrigger("Push End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.SLIDE:
                animator.SetTrigger("Slide");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.SLIDE_END:
                animator.SetTrigger("Slide End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP:
                animator.SetTrigger("Drop");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP_HIGH:
                animator.SetTrigger("Drop High");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP_LOW:
                animator.SetTrigger("Drop Low");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP_LOW_END:
                animator.SetTrigger("Drop Low End");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP_CLIMBING:
                animator.SetTrigger("Drop Climbing");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.CRUSHED_TO_DEATH:
                animator.SetTrigger("Crushed to Death");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            case AnimationSwitch.DROP_TO_DEATH:
                animator.SetTrigger("Drop to Death");
                animeSwitch = AnimationSwitch.IDLE;
                break;
            default:
                break;
        }

        switch (playerState)
        {
            case PlayerState.L_MOVE:
                move.x = -1f;
                move.y = 0f;
                break;
            case PlayerState.R_MOVE:
                move.x = 1f;
                move.y = 0f;
                break;
            case PlayerState.B_MOVE:
                move.x = 0f;
                move.y = -1f;
                break;
            case PlayerState.F_MOVE:
                move.x = 0f;
                move.y = 1f;
                break;
            case PlayerState.R_MOVE_COLLISION:
                move.x = 1f;
                move.y = 0f;
                break;
            case PlayerState.R_INTERACTION_PULL:
            case PlayerState.R_INTERACTION_PULL_CLIMBING:
            case PlayerState.L_INTERACTION_PULL:
            case PlayerState.L_INTERACTION_PULL_CLIMBING:
            case PlayerState.F_INTERACTION_PULL:
            case PlayerState.F_INTERACTION_PULL_CLIMBING:
            case PlayerState.B_INTERACTION_PULL:
            case PlayerState.B_INTERACTION_PULL_CLIMBING:
            case PlayerState.RL_CLIMBING_MOVE:
            case PlayerState.LL_CLIMBING_MOVE:
            case PlayerState.FR_CLIMBING_MOVE:
            case PlayerState.BL_CLIMBING_MOVE:
            case PlayerState.RL_BL_CHANGE_CLIMBING:
            case PlayerState.LL_FR_CHANGE_CLIMBING:
            case PlayerState.FR_RL_CHANGE_CLIMBING:
            case PlayerState.BL_LL_CHANGE_CLIMBING:
                move.x = -1f;
                move.y = 0f;
                break;
            case PlayerState.RR_CLIMBING_MOVE:
            case PlayerState.LR_CLIMBING_MOVE:
            case PlayerState.FL_CLIMBING_MOVE:
            case PlayerState.BR_CLIMBING_MOVE:
            case PlayerState.RR_FL_CHANGE_CLIMBING:
            case PlayerState.LR_BR_CHANGE_CLIMBING:
            case PlayerState.FL_LR_CHANGE_CLIMBING:
            case PlayerState.BR_RR_CHANGE_CLIMBING:
                move.x = 1f;
                move.y = 0f;
                break;
            default:
                move.x = 0f;
                move.y = 0f;
                break;
        }

        animationSpeedPercent = currentSpeed / speed;
        animator.SetFloat("Vertical Move", move.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", move.x * animationSpeedPercent, 0.05f, Time.deltaTime);
    }


    //-----------------------------------------------
    // 피해 체크
    //-----------------------------------------------
    private void CheckDamage()
    {
        Vector3 box;            // 박스 크기

        // 이미 죽어있음
        if (isDeath)
        {
            return;
        }

        box.x = 0.1f;
        box.y = 0.1f;
        box.z = 0.1f;
        
        if (Physics.CheckBox(headTrans.position, box, Quaternion.identity, layerMaskCube))
        {
            // 플레이어 캐릭터가 안죽었으면
            if (!isDeath)
            {
                // 머리 충돌 옵션
                if (checkHeadCollisionOption)
                {
                    Debug.Log("죽지않음");
                    //----------------------
                    // 죽지 않고 빠져나감
                    //----------------------
                    checkHeadCollisionOption = false;
                    return;
                }
                // 사망 플래그
                isDeath = true;
                // 플레이어 사망
                playerState = PlayerState.CRUSHED_TO_DEATH;
                // 애니메이션 압사
                animeSwitch = AnimationSwitch.CRUSHED_TO_DEATH;
                // 캐릭터 컨트롤러 비활성화
                characterController.enabled = false;
            }
        }
    }


    //-----------------------------------------------
    // 빙판 체크 true, false 값만 반환
    //-----------------------------------------------
    // 미끄러짐 체크
    private bool CheckSlide()
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 바닥 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 바닥이 아이스 큐브
            if (rayHit.transform.gameObject.CompareTag("IceCube"))
            {
                // 미끄러짐
                return true;
            }
        }

        return false;
    }

    //-----------------------------------------------
    // 빙판 체크 외에
    // 연속적인 미끄러짐으로
    // 다음 목표 좌표를 갱신을 합니다
    // destPos : 이동 좌표
    //-----------------------------------------------
    // 미끄러짐 체크
    private bool CheckSlide(Vector3 direction)
    {
        RaycastHit rayHit;      // 레이 충돌한 물체

        // 바닥 검사
        // 있다
        if (Physics.Raycast(transform.position, Vector3.down, out rayHit, 1f, layerMaskCube))
        {
            // 바닥이 아이스 큐브
            if (rayHit.transform.gameObject.CompareTag("IceCube"))
            {
                // 캐릭터 정면 검사
                if (!Physics.Raycast(transform.position, transform.forward, out rayHit, 1f, layerMaskCube))
                {
                    // 미끄러짐
                    destPos = destPos + direction;
                    return true;
                }
            }
        }

        return false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log(hit.gameObject.layer);
    }
}
