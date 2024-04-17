using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyHitHandler : MonoBehaviour
{
    public EnemyAI enemyAIScript;
    public Animator animator;
    public float life; //현재 남은 HP
    public float knockbackBasicForce; //피격시 넉백의 강도
    public bool isCorpseState; //시체상태인지
    
    private Rigidbody2D rb;
    private bool isInvincible = false; //무적상태인지
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (life <= 0)
        {
            StartCoroutine(EnemyKnockdown());
        }

        //...TEST CODE
        if (isCorpseState)
        {
            var b = this.GetComponentInChildren<TextMeshPro>();
            b.text = "[시체 상태]";
        }
    }
    
    public void ApplyDamage(float damage) {
        if (!isInvincible) {
            //피격 (Hit) 애니메이션 트리거 설정
            animator.SetTrigger("Hit");

            life -= damage; // 라이프 차감
            rb.velocity = Vector2.zero; // 현재 속도를 0으로 초기화

            // 넉백 방향 결정 (캐릭터가 오른쪽을 바라보고 있으면 오른쪽으로, 그렇지 않으면 왼쪽으로 넉백)
            bool isFacingRight = GameManager.Instance.GetCharacterController2D().m_FacingRight;
            //knockbackBasicForce = 7000f;
            float knockbackForce = isFacingRight ? knockbackBasicForce : -knockbackBasicForce;

            rb.AddForce(new Vector2(knockbackForce, 0)); // 넉백 적용

            // 히트 효과 코루틴 실행
            StartCoroutine(HitTime());
        }
    }
    
    IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.1f); //0.1초동안 무적상태
        isInvincible = false;
    }
    
    IEnumerator EnemyKnockdown()
    {
        //Enemy 상태 초기화
        enemyAIScript.canMove = false;
        enemyAIScript.canRotation = false;
        enemyAIScript.canTracking = false;
        enemyAIScript.canAttack = false;
        enemyAIScript.isRunning = false;
        
        animator.SetBool("Dead", true);
        isInvincible = true;
        
        Collider2D[] colliders = GetComponents<Collider2D>();//몬스터에게 붙어있는 콜라이더 컴포넌트 모두 가져오기
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        
        yield return new WaitForSeconds(0.25f);
        foreach (var collider in colliders) // 모든 Collider2D 컴포넌트의 isTrigger를 true로 설정
        {
            collider.isTrigger = true;
        }
        
        yield return new WaitForSeconds(1f);
        isCorpseState = true; //시체 파밍 상태로 전환
    }
}
