using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMessageScript
{
    //--------------------------------
    // enum
    //--------------------------------

    // 메시지 타입
    public enum msgType
    {
        UNDO
    }


    //--------------------------------
    // struct
    //--------------------------------

    // 큐브 위치 데이터
    public struct CubePosData
    {
        public CubePosData(GameObject setCubeObject, Vector3 setCubePos)
        {
            flag = true;
            cubeObject = setCubeObject;
            CubePos = setCubePos;
        }

        public bool flag;
        public GameObject cubeObject;
        public Vector3 CubePos;
    }


    public struct UndoData
    {
        public Vector3 playerPos;
        public CubePosData[] cubePosArray;
    }


    //--------------------------------
    // class
    //--------------------------------

    // 최상위 메시지 클래스
    public class GameMessage
    {
        public msgType messageType;     // 메시지 타입
    }

    // 되돌리기 스택 데이터 메시지 클래스
    public class UndoDataMsg : GameMessage
    {
        public UndoDataMsg(Vector3 setPlayerPos, ref CubePosData[] setCubePosArray)
        {
            cubePosArray = setCubePosArray;
            playerPos = setPlayerPos;
            messageType = msgType.UNDO;
        }

        public Vector3 playerPos;
        public CubePosData[] cubePosArray;
    }

    public class GameOver : GameMessage
    {
        public byte gameOverType;
    }
}