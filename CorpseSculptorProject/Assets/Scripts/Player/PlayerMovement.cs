using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Count_dashAttackCool;
    public float Time_dashAttackCool;
    public GameObject testDashAttackText;
    
    private bool jump = false;
    private bool walljump = false;
    private Vector3 velocity = Vector3.zero;
    private CharacterController2D controller;
    private Animator animator;

    [HideInInspector] public float horizontalMove = 0f;

    private void Awake()
    {
        controller = GameManager.Instance.GetCharacterController2D();
        animator = controller.animator;
    }

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
            Move(horizontalMove * Time.fixedDeltaTime, walljump);
            walljump = false;
        }
        
        //대쉬공격 쿨타임 업데이트
        Count_dashAttackCool += Time.fixedDeltaTime;
        if (Count_dashAttackCool > Time_dashAttackCool)
        {
            controller.canDashAttack = true;
        }
        //대쉬공격이 가능한 상태면 UI로 표시
        testDashAttackText.SetActive(controller.canDashAttack);
    }
    
    /// <summary>
    /// 플레이어 움직임을 관리하는 로직 <br/>
    /// </summary>
    /// <param name="move">플레이어의 움직임 <br/>(0 - 정지, -1 - 왼쪽, +1 - 오른쪽)</param>
    /// <param name="jump">플레이어가 점프키를 눌렀는지</param> 
    /// <param name="dash">플레이어가 대쉬키를 눌렀는지</param>
    /// <param name="walljump">플레이어가 벽타기 중에 점프키를 눌렀는지</param>
    public void Move(float move, bool walljump) //bool jump, bool dash, 
    {
        //움직일수 있는지 판단
        if (controller.canMove)
        {
            //이동 ----
            //땅에 있거나 airControl이 켜져 있는 경우에 플레이어 제어
            if (controller.m_Grounded || controller.m_AirControl)
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
        // 대쉬 키 입력과 대쉬 가능 여부를 확인
        if (Input.GetKeyDown(DashKey) && controller.canDash && controller.canMove)
        {
            // 대쉬 공격이 가능한 상태인지 판단하여 애니메이션 설정
            if (controller.canDashAttack)
            {
                // 대쉬 공격 쿨타임 카운트 초기화 및 대쉬 공격 가능 상태를 false로 설정
                Count_dashAttackCool = 0f;
                controller.canDashAttack = false;
                controller.isDashAttacking = true;
            }
            else // 대쉬 상태를 활성화하고, 대쉬 쿨다운을 시작
            {
                controller.isDashing = true;
            }
            StartCoroutine(DashCooldown());
        }
    }
    
    IEnumerator DashCooldown()
    {
        animator.SetBool("IsDashAttacking", controller.isDashAttacking);
        animator.SetBool("IsDashing", controller.isDashing);
        
        // 캐릭터 이동 처리
        controller.m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * controller.playerdata.playerDashForce, 0);
        // 연속 대쉬 방지를 위해 대쉬 가능 상태를 false로 설정
        controller.canDash = false;
        
        // 대쉬 이동 시간 처리 (대쉬 애니메이션의 지속 시간)
        yield return new WaitForSeconds(0.5f);
        // 대쉬 상태 해제
        controller.isDashing = false;
        controller.isDashAttacking = false;
        
        // 대쉬 쿨다운 처리
        yield return new WaitForSeconds(0.5f);
        // 다시 대쉬를 할 수 있도록 대쉬 가능 상태를 true로 설정
        controller.canDash = true;
    }

}
