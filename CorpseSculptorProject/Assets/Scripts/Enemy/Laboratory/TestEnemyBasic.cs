using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestEnemyBasic : MonoBehaviour
{
    public float life;
    public bool isCorpseState;
    
    
    private Rigidbody2D rb;
    private bool isInvincible = false;
    //private bool isHitted = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            //TODO 애니메이션 트리거 설정
            //transform.GetComponent<Animator>().SetBool("Hit", true);

            life -= damage; // 라이프 차감
            rb.velocity = Vector2.zero; // 현재 속도를 0으로 초기화

            // 넉백 방향 결정 (캐릭터가 오른쪽을 바라보고 있으면 오른쪽으로, 그렇지 않으면 왼쪽으로 넉백)
            bool isFacingRight = GameManager.Instance.GetCharacterController2D().m_FacingRight;
            float basicForce = 7000f;
            float knockbackForce = isFacingRight ? basicForce : -basicForce;

            rb.AddForce(new Vector2(knockbackForce, 0)); // 넉백 적용

            // 히트 효과 코루틴 실행
            StartCoroutine(HitTime());
        }
    }

    
    IEnumerator HitTime()
    {
        //isHitted = true;
        isInvincible = true;
        yield return new WaitForSeconds(0.1f);
        //isHitted = false;
        isInvincible = false;
    }
    
    IEnumerator EnemyKnockdown()
    {
        isInvincible = true;
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        /*capsule.size = new Vector2(1f, 1f);
        capsule.offset = new Vector2(0f, -0.5f);
        capsule.direction = CapsuleDirection2D.Horizontal;*/
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(0.25f);
        capsule.isTrigger = true;
        yield return new WaitForSeconds(1f);
        isCorpseState = true;
        
        /*
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        yield return new WaitForSeconds(0.25f);
        rb.velocity = new Vector2(0, rb.velocity.y);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
        */

    }
    
}
