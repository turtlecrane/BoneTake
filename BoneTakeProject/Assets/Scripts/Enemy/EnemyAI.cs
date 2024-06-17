using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour, IEnemyAI
{
    [Header("Component")]
    public EnemyHitHandler enemyHitHandler;
    public EnemyAttack enemyAttack;
    public Transform enemyGFX;
    public Transform enemyTrackingPosition;
    
    [Header("SettingValue")] 
    public Weapon_Type weaponType;
    public Weapon_Name weaponName;
    public LayerMask playerLayer;
    
    public float speed;
    public float maxSpeed;
    public float jumpForce;
    public float jumpCoolTime;
    public float groundedCheckDistance;
    public float landingCheckDistance;
    public float randomMovingValue_MIN;
    public float randomMovingValue_MAX;
    public float boneExtractionTime;

    [Header("State")]
    public bool isRunning;
    public bool isAttacking = false;
    public bool isLanding;
    public bool isJumpCoolDown;
    public bool jumpEnabled;
    public float movingCount;
    
    public bool canAttack { get; set; }
    public bool canMove { get; set; }
    public bool canRotation { get; set; }
    public bool canTracking { get; set; }
    public bool isGrounded { get; set; }
    public bool facingRight { get; set; }
    
    [HideInInspector] public CharacterController2D charCon2D;
    [HideInInspector] public Transform target;
    [HideInInspector] public Animator animator;
    [HideInInspector] public float nextWaypointDistance = 1f;
    [HideInInspector] public Vector3 startOffset;
    [HideInInspector] public Path path;
    [HideInInspector] public int currentWaypoint = 0;
    [HideInInspector] public bool reachedEndOfPath = false;
    [HideInInspector] public Seeker seeker;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public float randomValue;
    [HideInInspector] public List<Collider2D> collidersInTrigger = new List<Collider2D>();
    
    public List<Collider2D> sortedColliders;
    public int randomMove;
    
    
    private void Awake()
    {
        randomValue = 1f + Random.value;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = enemyGFX.gameObject.GetComponent<Animator>();
        canMove = true;
        canRotation = true;
    }

    void Start()
    {
        //경로찾기를 0.2초마다 리프래쉬 한다.
        InvokeRepeating("UpdatePath", 0f, .2f);
        Invoke("AutoMoveRandomValue", 3);
        charCon2D = CharacterController2D.instance;
        target = GameObject.FindWithTag("Player").transform;
    }
    
    /// <summary>
    /// 움직이기
    /// </summary>
    public void EnemyMoving()
    {
        if (isRunning)
        {
            // 플레이어의 방향(오른쪽 또는 왼쪽)에 따라 수평 이동
            float moveDirection = facingRight ? 1f : -1f;
            AddForceDirection(moveDirection);
        }
    }

    public void AddForceDirection(float direction)
    {
        if(canMove)
        {
            rb.AddForce(new Vector2(direction * speed * Time.deltaTime, 0), ForceMode2D.Force);
        }
    }

    public void NonTrackingAutoMove()
    {
        if (enemyHitHandler.isCorpseState || enemyHitHandler.life <= 0) return;
        AddForceDirection(randomMove);
        if (randomMove < 0)
        {
            facingRight = false;
            canMove = true;
        }
        else if (randomMove > 0)
        {
            facingRight = true;
            canMove = true;
        }
        else if (randomMove == 0)
        {
            canMove = false;
        }

        if (rb.velocity.x == 0)
        {
            isRunning = false;
        }
        else
        {
            isRunning = true;
        }
    }

    /// <summary>
    /// 무작위 움직임 난수 설정 (3초마다 업데이트)
    /// </summary>
    public void AutoMoveRandomValue()
    {
        if (!canTracking)
        {
            randomMove = Random.Range(-1, 2);
        }
        Invoke("AutoMoveRandomValue", 3);
    }

    /// <summary>
    /// 점프
    /// </summary>
    public void EnemyJumping()
    {
        if (canMove && jumpEnabled && isGrounded && !isJumpCoolDown &&!isAttacking)
        {
            // 수직 방향으로 점프력을 가함
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            StartCoroutine(JumpCoolDown());
        }
    }

    /// <summary>
    /// 땅에 닿아 있는 상태인지 검사
    /// </summary>
    public void CheckisGround()
    {
        startOffset = transform.position;
        isGrounded = Physics2D.Raycast(startOffset, Vector2.down, groundedCheckDistance, LayerMask.GetMask("Ground")).collider != null;
        Debug.DrawRay(startOffset, Vector2.down * groundedCheckDistance, Color.red);
        if (isGrounded) jumpEnabled = false;
    }
    
    /// <summary>
    /// 랜딩상태 추적
    /// </summary>
    public void CheckisLanding()
    {
        startOffset = transform.position;
        animator.SetBool("IsLanding", Physics2D.Raycast(startOffset, Vector2.down, landingCheckDistance, LayerMask.GetMask("Ground")).collider != null);
        Debug.DrawRay(startOffset, Vector2.down * landingCheckDistance, Color.magenta);
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
    public void Flip()
    {
        if (canRotation)
        {
            float scaleX = facingRight ? 1f : -1f;
            enemyGFX.localScale = new Vector3(scaleX, 1f, 1f); 
            enemyTrackingPosition.localScale = new Vector3(scaleX, 1f, 1f); 
        }
    }

    /// <summary>
    /// 경로 방향 계산 
    /// </summary>
    public void CalculatePathDirection()
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
        isJumpCoolDown = true;
        yield return new WaitForSeconds(jumpCoolTime);
        isJumpCoolDown = false;
    }
    
    private void OnPathComplte(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    
    public IEnumerator StopMoving()
    {
        movingCount = 0;
        randomValue = 1+Random.value;
        canMove = false;
        yield return new WaitForSeconds(0.5f + Random.value);
        canMove = true;
    }
    
    public bool IsCurrentAnimationTag(string tag)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(tag);
    }
    
    public IEnumerator EnemyAttackCoolDown()
    {
        canMove = false;
        canRotation = false;
        yield return new WaitUntil(() =>  isAttacking == false);
        canMove = true;
        canRotation = true;
    }
    
    // 플레이어가 추적 범위에 들어왔을 때
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Enemy"))
        {
            collidersInTrigger.Add(collision);
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Enemy"))
        {
            sortedColliders = collidersInTrigger.OrderBy(collider => (collider.transform.position - transform.position).sqrMagnitude).ToList();
            if(!sortedColliders[0].gameObject.CompareTag("Player")) return;
            canTracking = true;
            if (!isAttacking)
            {
                canMove = true;
            }
        }
    }

    // 플레이어가 추적 범위에서 나갔을 때
    public void OnTriggerExit2D(Collider2D collision)
    {
        collidersInTrigger.Remove(collision);
        
        if (collision.gameObject.CompareTag("Player")) // 플레이어 태그를 확인
        {
            canTracking = false;
            canMove = false;
        }
    }
}
