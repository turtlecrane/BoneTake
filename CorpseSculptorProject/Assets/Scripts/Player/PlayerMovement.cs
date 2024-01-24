using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool dash = false;
    private bool walljump = false;
    
    void Update () {
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        
        //이동 속도에 따른 뛰는(Player_Running)애니메이션 조절
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) //점프시작
        {
            controller.m_Rigidbody2D.gravityScale = 5f;
            if (controller.isClimbing)
            {
                controller.m_JumpForce = controller.m_originalJumpForce;
                walljump = true;
            }
        }
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) //점프시작
        {
            controller.m_JumpForce += controller.m_jumpForceIncrement * Time.deltaTime;
            if (controller.m_Grounded && !controller.isClimbing)
            {
                jump = true;
                controller.m_Rigidbody2D.gravityScale = 0f;
            }
            if ((controller.m_JumpForce > controller.m_limitJumpForce) && !controller.isClimbing)
            {
                jump = false;
                controller.m_Rigidbody2D.gravityScale = 5f;
            }
        }
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))) //점프끝
        {
            if (!controller.isClimbing)
            {
                controller.m_JumpForce = controller.m_originalJumpForce;
                controller.m_Rigidbody2D.gravityScale = 5f;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dash = true;
        }
    }
    
    void FixedUpdate ()
    {
        //움직이기
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash, walljump);
        jump = false;
        dash = false;
        walljump = false;
    }

    public void OnFall()
    {
        animator.SetBool("IsJumping", true);
        animator.SetBool("IsLanding", false);
    }

    /// <summary>
    /// 점프 하고 착지할때 CharacterController2D에서 호출됨
    /// </summary>
    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsLanding", true);
    }
}
