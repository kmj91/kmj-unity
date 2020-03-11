using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MapToolGlobalScript
{
    //--------------------------------
    // enum
    //--------------------------------

    // 메뉴 UI 요소 타입
    public enum MenuElementType
    {
        EMPTY,              // 아무것도 선택되지 않음
        PLAYER,             // 플레이어
        DEST,               // 목적지
        NORMAL_CUBE,        // 일반 큐브
        ICE_CUBE            // 얼음 큐브
    }

    //--------------------------------
    // struct
    //--------------------------------

    // 오브젝트 정보
    [System.Serializable]
    public struct ObjectData 
    {
        public MenuElementType objectType;
        [System.NonSerialized]
        public GameObject gameObject;
        [System.NonSerialized]
        public Color color;
    }
}