using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameGlobalScript;

public class PlayerAction : GameScript
{
    public float m_speed;                   // 캐릭터 이동 속도

    private GameManager m_gameManger;        // 게임 매니저


    // 초기화
    public void Init(GameManager gameManager, float speed)
    {
        m_scriptType = en_ActionScriptType.PLAYER;
        m_gameManger = gameManager;
        m_speed = speed;
    }

    public void MoveForward()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void MoveForwardUp()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void MoveForwardDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.forward + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void MoveBack()
    {
        // 좌표
        transform.position = transform.position + Vector3.back;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void MoveBackUp()
    {
        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void MoveBackDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.back + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    public void MoveLeft()
    {
        // 좌표
        transform.position = transform.position + Vector3.left;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void MoveLeftUp()
    {
        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void MoveLeftDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.left + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void MoveRight()
    {
        // 좌표
        transform.position = transform.position + Vector3.right;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void MoveRightUp()
    {
        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.up;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    public void MoveRightDown()
    {
        // 좌표
        transform.position = transform.position + Vector3.right + Vector3.down;
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }

    // 방향 앞으로
    public void DirectionForward()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    // 방향 뒤로
    public void DirectionBack()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 180, 0);
    }

    // 방향 왼쪽
    public void DirectionLeft()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 270, 0);
    }

    // 방향 오른쪽
    public void DirectionRight()
    {
        // 방향
        transform.eulerAngles = new Vector3(0, 90, 0);
    }
}
