using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;
    
    void Update () {
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        
        //이동 속도에 따른 뛰는(Player_Running)애니메이션 조절
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove)); 

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) //점프시작
        {
            jump = true;
            controller.m_JumpForce += controller.m_jumpForceIncrement * Time.deltaTime;
            controller.m_Rigidbody2D.gravityScale = 0f;
            if (controller.m_JumpForce > controller.m_limitJumpForce)
            {
                jump = false;
                //controller.m_JumpForce = controller.m_originalJumpForce;
                controller.m_Rigidbody2D.gravityScale = 5f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W)) //점프끝
        {
            controller.m_JumpForce = controller.m_originalJumpForce;
            controller.m_Rigidbody2D.gravityScale = 5f;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dash = true;
        }
    }
    
    void FixedUpdate ()
    {
        //움직이기
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }

    public void OnFall()
    {
        //Debug.Log($"공중에 있음");
        animator.SetBool("IsJumping", true);
        animator.SetBool("IsLanding", false);
    }

    /// <summary>
    /// 점프 하고 착지할때 CharacterController2D에서 호출됨
    /// </summary>
    public void OnLanding()
    {
        //Debug.Log($"방금 착지함");
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsLanding", true);
    }
}
