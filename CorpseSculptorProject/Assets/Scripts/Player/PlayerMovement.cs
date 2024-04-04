using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float horizontalMove = 0f;
    private bool jump = false;
    private bool dash = false;
    private bool walljump = false;
    private Vector3 velocity = Vector3.zero;
    
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
            Move(horizontalMove * Time.fixedDeltaTime, jump, dash, walljump);
            jump = false;
            dash = false;
            walljump = false;
        }
    }
    
    /// <summary>
    /// 플레이어 움직임을 관리하는 로직 <br/>
    /// </summary>
    /// <param name="move">플레이어의 움직임 <br/>(0 - 정지, -1 - 왼쪽, +1 - 오른쪽)</param>
    /// <param name="jump">플레이어가 점프키를 눌렀는지</param>
    /// <param name="dash">플레이어가 대쉬키를 눌렀는지</param>
    /// <param name="walljump">플레이어가 벽타기 중에 점프키를 눌렀는지</param>
    public void Move(float move, bool jump, bool dash, bool walljump)
    {
        //움직일수 있는지 판단
        if (controller.canMove)
        {
            //대쉬 조작 ----
            if (dash && controller.canDash)
            {
                StartCoroutine(DashCooldown());
            }
            
            //대쉬 ----
            if (controller.isDashing)
            {
                controller.m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * controller.playerdata.playerDashForce, 0);
            }
            
            //이동 ----
            //땅에 있거나 airControl이 켜져 있는 경우에 플레이어 제어
            else if (controller.m_Grounded || controller.m_AirControl)
            {
                //최고 낙하속도 제한 (낙하 속도가 너무 빠르면 맵이 뚫림)
                if (controller.m_Rigidbody2D.velocity.y < -controller.limitFallSpeed)
                {
                    controller.m_Rigidbody2D.velocity = new Vector2(controller.m_Rigidbody2D.velocity.x, -controller.limitFallSpeed);
                    controller.isBigLanding = true;
                }
                
                //인자로 받은 플레이어 움직임 속도로 플레이어 이동
                Vector3 targetVelocity = new Vector2(move * 10f, controller.m_Rigidbody2D.velocity.y);
                //그리고 SmoothDamp하여 캐릭터에 적용
                controller.m_Rigidbody2D.velocity = Vector3.SmoothDamp(controller.m_Rigidbody2D.velocity, targetVelocity, ref velocity, controller.m_MovementSmoothing);

                //입력이 플레이어를 오른쪽으로 움직이고 플레이어가 왼쪽을 바라보고 있는 경우
                if (move > 0 && !controller.m_FacingRight)
                {
                    controller.Flip();
                    controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity; //중력 재개 5
                }
                //그렇지 않으면 입력이 플레이어를 왼쪽으로 움직이고 플레이어가 오른쪽을 향하고 있는 경우
                else if (move < 0 && controller.m_FacingRight)
                {
                    controller.Flip();
                    controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity; //중력 재개 5
                }
            }
            
            //점프 ----
            if (controller.m_Grounded && jump)
            {
                controller.Player_Jump();
            }
            if (walljump) //벽메달리기 중 점프
            {
                controller.Player_WallJump();
            }

            //벽타기 ----
            if (!controller.m_Grounded && controller.m_IsWall)//점프하고 벽에 붙어있는 상태에서
            {
                // 플레이어가 벽을 향해 이동 중인지 확인
                bool isMovingTowardWall = (controller.climbingDirect > 0 && move > 0) || (controller.climbingDirect < 0 && move < 0);

                if (isMovingTowardWall)
                {
                    // 벽 타기를 실행
                    controller.ClimbingWall();
                    return;
                }
            }
            else
            {
                controller.isClimbing = false;
            }
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
    
    IEnumerator DashCooldown()
    {
        animator.SetBool("IsDashing", true);
        controller.isDashing = true;
        controller.canDash = false;
        yield return new WaitForSeconds(0.1f);
        controller.isDashing = false;
        yield return new WaitForSeconds(0.5f);
        controller.canDash = true;
    }
}
