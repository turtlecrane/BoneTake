using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boss00_Attack : BossAttack
{
    [Header("Boss00 Variables")]
    public ParticleSystem dustParticle;
    public Boss00_EventKeyController eventKeyController;
    public Transform shootPoint;
    public EnemyBullet bullet;
    public float shootSpeed;
    public float fallSpeed = 1000f; // 낙하할 때 부여할 힘의 크기
    public Transform jumpPosition;
    public float jumpMoveDuration = 1.0f;
    
    private Transform player;
    
    void Update()
    {
        isAttacking = IsCurrentAnimationTag("attack");
    
        // 플레이어 오브젝트 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (CanTrackingPlayer())
        {
            float distanceToPlayer = Vector2.Distance(bossTranform.transform.position, player.position);
            UpdateTowardsPlayer(player.position - bossTranform.transform.position);
            PerformAttackOnDistance(distanceToPlayer);
        }
    }

    private bool CanTrackingPlayer()
    {
        return player != null && 
               !bossHitHandler.isStun && 
               !bossHitHandler.isCorpseState && 
               !bossDirection.isDirecting && 
               !CharacterController2D.instance.playerHitHandler.isDead;
    }

    private void UpdateTowardsPlayer(Vector3 playerDirection)
    {
        if (!isAttacking)
        {
            bossTranform.transform.localScale = new Vector3(playerDirection.x < 0 ? -1 : 1, 1, 1);
            facingRight = playerDirection.x >= 0;
        }
    }

    private void PerformAttackOnDistance(float distanceToPlayer)
    {
        if(isAttacking) return;
        
        if (distanceToPlayer <= closeAttackDistance) 
        {
            //animator.SetBool("Attack01", true);
            //StartCoroutine(Boss00_Attack01());
            //애니메이션 변경
            animator.SetBool("Attack01_Jump", true);
        }
        else if (distanceToPlayer < farAttackDistance && !animator.GetBool("Attack01_Jump"))
        {
            animator.SetBool("Attack02", true);
        }
        else if (distanceToPlayer > farAttackDistance && !animator.GetBool("Attack01_Jump"))
        {
            animator.SetBool("Attack03", true);
        }
    }

    public IEnumerator Boss00_Attack01()
    {
        rb.gravityScale = 0f;
        
        // 지정된 시간 동안 위치로 이동
        // Dotween을 사용하여 폭발적으로 이동
        rb.gameObject.transform.DOMove(jumpPosition.position, jumpMoveDuration)
            .SetEase(Ease.OutExpo); // SetEase를 통해 폭발적 이동 효과 적용

        // 이동 시간 동안 대기
        yield return new WaitForSeconds(jumpMoveDuration);

        // 4. 위치에서 1초간 대기
        yield return new WaitForSeconds(1.0f);

        // 5. 중력을 원래대로 되돌리기
        rb.gravityScale = 5f;

        // 6. Attack 애니메이션 트리거
        animator.SetTrigger("Attack01");
        rb.AddForce(Vector2.down * fallSpeed, ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// 중거리공격의 돌진로직
    /// </summary>
    /// <returns></returns>
    public IEnumerator DashAttack()
    {
        float dashDirection = bossTranform.transform.localScale.x; // 보스 몬스터가 바라보는 방향
        Vector2 dashVector = new Vector2(dashDirection, 0) * dashSpeed; // 돌진 벡터
        
        float dashDuration = 0.65f; // 돌진 지속 시간
        float dashTime = 0f; // 돌진 시간 추적
    
        while (dashTime < dashDuration)
        {
            rb.MovePosition(rb.position + dashVector * Time.fixedDeltaTime);
            dashTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); // FixedUpdate가 호출될 때까지 대기
        }

        // 돌진이 끝난 후 위치 보정
        rb.velocity = Vector2.zero;
    }


    /// <summary>
    /// 투사체 발사
    /// </summary>
    public void FarAttack_Shoot()
    {
        // 투사체 인스턴스 생성
        var bulletGO = Instantiate(bullet.gameObject, shootPoint.position, Quaternion.identity);
        // 투사체 localScale 설정
        bulletGO.transform.localScale = facingRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        var rb = bulletGO.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 투사체 발사 방향 설정
            Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
            // 투사체 속도 적용
            rb.velocity = shootDirection * shootSpeed;
        }

        // 투사체의 스크립트 추가 설정
        var comp = bulletGO.GetComponent<EnemyBullet>();
        if (comp != null)
        {
            comp.bossAttackScript = this;
        }
    }

    public void Boss00_DoDamage()
    {
        float xOffset = facingRight ? -1 : 1;
        Vector2 hitboxCenter = new Vector2(bossTranform.transform.position.x + (xOffset * eventKeyController.hitBoxOffset_X), bossTranform.transform.position.y + eventKeyController.hitBoxOffset_Y);
        Collider2D[] hitBox = Physics2D.OverlapBoxAll(hitboxCenter, eventKeyController.hitBoxSize,0f);
        
        for (int i = 0; i < hitBox.Length; i++)
        { 
            if (hitBox[i].gameObject != null && hitBox[i].CompareTag("Player"))
            {
                hitBox[i].gameObject.GetComponent<PlayerHitHandler>().Player_ApplyDamage(damage, true, facingRight);
            }
        }
    }

    public void PlayDustParticle()
    {
        dustParticle.Play();
    }
}
