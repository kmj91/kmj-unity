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
        EMPTY,
        NORMAL_CUBE
    }

    //--------------------------------
    // struct
    //--------------------------------

    // 오브젝트 정보
    public struct ObjectData 
    {
        public MenuElementType objectType;
        public GameObject gameObject;
        public Color color;
    }
}