using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInput : MonoBehaviour
{
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";
    public string clickButtonName = "Fire1";

    public Vector2 moveInput { get; private set; }
    public bool click { get; private set; }

    private void Update()
    {
        // 방향키
        float HorizontalMove = Input.GetAxis(moveHorizontalAxisName);
        float VerticalMove = Input.GetAxis(moveVerticalAxisName);

        // 더 큰 값이 우선 순위
        if (Math.Abs(HorizontalMove) >= Math.Abs(VerticalMove))
        {
            // 입력 값
            moveInput = new Vector2(HorizontalMove, 0);
        }
        else
        {
            // 입력 값
            moveInput = new Vector2(0, VerticalMove);
        }

        // 클릭
        click = Input.GetButton(clickButtonName);

    }
}
