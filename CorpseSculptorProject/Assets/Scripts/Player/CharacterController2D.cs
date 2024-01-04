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
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    
    [Header("점프 관련")]
    public Rigidbody2D m_Rigidbody2D;   //플레이어 리지드바디
    public float m_JumpForce = 400f;    //점프력(임시) -> TODO 누를수록 강도가 높아져야하는 메커니즘으로 변경해야함
    public bool m_AirControl = true;	//플레이어가 점프 도중 움직일수 있는가 -> TODO 결정해야함
    public bool m_Grounded;             //플레이어가 접지되었는지 여부.
    public Vector3 velocity = Vector3.zero;
    public float limitFallSpeed = 25f;  //낙하 속도 제한
    public bool canDoubleJump = true;   //플레이어가 더블점프를 할수있는지
    public float prevVelocityX = 0f;
    public LayerMask groundLayer;       //바닥을 나타내는 레이어
    
    [Header("이동 관련")]
    public bool m_FacingRight = true;   //플레이어가 현재 어느 방향을 바라보고 있는지
    public float m_DashForce = 25f;     //플레이어 대쉬의 크기
    public bool canDash = true;         //플레이어가 대쉬를 할수있는 상황인지 여부
    public bool isDashing = false;      //플레이어가 대쉬를 하는중인지
    public bool canMove = true;         //플레이어가 움직일수 있는지

    [Header("Events")]
    public Animator animator;
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>(); //TODO 애니메이터 연결해야함

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
                canDoubleJump = true;
                /*if (m_Rigidbody2D.velocity.y < 0f)
                    limitVelOnWallJump = false;*/ // TODO 벽타기 관련
            }
        }
    }

    /// <summary>
    /// 플레이어 움직임을 관리하는 로직
    /// </summary>
    /// <param name="move">플레이어의 움직임 속도</param>
    /// <param name="jump">플레이어가 점프중인지</param>
    /// <param name="dash">플레이어가 대쉬중인지</param>
    public void Move(float move, bool jump, bool dash)
    {
        //움직일수 있는지 판단
        if (canMove)
        {
            if (dash && canDash)//&& !isWallSliding
            {
                StartCoroutine(DashCooldown());
            }
            //대쉬
            if (isDashing)
            {
                m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
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
                if (move > 0 && !m_FacingRight) // && !isWallSliding
                {
                    Flip();
                }
                //그렇지 않으면 입력이 플레이어를 왼쪽으로 움직이고 플레이어가 오른쪽을 향하고 있는 경우
                else if (move < 0 && m_FacingRight) // && !isWallSliding
                {
                    Flip();
                }
            }
            
            //플레이어가 점프하는 경우
            if (m_Grounded && jump)
            {
                //점프 애니메이션으로 전환
                /*animator.SetBool("IsJumping", true);
                animator.SetBool("JumpUp", true);*/
                //지면에 있지 않음으로 상태 변경
                m_Grounded = false;
                //플레이어에게 수직으로 힘추가
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
        }
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
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - 1f), new Vector2(1f, 0.5f));
    }
}
