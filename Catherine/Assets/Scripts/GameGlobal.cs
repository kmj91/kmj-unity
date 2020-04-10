using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameGlobalScript
{
    //--------------------------------
    // enum
    //--------------------------------

    // 게임 오브젝트 태그
    public enum en_GameObjectTag
    {
        EMPTY,              // 비어있는
        PLAYER,             // 플레이어
        DEST,               // 목적지
        NORMAL_CUBE,        // 일반 큐브
        ICE_CUBE            // 얼음 큐브
    }

    // 게임 오브젝트 레이어
    public enum en_GameObjectLayer
    {
        EMPTY,              // 비어있는
        PLAYER,             // 플레이어
        DEST,               // 목적지
        CUBE                // 큐브
    }

    // 이동 가능 불가 구분 타입
    public enum en_MeshType
    {
        BLOCK,              // 블록
        EMPTY               // 비어있음
    }

    // 방향
    public enum en_Direction
    {
        FORWARD,
        BACK,
        LEFT,
        RIGHT
    }

    // 액션 스크립트 식별
    public enum en_ActionScriptType
    {
        PLAYER,
        DEST,
        CUBE
    }

    // 큐브 상태
    public enum en_CubeState 
    {
        STAY,
        MOVE_FORWARD,
        MOVE_BACK,
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_UP,
        MOVE_DOWN,
    }

    // 플레이어 상태
    public enum en_PlayerState
    {
        STAY,
        MOVE_FORWARD,
        MOVE_BACK,
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_UP,
        MOVE_DOWN,
    }


    //--------------------------------
    // struct
    //--------------------------------

    // 게임 오브젝트 정보
    public struct st_GameObjectData
    {
        public en_GameObjectTag objectTag;          // 오브젝트 태그
        public en_GameObjectLayer objectLayer;      // 오브젝트 레이어
        public en_MeshType meshData;                // 이동 가능 여부 데이터 타입
        public GameObject gameObject;               // 맵툴에 생성된 게임 오브젝트
        public GameScript actionScript;                 // 게임 오브젝트 액션 스크립트
    }

    //--------------------------------
    // class
    //--------------------------------

    // 최상위 스크립트 클래스
    public class GameScript : MonoBehaviour
    {
        public en_ActionScriptType m_scriptType;     // 스크립트 식별 타입

        public virtual void MoveForward() { }
        public virtual void MoveBack() { }
        public virtual void MoveLeft() { }
        public virtual void MoveRight() { }
    }
}