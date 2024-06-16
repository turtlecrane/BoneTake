using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Flight : MonoBehaviour, IEnemyAI
{
    [Header("Component")]
    [HideInInspector] public CharacterController2D charCon2D;
    public EnemyHitHandler enemyHitHandler;
    public EnemyAttack enemyAttack;
    public Transform enemyGFX;
    //public Transform enemyTrackingPosition;
    [HideInInspector] public Transform target;

    [Header("SettingValue")] 
    public Weapon_Type weaponType;
    public Weapon_Name weaponName;
    public LayerMask playerLayer;

    public float trackingSpeed; //추적 이동 속도
    public float dashSpeed; //돌진 속도
    public float boneExtractionTime;
    public float groundCheckDistance;
    
    [Header("State")]
    public bool isAttacking = false;
    public bool closeToGround = false;
    public bool canTracking { get; set; }
    public bool facingRight { get; set; }
    public bool canMove { get; set; }
    public bool canRotation { get; set; }
    public bool canAttack { get; set; }
    public bool isGrounded { get; set; }
    
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = enemyGFX.gameObject.GetComponent<Animator>();
        canMove = true;
        canRotation = true;
    }

    private void Start()
    {
        InvokeRepeating("FacingPlayer", 1f, 1f);
        InvokeRepeating("CheckisGround", 1f, 0.5f);
        charCon2D = CharacterController2D.instance;
        target = GameObject.FindWithTag("Player").transform;
    }

    /// <summary>
    /// 플레이어 향하기
    /// </summary>
    public void FacingPlayer()
    {
        if (target == null)
        {
            if (target == null) Debug.Log("플레이어 대상을 분실함.");
            return;
        }

        // 플레이어의 위치와 적의 위치 비교
        facingRight = target.position.x >= transform.position.x;
        Flip();
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
            //enemyTrackingPosition.localScale = new Vector3(scaleX, 1f, 1f); 
        }
    }
    
    /// <summary>
    /// 땅과의 거리 체크
    /// </summary>
    public void CheckisGround()
    {
        closeToGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground")).collider != null;
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, Color.red);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
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
    
    /// <summary>
    /// 속도를 점진적으로 0 으로 만듬
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="decelerationTime"></param>
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
}
