using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어의 속성정보를 관리하는 스크립트
/// </summary>
public class CharacterController2D : MonoBehaviour
{
    private float m_MovementSmoothing = .05f;
    
    [Header("플레이어 컴포넌트")]
    public Rigidbody2D m_Rigidbody2D;       //플레이어 리지드바디
    public Animator animator; //플레이어 애니메이터
    
    [Header("점프 관련")]
    public bool m_Grounded;             //플레이어가 바닥에 접지되었는지 여부.
    private bool m_AirControl = true;	//플레이어가 점프 도중 움직일수 있음.
    public float m_originalJumpForce = 200f; //초기 점프력
    public float m_JumpForce;               //점프력
    public float m_jumpForceIncrement = 100f; //누를수록 증가되는 점프력의 양
    public float m_limitJumpForce;            //최대 점프력
    private Vector3 velocity = Vector3.zero;
    public float limitFallSpeed = 25f;  //낙하 속도 제한
    public LayerMask groundLayer;       //바닥을 나타내는 레이어
    public float wallJumpHorizontalForce; //벽타기중 점프시 수평으로 튕기는 정도 - 초기값 : 1000
    public float wallJumpVerticalForce; //벽타기중 점프시 점프력 - 초기값 : 900
    
    [Header("이동 관련")]
    public bool m_FacingRight = true;   //플레이어가 현재 어느 방향을 바라보고 있는지
    private bool canDash = true;         //플레이어가 대쉬를 할수있는 상황인지 여부
    private bool isDashing = false;      //플레이어가 대쉬를 하는중인지
    private bool canMove = true;         //플레이어가 움직일수 있는지
    public float m_DashForce = 25f;     //플레이어 대쉬의 크기

    [Header("벽타기 관련")] 
    private bool m_IsWall = false; //벽 메달리기가 가능한 상태인지
    public bool isClimbing = false; //벽 메달리기 중인지
    public LayerMask wallLayer;
    private int climbingDirect = 0; //어느쪽 벽 메달리기 인지 상태 (왼-false, 오-true, 벽메달리기 상태가 아님(초기화상태) : 0 )
    private float prevVelocityX = 0f;
    
    

