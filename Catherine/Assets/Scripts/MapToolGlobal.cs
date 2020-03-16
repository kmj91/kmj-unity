using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace MapToolGlobalScript
{
    //--------------------------------
    // enum
    //--------------------------------

    // 메뉴 UI 요소 타입
    public enum en_MenuElementType
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
    
    // 인덱스 위치
    [Serializable]
    public struct st_IndexPos
    {
        public int iY;
        public int iZ;
        public int iX;
    }

    // 맵 정보
    [Serializable]
    public struct st_MapData
    {
        public int iMapSizeY;                       // 맵 Y축 크기
        public int iMapSizeZ;                       // 맵 Z축 크기
        public int iMapSizeX;                       // 맵 X축 크기
        public st_IndexPos playerPostion;           // 맵툴에 생성된 플레이어 오브젝트 위치 (배열 인덱스 기준)
        public st_IndexPos destPostion;             // 맵툴에 생성된 목적지 오브젝트 위치
        public bool isPlayerActive;                 // 플레이어 생성 확인
        public bool isDestActive;                   // 목적지 생성 확인
    }

    // 맵 오브젝트 정보
    [Serializable]
    public struct st_MapObjectData 
    {
        public en_MenuElementType objectType;       // 오브젝트 타입
        [NonSerialized]
        public GameObject gameObject;               // 맵툴에 생성된 게임 오브젝트
        [NonSerialized]
        public Color color;                         // 오브젝트 원본 컬러
    }
}