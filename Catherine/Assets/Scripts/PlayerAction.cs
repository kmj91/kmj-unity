using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public float m_speed;                   // 캐릭터 이동 속도

    private GameManager m_gameManger;        // 게임 매니저


    // 초기화
    public void Init(GameManager gameManager, float speed)
    {
        m_gameManger = gameManager;
        m_speed = speed;
    }

    public void MoveForward()
    {
        transform.position = transform.position + Vector3.forward;
    }

    public void MoveForwardUp()
    {
        transform.position = transform.position + Vector3.forward + Vector3.up;
    }

    public void MoveForwardDown()
    {
        transform.position = transform.position + Vector3.forward + Vector3.down;
    }

    public void MoveBack()
    {
        transform.position = transform.position + Vector3.back;
    }

    public void MoveBackUp()
    {
        transform.position = transform.position + Vector3.back + Vector3.up;
    }

    public void MoveBackDown()
    {
        transform.position = transform.position + Vector3.back + Vector3.down;
    }

    public void MoveLeft()
    {
        transform.position = transform.position + Vector3.left;
    }

    public void MoveLeftUp()
    {
        transform.position = transform.position + Vector3.left + Vector3.up;
    }

    public void MoveLeftDown()
    {
        transform.position = transform.position + Vector3.left + Vector3.down;
    }

    public void MoveRight()
    {
        transform.position = transform.position + Vector3.right;
    }

    public void MoveRightUp()
    {
        transform.position = transform.position + Vector3.right + Vector3.up;
    }

    public void MoveRightDown()
    {
        transform.position = transform.position + Vector3.right + Vector3.down;
    }
}
