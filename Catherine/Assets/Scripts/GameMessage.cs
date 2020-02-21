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
        UNDO_POINT,
        UNDO_CUBE
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

    // 되돌리기 지점 데이터 메시지 클래스
    public class UndoPointDataMsg : GameMessage
    {
        public UndoPointDataMsg(Vector3 setPlayerPos, ref CubePosData[] setCubePosArray)
        {
            cubePosArray = setCubePosArray;
            playerPos = setPlayerPos;
            messageType = msgType.UNDO_POINT;
        }

        public Vector3 playerPos;
        public CubePosData[] cubePosArray;
    }

    // 되돌리기 지점에 추가적으로 저장될 큐브 위치
    public class UndoCubeDataMsg : GameMessage
    {
        public UndoCubeDataMsg(GameObject setCubeObject, Vector3 setCubePos)
        {
            cubeObject = setCubeObject;
            CubePos = setCubePos;
            messageType = msgType.UNDO_CUBE;
        }

        public GameObject cubeObject;
        public Vector3 CubePos;
    }

    public class GameOver : GameMessage
    {
        public byte gameOverType;
    }
}