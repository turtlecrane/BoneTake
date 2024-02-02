using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool dash = false;
    private bool walljump = false;
    
    void Update () {
        if (controller.canMove)
        {
            Player_Running();
            Player_JumpingClimbing(KeyCode.Space, KeyCode.W);
            Player_Dash(KeyCode.LeftShift);
        }
    }
    
    void FixedUpdate ()
    {
        //움직이기
        if (controller.canMove)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash, walljump);
            jump = false;
            dash = false;
            walljump = false;
        }
    }
    
    /// <summary>
    /// 플레이어 좌우 움직임
    /// </summary>
    public void Player_Running()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * controller.playerdata.playerSpeed;
        //이동 속도에 따른 뛰는(Player_Running)애니메이션 조절
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
    }

    /// <summary>
    /// 플레이어가 점프하는 기능, 벽메달리기 기능 담당 (중력을 컨트롤함)
    /// </summary>
    public void Player_JumpingClimbing(KeyCode JumpKey1, KeyCode JumpKey2)
    {
        if (Input.GetKeyDown(JumpKey1) || Input.GetKeyDown(JumpKey2)) //점프시작
        {
            controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity; //5
            if (controller.isClimbing)
            {
                controller.m_JumpForce = controller.m_originalJumpForce;
                walljump = true;
            }
        }
        if (Input.GetKey(JumpKey1) || Input.GetKey(JumpKey2)) //점프시작
        {
            controller.m_JumpForce += controller.m_jumpForceIncrement * Time.deltaTime;
            if (controller.m_Grounded && !controller.isClimbing)
            {
                jump = true;
                controller.m_Rigidbody2D.gravityScale = 0f;
            }
            else if (!controller.m_Grounded && controller.isClimbing)//등반중인경우의 점프
            {
                controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity;//5
            }
            if ((controller.m_JumpForce > controller.playerdata.playerJumpForce) && !controller.isClimbing)
            {
                jump = false;
                controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity;//5
            }
        }
        if ((Input.GetKeyUp(JumpKey1) || Input.GetKeyUp(JumpKey2))) //점프끝
        {
            if (!controller.isClimbing)
            {
                controller.m_JumpForce = controller.m_originalJumpForce;
                controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity;//5
            }
        }
    }

    /// <summary>
    /// 플레이어의 대쉬
    /// </summary>
    public void Player_Dash(KeyCode DashKey)
    {
        if (Input.GetKeyDown(DashKey))
        {
            dash = true;
        }
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
    
    public void OnBigLanding() //TODO 큰 착지 이벤트 실행
    {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsLanding", true);
    }
}
