using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float horizontalMove = 0f;
    public bool jump = false;
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
            Move(horizontalMove * Time.fixedDeltaTime, dash, walljump); //jump
            //jump = false;
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
    public void Move(float move, bool dash, bool walljump) //bool jump,
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
        // 점프 키를 누르기 시작했을 때
        if ((Input.GetKeyDown(JumpKey1) || Input.GetKeyDown(JumpKey2)))
        {
            if (controller.isClimbing) //벽점프의 경우
            {
                controller.m_JumpForce = controller.m_originalJumpForce;
                walljump = true;
            }
            else if (controller.m_Grounded) //그냥 점프의 경우
            {
                controller.m_Rigidbody2D.gravityScale = 0f; // 중력을 일시적으로 0으로 설정하여 점프 시작 시의 낙하를 방지
                controller.m_JumpForce = controller.m_originalJumpForce; // 점프력을 초기화
                jump = true; // 점프 상태 시작
            }
        }

        // 점프 키를 계속 누르고 있을 때
        if ((Input.GetKey(JumpKey1) || Input.GetKey(JumpKey2)) && jump)
        {
            if (controller.m_JumpForce <= controller.playerdata.playerMaxJumpForce)
            {
                controller.m_Rigidbody2D.AddForce(Vector2.up * controller.m_JumpForce * Time.deltaTime, ForceMode2D.Impulse); // 위쪽으로 힘을 가함
                controller.m_JumpForce += controller.m_jumpForceIncrement * Time.deltaTime; // 시간에 따라 점프력 증가
            }
            else
            {
                jump = false; // 최대 점프력에 도달하면 점프 상태 종료
            }
        }
        
        // 점프 키에서 손을 뗐을 때
        if (Input.GetKeyUp(JumpKey1) || Input.GetKeyUp(JumpKey2))
        {
            jump = false; // 점프 상태 종료
            controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity; // 중력을 원래대로 복구하여 낙하 시작
        }

        // 점프 상태가 아닐 때 중력 복구 
        if (!jump && controller.m_Rigidbody2D.gravityScale == 0f)
        {
            if (controller.isClimbing) return; //등반중인 경우에는 무시
            controller.m_Rigidbody2D.gravityScale = controller.m_playerRigidGravity; // 중력을 원래대로 복구
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
