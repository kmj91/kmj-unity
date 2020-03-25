using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameGlobalScript;

public class CubeAction : GameScript
{
    public float m_speed;                   // 큐브 이동 속도

    
    // 초기화
    public void Init(float speed)
    {
        m_speed = speed;
    }

    public void MoveForward()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward;
    }

    public void MoveBack()
    {
        // 좌표
        transform.position = transform.position + Vector3.back;
    }

    public void MoveLeft()
    {
        // 좌표
        transform.position = transform.position + Vector3.left;
    }

    public void MoveRight()
    {
        // 좌표
        transform.position = transform.position + Vector3.right;
    }
}
