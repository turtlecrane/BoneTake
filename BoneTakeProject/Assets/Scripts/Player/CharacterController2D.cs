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
    [Header("플레이어 컴포넌트")] 
    public Transform playerTarget;
    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerHitHandler playerHitHandler;
    public PlayerInteraction playerInteraction;
    public PlayerEventKey playerEventKey;
    public Rigidbody2D m_Rigidbody2D; //플레이어 리지드바디
    public Animator animator; //플레이어 애니메이터
    public ParticleSystem dustParticle;
    
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
    
    public bool inWater;             //플레이어가 물로 진입했는지 여부
    public bool inDefaultGround;
    public bool inGrassGround;
    public bool inHardGround;
    
    [Tooltip("큰착지시 움직일수 없는 시간 조절")][Range (0.0f, 5.0f)]
    public float bigFallCantMoveCoolTime;
    
    [Header("벽타기 관련")]
    public bool isClimbing = false; //벽 메달리기 중인지
    public LayerMask wallLayer;
    public float limitClimbingCount; //벽에서 몇초만큼 조작이 없을때 추락하는지
    
    private float prevVelocityX = 0f;
    private float climbingCount; //플레이가 몇초동안 벽에 메달려있는지 카운트
    
    [HideInInspector] public PlayerData playerdata; //플레이어 데이터
    [HideInInspector] public float m_MovementSmoothing = .05f;
    [HideInInspector] public int climbingDirect = 0; //어느쪽 벽 메달리기 인지 상태 (왼-false, 오-true, 벽메달리기 상태가 아님(초기화상태) : 0 )
    [HideInInspector] public bool m_AirControl = true;	//플레이어가 점프 도중 움직일수 있음.
    [HideInInspector] public bool m_IsWall = false; //벽 메달리기가 가능한 상태인지
    public float m_playerRigidGravity; //플레이어가 받는 중력값
    [HideInInspector] public float m_JumpForce;             //현재 점프력
    [HideInInspector] public bool m_FacingRight = true;   //플레이어가 현재 어느 방향을 바라보고 있는지
    public bool m_Grounded;             //플레이어가 바닥에 접지되었는지 여부.
    [HideInInspector] public bool isBossDirecting; //보스 만나는 연출
    private int playerLayer;
    private int nonCollidingPlayerLayer;
    public float playTimeCount;
    
    public static CharacterController2D instance;
    
    private void Awake()
    {
        #region 싱글톤
        
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
        }
        
        #endregion
        canMove = true;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_playerRigidGravity = m_Rigidbody2D.gravityScale;
        playerLayer = LayerMask.NameToLayer("Player");
        nonCollidingPlayerLayer = LayerMask.NameToLayer("NonCollidingPlayer");
        playerdata = PlayerDataManager.instance.nowPlayer; //플레이어 데이터 받아오기
        playTimeCount = playerdata.playTime;
    }

    private void Update()
    {
        //상태에 따라 레이어 변경
        gameObject.layer = isDashAttacking||playerHitHandler.isInvincible ? nonCollidingPlayerLayer : playerLayer;  //Enemy와의 충돌무시

        #region 체력 시스템 ... TESTCODE

        if (Input.GetKeyDown(KeyCode.O)) //최대 체력 증가 시스템
        {
            playerdata.playerMaxHP++;
            playerdata.playerHP++;
        }
        else if(Input.GetKeyDown(KeyCode.K)) //최대 체력 감소 시스템
        {
            if (playerdata.playerMaxHP == playerdata.playerHP)
            {
                playerdata.playerHP--;
            }
            playerdata.playerMaxHP--;
        }
        else if(Input.GetKeyDown(KeyCode.P)) //체력 회복 시스템
        {
            if (playerdata.playerMaxHP == playerdata.playerHP)
            {
                return;
            }
            playerdata.playerHP++;
        }
        else if(Input.GetKeyDown(KeyCode.L)) //체력 감소 시스템 (피격과 같음)
        {
            playerdata.playerHP--;
        }

        #endregion
        
        //일시정지 화면 노출
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // isPaused 값을 반전시킴
            GameManager.Instance.GetPauseMenu().isPaused = !GameManager.Instance.GetPauseMenu().isPaused;

            /*if (GameManager.Instance.GetInGameUiManager().cursorObj.activeSelf)
            {
                GameManager.Instance.GetInGameUiManager().cursorObj.SetActive(false);
            }*/
            
            
            // GameManager를 통해 일시정지 메뉴의 활성화 상태 설정
            GameManager.Instance.GetPauseMenu().gameObject.SetActive(GameManager.Instance.GetPauseMenu().isPaused);
        }

        UpdateClimbingState();

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
            isLanding = true;
            isFalling = true;
        }
        else if (m_Rigidbody2D.velocity.y <= -45.0f)//이 속도를 넘어가면 큰착지라고 판단
        {
            //기본착지 -> 큰착지로 전환
            isLanding = false;
            isBigLanding = true;
        }

        playerAttack.canAttack = !isClimbing;

        UpdateAnimatorParameters();
    }
    
    private void FixedUpdate()
    {
        playTimeCount += Time.deltaTime;
        playerdata.playTime = playTimeCount;
        
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        inWater = false;
        
        //접지중임을 판단하는 로직 (이게 있어야 점프가 됨)
        Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y + 0.1f), new Vector2(1f, 0.1f), 0f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                
                if (colliders[i].gameObject.layer == LayerMask.NameToLayer("Water"))
                {
                    inWater = true;
                    isLanding = false;
                    isBigLanding = false;
                }
                else
                {
                    if (!wasGrounded && isFalling)
                    {
                        var playerCameraScript = GameManager.Instance.GetPlayerFollowCameraController();
                        if (isBigLanding)
                        {
                            //착지를했을때 1번만 실행됨.
                            if (playerCameraScript.m_playerFollowCamera.gameObject.activeSelf)
                            {
                                playerEventKey.PlayBigLandingAudio();
                                dustParticle.Play(); //먼지 파티클 재생
                                StartCoroutine(playerCameraScript.PlayerBigLandingNosie());
                            }
                            StartCoroutine(BigLandingMoveCooldown());
                        }
                        else
                        {
                            StartCoroutine(BasicLandingCooldown());
                            //카메라 초기화
                            playerCameraScript.virtualCamera.m_Lens.OrthographicSize = playerCameraScript.lensOrtho_InitSize;
                        }
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
            float xOffset = m_FacingRight ? 1.2f : -1.2f;
            climbingDirect = m_FacingRight ? 1 : -1;
            Collider2D[] collidersWall = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + xOffset, transform.position.y + 1f), new Vector2(0.01f, 2f), 0f, wallLayer);
            CheckWallHangingIsPossible(collidersWall);
        }
    }
    
    private void UpdateClimbingState()
    {
        if (isClimbing && !m_Grounded)
        {
            climbingCount += Time.deltaTime;
            if (climbingCount >= limitClimbingCount)
            {
                InitClimbing();
            }
        }
    }
    
    private void UpdateAnimatorParameters()
    {
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsBigLanding", isBigLanding);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsWallJumping", isWallJumping);
        animator.SetBool("IsLanding", isLanding);
        animator.SetBool("IsHanging", isClimbing);
        animator.SetBool("InWater", inWater);
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
        float xOffset = m_FacingRight ? 1.2f : -1.2f;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + xOffset, transform.position.y + 1f), new Vector2(0.01f, 2f));
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        inDefaultGround = other.gameObject.CompareTag("DefaultGround") || other.gameObject.CompareTag("Untagged") || other.gameObject.CompareTag("Map");
        inGrassGround = other.gameObject.CompareTag("GrassGround");
        inHardGround = other.gameObject.CompareTag("HardGround");
    }

    /// <summary>
    /// 속도를 점진적으로 0 으로 만듬
    /// </summary>
    /// <param name="decelerationTime">0이 되는데 까지의 시간</param>
    /// <returns></returns>
    public IEnumerator DecelerateToZero(Rigidbody2D rb, float decelerationTime)
    {
        float elapsedTime = 0f;
        Vector2 initialVelocity = rb.velocity;

        while (elapsedTime < decelerationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / decelerationTime;
            rb.velocity = Vector2.Lerp(initialVelocity, Vector2.zero, t);
            yield return null;
        }

        // 속도를 완전히 0으로 설정
        rb.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            AudioManager.instance.PlaySFX("WaterEnter");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            AudioManager.instance.PlaySFX("WaterExit");
        }
    }
}
