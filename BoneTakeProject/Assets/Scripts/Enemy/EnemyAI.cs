using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    [Header("Component")]
    [HideInInspector]public CharacterController2D charCon2D;
    public EnemyHitHandler enemyHitHandler;
    public EnemyAttack enemyAttack;
    public Transform target;
    public Transform enemyGFX;
    public CapsuleCollider2D enemyTrackingRange;

    [Header("SettingValue")] 
    public Weapon_Type weaponType;
    
    //TODO 발골시 발골되는 아이템의 이름을 설정해야함.
    public Weapon_Name weaponName;
    
    public float speed;
    public float maxSpeed;
    public float jumpForce;
    public float jumpCoolTime;
    public float groundedCheckDistance;
    public LayerMask playerLayer;
    public float randomMovingValue_MIN;
    public float randomMovingValue_MAX;
    public float boneExtractionTime;

    [Header("State")]
    //TESTCODE ...
    public bool canMove;
    public bool canAttack;
    public bool canRotation;
    public bool canTracking;
    public bool isRunning;
    public bool isAttacking = false;
    public bool jumpEnabled;
    public bool isGrounded;
    public bool facingRight = true;
    public bool isJumpCoolDown;
    public float movingCount;

    private Animator animator;
    private float nextWaypointDistance = 1f;
    private Vector3 startOffset;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    private float randomValue;

    private void Awake()
    {
        randomValue = 1f + Random.value;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = enemyGFX.gameObject.GetComponent<Animator>();
        charCon2D = GameManager.Instance.GetCharacterController2D();
        canMove = true;
        canRotation = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //경로찾기를 0.1초마다 리프래쉬 한다.
        InvokeRepeating("UpdatePath", 0f, .2f);
    }

    private void Update()
    {
        animator.SetBool("IsRunning", isRunning);
        if (isRunning)
        {
            movingCount += Time.deltaTime;
        }
        else
        {
            movingCount = 0;
        }
        
        // movingCount가 1과 2사이일 때
        if (movingCount > randomMovingValue_MIN && movingCount <= randomMovingValue_MAX)
        {
            if (movingCount >= randomValue)
            {
                StartCoroutine(StopMoving());
            }
        }
        
        isAttacking = IsCurrentAnimationTag("attack");
    }

    private IEnumerator StopMoving()
    {
        Debug.Log("멈추기");
        movingCount = 0;
        randomValue = 1+Random.value;
        canMove = false;
        yield return new WaitForSeconds(0.5f + Random.value);
        Debug.Log("다시이동");
        canMove = true;
    }

    void FixedUpdate()
    {
        if (path == null) return;
        
        // 경로의 마지막에 도달했는지 여부를 설정
        reachedEndOfPath = currentWaypoint >= path.vectorPath.Count;
        if (reachedEndOfPath) return;
        
        // 땅에 닿아 있는 상태인지 검사
        startOffset = transform.position;
        isGrounded = Physics2D.Raycast(startOffset, Vector2.down, groundedCheckDistance, LayerMask.GetMask("Ground")).collider != null;
        Debug.DrawRay(startOffset, Vector2.down * groundedCheckDistance, Color.red);
        if (isGrounded) jumpEnabled = false;
        
        Flip();
        
        // x축 최대 속도 조정
        float clampedSpeed = Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed);
        rb.velocity = new Vector2(clampedSpeed, rb.velocity.y);
        
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            if (currentWaypoint + 1 < path.vectorPath.Count) // 경로의 마지막 waypoint를 체크
            {
                currentWaypoint++;
            }
            else
            {
                // 경로의 마지막 waypoint에 도달했을 때의 처리를 여기에 추가
                // 예를 들어, 적이 플레이어에 도달했거나, 경로 추적을 중단할 수 있습니다.
                Debug.Log("적이 플레이어에 도달했거나, 경로 추적을 중단");
            }
        }

        CalculatePathDirection();

        //점프 가능 상태라고 알림
        if (((Vector2)path.vectorPath[currentWaypoint] - rb.position).y >= 0.5f)
        {
            jumpEnabled = true;
        }
        
        if (canMove && !charCon2D.playerHitHandler.isDead)
        {
            EnemyMoving();
            EnemyJumping();
        }

        if (isAttacking)
        {
            StartCoroutine(EnemyAttackCoolDown());
        }
    }

    /// <summary>
    /// 움직이기
    /// </summary>
    void EnemyMoving()
    {
        if (isRunning)
        {
            // 플레이어의 방향(오른쪽 또는 왼쪽)에 따라 수평 이동
            float moveDirection = facingRight ? 1f : -1f;
            rb.AddForce(new Vector2(moveDirection * speed * Time.deltaTime, 0), ForceMode2D.Force);
        }
    }

    /// <summary>
    /// 점프
    /// </summary>
    void EnemyJumping()
    {
        if (canMove && jumpEnabled && isGrounded && !isJumpCoolDown &&!isAttacking)
        {
            // 수직 방향으로 점프력을 가함
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            StartCoroutine(JumpCoolDown());
        }
    }
    
    /// <summary>
    /// 경로찾기를 하는 함수
    /// </summary>
    void UpdatePath()
    {
        if (canTracking && seeker.IsDone() && !charCon2D.playerHitHandler.isDead)
        {
            seeker.StartPath(rb.position, target.position, OnPathComplte);
        }
    }

    /// <summary>
    /// 방향 전환
    /// </summary>
    void Flip()
    {
        if (canRotation)
        {
            float scaleX = facingRight ? 1f : -1f;
            enemyGFX.localScale = new Vector3(scaleX, 1f, 1f);
        }
    }

    /// <summary>
    /// 경로 방향 계산 
    /// </summary>
    void CalculatePathDirection()
    {
        if (canTracking && !isAttacking && !charCon2D.playerHitHandler.isDead)
        {
            if (((Vector2)path.vectorPath[currentWaypoint] - rb.position).x <= -0.1f)
            {
                if (canRotation)
                {
                    facingRight = false;
                }
                isRunning = true;
            }
            else if(((Vector2)path.vectorPath[currentWaypoint] - rb.position).x >= 0.1f)
            {
                if (canRotation)
                {
                    facingRight = true;
                }
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }
        }
        if (!canMove)
        {
            isRunning = false;
        }
    }

    /// <summary>
    /// 점프 쿨타임 설정 (연속 점프 방지)
    /// </summary>
    private IEnumerator JumpCoolDown()
    {
        isRunning = false;
        //canMove = false;
        isJumpCoolDown = true;
        yield return new WaitForSeconds(jumpCoolTime);
        isJumpCoolDown = false;
        //canMove = true;
    }
    
    private void OnPathComplte(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    
    private bool IsCurrentAnimationTag(string tag)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(tag);
    }
    
    private IEnumerator EnemyAttackCoolDown()
    {
        canMove = false;
        canRotation = false;
        yield return new WaitUntil(() =>  isAttacking == false);
        canMove = true;
        canRotation = true;
    }
    
    // 플레이어가 추적 범위에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 플레이어 태그를 확인
        {
            canTracking = true;
            if (!isAttacking)
            {
                canMove = true;
            }
        }
    }

    // 플레이어가 추적 범위에서 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 플레이어 태그를 확인
        {
            canTracking = false;
            canMove = false;
        }
    }
}
