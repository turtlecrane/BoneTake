using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public Transform enemyGFX;
    public CapsuleCollider2D enemyTrackingRange;
    public float enemyAttackingRangeRadius;
    
    public float speed;
    public float maxSpeed;
    public float jumpForce;
    public float jumpCoolTime;
    public LayerMask playerLayer;

    [Header("State")]
    //TESTCODE ...
    public bool canMove;
    public bool canAttack;
    public bool canRotation;
    public bool canTracking;
    public bool isRunning;
    public bool jumpEnabled;
    public bool isGrounded;
    public bool facingRight = true;
    public bool isJumpCoolDown;
    
    private float nextWaypointDistance = 1f;
    private Vector3 startOffset;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    
    
    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        canMove = true;
        canRotation = true;
        //경로찾기를 0.1초마다 리프래쉬 한다.
        InvokeRepeating("UpdatePath", 0f, .2f);
    }
    
    void FixedUpdate()
    {
        if (path == null) return;
        
        // 경로의 마지막에 도달했는지 여부를 설정
        reachedEndOfPath = currentWaypoint >= path.vectorPath.Count;
        if (reachedEndOfPath) return;
        
        // 땅에 닿아 있는 상태인지 검사
        startOffset = transform.position;
        isGrounded = Physics2D.Raycast(startOffset, Vector2.down, 1.5f, LayerMask.GetMask("Ground")).collider != null;
        Debug.DrawRay(startOffset, Vector2.down * 1.5f, Color.red);
        if (isGrounded) jumpEnabled = false;
        
        CheckAttackEnable();
        
        Flip();
        
        // x축 최대 속도 조정
        float clampedSpeed = Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed);
        rb.velocity = new Vector2(clampedSpeed, rb.velocity.y);
        
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        CalculatePathDirection();

        //점프 가능 상태라고 알림
        if (((Vector2)path.vectorPath[currentWaypoint] - rb.position).y >= 0.5f)
        {
            jumpEnabled = true;
        }
        
        if (canMove)
        {
            EnemyMoving();
            EnemyJumping();
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
        if (jumpEnabled && isGrounded && !isJumpCoolDown)
        {
            // 수직 방향으로 점프력을 가함
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            StartCoroutine(JumpCoolDown());
        }
    }

    /// <summary>
    /// 공격 가능 상태인지 검사 (공격범위에 들어왔는지)
    /// </summary>
    void CheckAttackEnable()
    {
        if (canTracking)
        {
            bool playerFound = false; // 이번 프레임에서 플레이어를 찾았는지 여부
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyAttackingRangeRadius, playerLayer);

            foreach (var hit in hits)
            {
                if (hit.gameObject.CompareTag("Player"))
                {
                    playerFound = true; // 플레이어가 범위 안에 있다면 playerFound를 true로 설정
                    if (!canAttack) canAttack = true; // 상태 업데이트
                    break; // 플레이어를 찾았으니 루프 종료
                }
            }

            // 플레이어가 이전 프레임에서는 범위 안에 있었지만, 이번 프레임에서는 범위 안에 없는 경우
            if (canAttack && !playerFound) canAttack = false; // 상태 업데이트
        }
    }
    
    /// <summary>
    /// 경로찾기를 하는 함수
    /// </summary>
    void UpdatePath()
    {
        if (canTracking && seeker.IsDone())
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
        if (((Vector2)path.vectorPath[currentWaypoint] - rb.position).x <= -0.1f)
        {
            facingRight = false;
            isRunning = true;
        }
        else if(((Vector2)path.vectorPath[currentWaypoint] - rb.position).x >= 0.1f)
        {
            facingRight = true;
            isRunning = true;
        }
        else
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
    
    // 플레이어가 추적 범위에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 플레이어 태그를 확인
        {
            canTracking = true;
            canMove = true;
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
    
    private void OnDrawGizmos()
    {
        // 에디터 상에서 공격 범위를 파랑색 원으로 표시
        Gizmos.color = Color.blue; // 파랑색으로 설정
        Gizmos.DrawWireSphere(transform.position, enemyAttackingRangeRadius); // 원 그리기
    }
}