    [Header("Events")]
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnFallEvent == null)
            OnFallEvent = new UnityEvent();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        
        //접지중임을 판단하는 로직 (이게 있어야 점프가 됨)
        Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y - 1f), new Vector2(1f, 0.5f), 0f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
            if (!wasGrounded )
            {
                OnLandEvent.Invoke();
                /*if (!m_IsWall && !isDashing) 
                    particleJumpDown.Play();*/ //착지 파티클 재생
                /*if (m_Rigidbody2D.velocity.y < 0f)
                    limitVelOnWallJump = false;*/ // TODO 벽타기 관련
            }
        }
        //기본적으로는 벽에 매달릴수 없는 상태임.
        m_IsWall = false;
        climbingDirect = 0;
        
        if (!m_Grounded)//플레이어가 접지중이 아니라면
        {
            OnFallEvent.Invoke();//떨어지는 이벤트 호출
            #region 벽타기 관련
            Vector2 boxPosition;
            if (m_FacingRight)
            {
                boxPosition = new Vector2(transform.position.x + 1f, transform.position.y);
                Collider2D[] collidersWall = Physics2D.OverlapBoxAll(boxPosition, new Vector2(0.2f, 2f), 0f, wallLayer);
                CheckWallHangingIsPossible(collidersWall);
                climbingDirect = 1;
            }
            else
            {
                boxPosition = new Vector2(transform.position.x - 1f, transform.position.y);
                Collider2D[] collidersWall = Physics2D.OverlapBoxAll(boxPosition, new Vector2(0.2f, 2f), 0f, wallLayer);
                CheckWallHangingIsPossible(collidersWall);
                climbingDirect = -1;
            }
            #endregion
        }
    }

    /// <summary>
    /// 인자로 들어온 collider의 layerMask가 wallLayer일때 벽타기 가능상태인지 확인하는 함수
    /// </summary>
    /// <param name="_collidersWall">layerMask가 wall로 되어있는 Physics2D배열</param>
    public void CheckWallHangingIsPossible(Collider2D[] _collidersWall)
    {
        for (int i = 0; i < _collidersWall.Length; i++)
        {
            if (_collidersWall[i].gameObject != null)
            {
                isDashing = false;//대쉬 불가능
                m_IsWall = true;//벽타기 가능상태로 전환
            }
        }
            
        prevVelocityX = m_Rigidbody2D.velocity.x;
    }
    
    /// <summary>
    /// 플레이어 움직임을 관리하는 로직 <br/>
    /// PlayerMovement.cs에서 Update호출됨
    /// </summary>
    /// <param name="move">플레이어의 움직임 <br/>(0 - 정지, -1 - 왼쪽, +1 - 오른쪽)</param>
    /// <param name="jump">플레이어가 점프키를 눌렀는지</param>
    /// <param name="dash">플레이어가 대쉬키를 눌렀는지</param>
    /// <param name="walljump">플레이어가 벽타기 중에 점프키를 눌렀는지</param>
    public void Move(float move, bool jump, bool dash, bool walljump)
    {
        Debug.Log("move : " + move + ", jump : " + jump + ", dash : " + dash + ", walljump : " + walljump);
        //움직일수 있는지 판단
        if (canMove)
        {
            //대쉬 조작 ----
            if (dash && canDash)
            {
                StartCoroutine(DashCooldown());
            }
            
            //대쉬 ----
            if (isDashing)
            {
                m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            
            //이동 ----
            //땅에 있거나 airControl이 켜져 있는 경우에 플레이어 제어
            else if (m_Grounded || m_AirControl)
            {
                if (m_Rigidbody2D.velocity.y < -limitFallSpeed)
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -limitFallSpeed);
                //인자로 받은 플레이어 움직임 속도로 플레이어 이동
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                //그리고 SmoothDamp하여 캐릭터에 적용
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

                //입력이 플레이어를 오른쪽으로 움직이고 플레이어가 왼쪽을 바라보고 있는 경우
                if (move > 0 && !m_FacingRight)
                {
                    Flip();
                    m_Rigidbody2D.gravityScale = 5f; //중력 재개
                }
                //그렇지 않으면 입력이 플레이어를 왼쪽으로 움직이고 플레이어가 오른쪽을 향하고 있는 경우
                else if (move < 0 && m_FacingRight)
                {
                    Flip();
                    m_Rigidbody2D.gravityScale = 5f; //중력 재개
                }
            }
            
            //점프 ----
            if (m_Grounded && jump)
            {
                Player_Jump();
            }
            if (walljump) //벽메달리기 중 점프
            {
                Player_WallJump();
            }

            //벽타기 ----
            if (!m_Grounded && m_IsWall)//점프하고 벽에 붙어있는 상태에서
            {
                if (climbingDirect > 0 && move>0)//오른쪽을 바라보고 방향키 오른쪽을 누르면
                {
                    ClimbingWall();
                }
                else if (climbingDirect < 0 && move<0)//왼쪽을 바라보고
                {
                    ClimbingWall();
                }
            }
            else
            {
                isClimbing = false;
            }
        }
    }

    /// <summary>
    /// 플레이어의 점프하기
    /// </summary>
    public void Player_Jump()
    {
        //점프 애니메이션으로 전환
        animator.SetBool("IsJumping", true);
        
        //지면에 있지 않음으로 상태 변경
        m_Grounded = false;
        
        //플레이어에게 수직으로 힘추가
        m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
    }
    
    /// <summary>
    /// 플레이어의 벽에서 점프하기
    /// </summary>
    public void Player_WallJump()
    {
        Debug.Log("벽타기점프");
        
        //점프 애니메이션으로 전환
        animator.SetBool("IsJumping", true);
        
        m_Rigidbody2D.velocity = new Vector2(0f, 0f);
        m_Rigidbody2D.AddForce(new Vector2((-1*transform.localScale.x) * wallJumpHorizontalForce, wallJumpVerticalForce));
        
        //뒤집기
        Flip();
    }

    /// <summary>
    /// 벽으로 점프하고 벽쪽으로 방향키를 누르면 벽타기로 간주, <br/> *벽에 고정되어있게 함.
    /// </summary>
    public void ClimbingWall()
    {
        isClimbing = true;
        m_Rigidbody2D.velocity = new Vector2(0f, 0f); // 속도 초기화
        m_Rigidbody2D.gravityScale = 0f; // 중력 제거
    }

    /// <summary>
    /// 대쉬 쿨타임 및 대쉬중인지 판단 여부 bool 변수 설정
    /// </summary>
    IEnumerator DashCooldown()
    {
        animator.SetBool("IsDashing", true);
        isDashing = true;
        canDash = false;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }
    
    /// <summary>
    /// 플레이어가 바라보는곳으로 플레이어의 이미지를 뒤집음
    /// </summary>
    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    //충돌을 감지하는 기즈모들를 표시
    private void OnDrawGizmosSelected()
    {
        //착지검사
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - 1f), new Vector2(1f, 0.5f));
        
        //플레이어가 바라보는 방향의 박스
        Gizmos.color = Color.blue;
        if (m_FacingRight)//오른쪽을 보고있는 경우
        {
            Gizmos.DrawWireCube(new Vector2(transform.position.x + 1f, transform.position.y), new Vector2(0.2f, 2f));
        }
        else
        {
            Gizmos.DrawWireCube(new Vector2(transform.position.x - 1f, transform.position.y), new Vector2(0.2f, 2f));
        }
    }
    
    /// <summary>
    /// 레이케스트 디버그용 로그찍는 함수
    /// </summary>
    /// <param name="colliders">레이케스트로 감지하는 콜라이더들</param>
    private void LogColliderNames(Collider2D[] colliders)
    {
        foreach (var collider in colliders)
        {
            if (collider != null && collider.gameObject != null)
            {
                Debug.Log(collider.gameObject.name);
            }
        }
    }
}
