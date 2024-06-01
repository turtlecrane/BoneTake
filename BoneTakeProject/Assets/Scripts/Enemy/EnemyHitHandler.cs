using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EnemyHitHandler : MonoBehaviour
{
    public ParticleSystem attackParticle;
    public ParticleSystem bloodParticle;
    
    public EnemyAI enemyAIScript;
    public Animator animator;
    public float life; //현재 남은 HP
    public float knockbackBasicForce; //피격시 넉백의 강도
    public bool isCorpseState; //시체상태인지
    public bool isExtracted; //발골완료된 상태인지

    private SpriteRenderer enemySprite;
    private Rigidbody2D rb;
    private bool isInvincible = false; //무적상태인지
    private bool isFading = false; // fade 중인지 상태 확인
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemySprite = animator.GetComponent<SpriteRenderer>();
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

        //발골됐으면
        if (isExtracted && !isFading)
        {
            StartCoroutine(FadeOutAndDestroy(1f));
            isFading = true;
        }

        if (animator.GetBool("Hit"))
        {
            StartCoroutine(HitPauseMovemen());
        }
    }
    
    /// <summary>
    /// PlayerEventKey스크립트에서 SendMessage로 호출됨
    /// </summary>
    /// <param name="damage"></param>
    public void Enemy_ApplyDamage(float damage) {
        if (!isInvincible)
        {
            CharacterController2D charCon2D = CharacterController2D.instance;
            //피격 (Hit) 애니메이션 트리거 설정
            animator.SetBool("Hit", true);
            attackParticle.Play();
            Blink();

            life -= damage; // 라이프 차감
            if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic 
                || charCon2D.playerAttack.weapon_type != Weapon_Type.etc)
            {
                charCon2D.playerAttack.weaponManager.weaponLife -=  1; //무기 HP를 1 줄임
            }
            rb.velocity = Vector2.zero; // 현재 속도를 0으로 초기화
            
            // 플레이어 캐릭터와 적 오브젝트의 위치 비교
            Transform playerTransform = charCon2D.gameObject.transform;
            float knockbackDirection = transform.position.x - playerTransform.position.x > 0 ? 1f : -1f;

            // 넉백 방향 결정 (플레이어 캐릭터가 왼쪽에 있으면 오른쪽으로, 오른쪽에 있으면 왼쪽으로 넉백)
            float knockbackForce = knockbackDirection * Mathf.Abs(knockbackBasicForce);
            rb.AddForce(new Vector2(knockbackForce, 0)); // 넉백 적용
            
            // 히트 효과 코루틴 실행
            StartCoroutine(HitTime());
        }
    }

    private void Blink()
    {
        enemySprite.color = Color.red;
        enemySprite.DOColor(Color.white, 0.5f);
    }

    /// <summary>
    /// 발골될때 피가 뿜어져나오는 파티클 (애니메이션 키에서 호출됨)
    /// </summary>
    public void PlayBloodParticle()
    {
        bloodParticle.Play();
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
        
        //공중에서 사망한경우
        if (!enemyAIScript.isGrounded)
        {
            // enemyAIScript.isGrounded가 true가 될 때까지 기다리기
            yield return new WaitUntil(() => enemyAIScript.isGrounded == true);
        }
        
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
    
    // 알파값을 천천히 0으로 감소시키고 오브젝트 제거
    IEnumerator FadeOutAndDestroy(float duration)
    {
        yield return new WaitForSeconds(2f);//1초 기다리기
        
        SpriteRenderer spriteRenderer = animator.GetComponent<SpriteRenderer>();

        float counter = 0;
        Color spriteColor = spriteRenderer.color;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, counter / duration);
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
    
    
    IEnumerator HitPauseMovemen()
    {
        enemyAIScript.canMove = false;
        yield return new WaitUntil(() => !animator.GetBool("Hit"));
        enemyAIScript.canMove = true;
    }
}
