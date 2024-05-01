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
    [Header("플레이어 데이터")]
    public PlayerData playerdata;
    
    [Space (10f)]
    [Header("플레이어 컴포넌트")] 
    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerHitHandler playerHitHandler;
    public Rigidbody2D m_Rigidbody2D;       //플레이어 리지드바디
    public Animator animator; //플레이어 애니메이터
    
    [Header("점프 관련")]
    public bool isJumping; //점프중인지
    public bool isWallJumping; //벽점프중인지
    public bool isFalling; //추락중인지
    public bool isBigLanding;           //큰 착지인지 아닌지 판단
    public bool isLanding;           //기본 착지인지 아닌지 판단
    public float m_jumpForceIncrement; //누를수록 증가되는 점프력의 양
    public float minJumpForce; //초기 점프력
    public float limitFallSpeed;  //낙하 속도 제한
    public float wallJumpHorizontalForce; //벽타기중 점프시 수평으로 튕기는 정도 - 초기값 : 1000
    public float wallJumpVerticalForce; //벽타기중 점프시 점프력 - 초기값 : 900
    public LayerMask groundLayer;       //바닥을 나타내는 레이어
    
    [Header("이동 관련")]
    public bool canMove;         //플레이어가 움직일수 있는지
    public bool canDash = true;         //플레이어가 대쉬를 할수있는 상황인지 여부
    public bool isDashing = false;      //플레이어가 대쉬를 하는중인지
    public bool isDashAttacking;
    public bool canDashAttack = false;   //플레이어가 대쉬어택을 할수있는 상태인지
    
    [Tooltip("큰착지시 움직일수 없는 시간 조절")][Range (0.0f, 5.0f)]
    public float bigFallCantMoveCoolTime;
    
    [Header("벽타기 관련")]
    public bool isClimbing = false; //벽 메달리기 중인지
    public LayerMask wallLayer;
    
    [Tooltip("벽에서 몇초만큼 조작이 없을때 추락하는지")]
    public float limitClimbingCount;
    
    private float prevVelocityX = 0f;
    public float climbingCount; //플레이가 몇초동안 벽에 메달려있는지 카운트
    //---------------------
    
    [HideInInspector] public float m_MovementSmoothing = .05f;
    [HideInInspector] public int climbingDirect = 0; //어느쪽 벽 메달리기 인지 상태 (왼-false, 오-true, 벽메달리기 상태가 아님(초기화상태) : 0 )
    [HideInInspector] public bool m_AirControl = true;	//플레이어가 점프 도중 움직일수 있음.
    [HideInInspector] public bool m_IsWall = false; //벽 메달리기가 가능한 상태인지
    [HideInInspector] public float m_playerRigidGravity; //플레이어가 받는 중력값
    [HideInInspector] public float m_JumpForce;             //현재 점프력
    [HideInInspector] public bool m_FacingRight = true;   //플레이어가 현재 어느 방향을 바라보고 있는지
    [HideInInspector] public bool m_Grounded;             //플레이어가 바닥에 접지되었는지 여부.
    private int playerLayer;
    private int nonCollidingPlayerLayer;
    
    private void Awake()
    {
        canMove = true;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_playerRigidGravity = m_Rigidbody2D.gravityScale;
        playerLayer = LayerMask.NameToLayer("Player");
        nonCollidingPlayerLayer = LayerMask.NameToLayer("NonCollidingPlayer");
        //플레이어 데이터 받아오기
        ReloadPlayerData();
    }

    private void Update()
    {
        //상태에 따라 레이어 변경
        if (isDashAttacking)
        {
            //gameObject.tag = "NonCollidingPlayer";
            gameObject.layer = nonCollidingPlayerLayer; //Enemy와의 충돌무시
        }
        else
        {
            //gameObject.tag = "Player";
            gameObject.layer = playerLayer;
        }
        
        if (isClimbing && !m_Grounded) 
        {
            climbingCount += Time.deltaTime; // 초단위로 카운팅
            if (climbingCount >= limitClimbingCount)
            { 
                InitClimbing(); // 클라이밍 종료
            }
        }

        if (m_Grounded)
        {
            isJumping = false;
        }
        
        if (m_Rigidbody2D.velocity.y >= 0.1)
        {
            isJumping = true;
        }
        else if ( -39.0f < m_Rigidbody2D.velocity.y && m_Rigidbody2D.velocity.y <= -0.1)
        {
            //떨어지고있으면
            isJumping = false;
            isWallJumping = false;
            isFalling = true;
            isLanding = true;
        }
        else if (m_Rigidbody2D.velocity.y <= -39.0f) //절대좌표로 약 Y:15-16에서 플레이어가 떨어졌을때의 속도
        {
            //기본착지 -> 큰착지로 전환
            isLanding = false;
            isBigLanding = true;
        }
        
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsBigLanding", isBigLanding);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsWallJumping", isWallJumping);
        animator.SetBool("IsLanding", isLanding);
        animator.SetBool("IsHanging", isClimbing);
        
    }
    
    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        
        //접지중임을 판단하는 로직 (이게 있어야 점프가 됨)
        Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y + 0.1f), new Vector2(1f, 0.1f), 0f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded && isFalling)
                {
                    var playerCameraScript = GameManager.Instance.GetPlayerFollowCameraController();
                    if (isBigLanding)
                    {
                        //착지를했을때 1번만 실행됨.
                        if (playerCameraScript.m_playerFollowCamera.gameObject.activeSelf)
                        {
                            StartCoroutine(playerCameraScript.PlayerBigLandingNosie());
                        }
                        StartCoroutine(BigLandingMoveCooldown());
                    }
                    else
                    {
                        StartCoroutine(BasicLandingCooldown());
                        //카메라 초기화
                        playerCameraScript.virtualCamera.m_Lens.OrthographicSize = playerCameraScript.lensOrtho_InitSize;
                        /*if (!m_IsWall && !isDashing)
                            particleJumpDown.Play();
                        */ //착지 파티클 재생
                    }
                }
                isFalling = false;
            }
        }
        
        //기본적으로는 벽에 매달릴수 없는 상태임.
        m_IsWall = false;
        climbingDirect = 0;
        
        if (!m_Grounded)//플레이어가 접지중이 아니라면
        {
            //벽타기 관련
            float xOffset = m_FacingRight ? 1f : -1f;
            climbingDirect = m_FacingRight ? 1 : -1;
            Collider2D[] collidersWall = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + xOffset, transform.position.y + 1f), new Vector2(0.01f, 2f), 0f, wallLayer);
            CheckWallHangingIsPossible(collidersWall);
        }
    }
    
    /// <summary>
    /// 플레이어의 벽에서 점프하기
    /// </summary>
    public void Player_WallJump()
    {
        climbingCount = 0f;
        isWallJumping = true;
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
        //초기화
        isJumping = false;
        isWallJumping = false;
        isFalling = false;
        isLanding = false;
        climbingCount = 0f;
    }
    
    /// <summary>
    /// 벽타기가 초기화 처리하는 함수<br/>(벽타기 카운트 초기화, 중력복구 등)
    /// </summary>
    public void InitClimbing()
    {
        Debug.Log("벽타기 종료");
        isClimbing = false;
        m_JumpForce = 0f;
        m_Rigidbody2D.gravityScale = m_playerRigidGravity;
        climbingCount = 0f;
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
    
    public void ReloadPlayerData()
    {
        playerdata = GameManager.Instance.GetPlayerDataManager().playerData;
    }

    /// <summary>
    /// 큰착지 후 움직일수없게 하는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator BigLandingMoveCooldown()
    {
        m_Rigidbody2D.velocity = Vector2.zero;
        canMove = false;
        yield return new WaitForSeconds(bigFallCantMoveCoolTime);
        canMove = true;
        isBigLanding = false;
    }
    
    IEnumerator BasicLandingCooldown()
    {
        //Debug.Log("기본 착지를 함");
        yield return new WaitForSeconds(0.1f);
        isLanding = false;
    }
    
    /// <summary>
    /// 플레이어가 바라보는곳으로 플레이어의 이미지를 뒤집음
    /// </summary>
    public void Flip()
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
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y + 0.1f), new Vector2(1f, 0.1f));
        
        //플레이어가 바라보는 방향의 박스 (벽검사 등)
        Gizmos.color = Color.blue;
        float xOffset = m_FacingRight ? 1f : -1f;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + xOffset, transform.position.y + 1f), new Vector2(0.01f, 2f));
    }
}
