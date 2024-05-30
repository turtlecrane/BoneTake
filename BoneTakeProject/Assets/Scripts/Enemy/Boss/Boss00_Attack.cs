using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss00_Attack : BossAttack
{
    [Header("Boss00 Variables")] 
    public ParticleSystem dustParticle;
    public Boss00_EventKeyController eventKeyController;
    public Transform shootPoint;
    public EnemyBullet bullet;
    public float shootSpeed;
    
    private Transform player;
    
    void Update()
    {
        isAttacking = IsCurrentAnimationTag("attack");
        
        // 플레이어 오브젝트 찾기
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (player != null && !bossHitHandler.isStun && !bossHitHandler.isCorpseState && !bossDirection.isDirecting && !CharacterController2D.instance.playerHitHandler.isDead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            Vector3 playerDirection = player.position - transform.position;

            if (!isAttacking)
            {
                // 플레이어 방향을 바라보도록 설정
                if (playerDirection.x < 0)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                    facingRight = false;
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    facingRight = true;
                }
                
                if (distanceToPlayer <= closeAttackDistance) 
                    animator.SetBool("Attack01", true);
                else if (closeAttackDistance < distanceToPlayer && distanceToPlayer < farAttackDistance)
                    animator.SetBool("Attack02", true);
                else if (farAttackDistance <= distanceToPlayer)
                    animator.SetBool("Attack03", true);
            }
        }
    }

    /// <summary>
    /// 중거리공격의 돌진로직
    /// </summary>
    /// <returns></returns>
    public IEnumerator DashAttack()
    {
        float dashDirection = transform.localScale.x; // 보스 몬스터가 바라보는 방향
        Vector3 dashVector = new Vector3(dashDirection, 0, 0); // 돌진 벡터

        float dashDuration = 0.65f; // 돌진 지속 시간
        float dashTime = 0f; // 돌진 시간 추적
        
        while (dashTime < dashDuration)
        {
            transform.Translate(dashVector * dashSpeed * Time.deltaTime, Space.World);
            dashTime += Time.deltaTime;
            yield return null;
        }
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
        Vector2 hitboxCenter = new Vector2(transform.position.x + (xOffset * eventKeyController.hitBoxOffset_X), transform.position.y + eventKeyController.hitBoxOffset_Y);
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
