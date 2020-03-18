using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public void MoveForward()
    {
        transform.position = transform.position + Vector3.forward;
    }

    public void MoveBack()
    {
        transform.position = transform.position + Vector3.back;
    }

    public void MoveLeft()
    {
        transform.position = transform.position + Vector3.left;
    }

    public void MoveRight()
    {
        transform.position = transform.position + Vector3.right;
    }
}
