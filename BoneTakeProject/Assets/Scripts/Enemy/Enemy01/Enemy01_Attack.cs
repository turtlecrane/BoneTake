using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Enemy01_Attack : MonoBehaviour
{
    public EnemyAI enemyAIScript;
    public int damage;
    public bool isCritDamage;
    
    public Vector2 attackRangeBoxSize;
    public float attackRangeOffset_X;
    public float attackRangeOffset_Y;
    
    public Vector2 hitBoxSize; //공격이 맞는 히트박스의 크기 조절
    public float hitBoxOffset_X;//공격 X축 반경을 조절
    public float hitBoxOffset_Y; //공격 Y축 반경을 조절

    [Header("State")] 
    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckAttackEnable();
    }
    
    /// <summary>
    /// 공격 가능 상태인지 검사 (공격범위에 들어왔는지)
    /// </summary>
    private void CheckAttackEnable()
    {
        if (enemyAIScript.canTracking && !enemyAIScript.enemyHitHandler.isCorpseState)
        {
            float xOffset = enemyAIScript.facingRight ? 1 : -1;
            bool playerFound = false; // 이번 프레임에서 플레이어를 찾았는지 여부
            
            //공격 가능 범위 사각형의 중심 위치를 정의
            Vector2 boxCenter = new Vector2(transform.position.x + (xOffset * attackRangeOffset_X), transform.position.y + attackRangeOffset_Y);
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, attackRangeBoxSize, 0, enemyAIScript.playerLayer);

            foreach (var hit in hits)
            {
                if (hit.gameObject.CompareTag("Player"))
                {
                    playerFound = true; // 플레이어가 범위 안에 있다면 playerFound를 true로 설정
                    if (!enemyAIScript.canAttack)
                    {
                        enemyAIScript.canAttack = true; // 상태 업데이트
                        animator.SetBool("IsAttacking", true);
                    }
                    break; // 플레이어를 찾았으니 루프 종료
                }
            }

            // 플레이어가 이전 프레임에서는 범위 안에 있었지만, 이번 프레임에서는 범위 안에 없는 경우
            if (enemyAIScript.canAttack && !playerFound)
            {
                enemyAIScript.canAttack = false; // 상태 업데이트
                animator.SetBool("IsAttacking", false);
            }
        }
    }

    public void Enemy01_DoDamage()
    {
        float xOffset = enemyAIScript.facingRight ? 1 : -1;
        Vector2 hitboxCenter = new Vector2(transform.position.x + (xOffset * hitBoxOffset_X), transform.position.y + hitBoxOffset_Y);
        Collider2D[] hitBox = Physics2D.OverlapBoxAll(hitboxCenter,hitBoxSize,0f);
        
        for (int i = 0; i < hitBox.Length; i++)
        {
            if (hitBox[i].gameObject != null && hitBox[i].CompareTag("Player"))
            {
                hitBox[i].gameObject.GetComponent<PlayerHitHandler>().Player_ApplyDamage(damage, isCritDamage, enemyAIScript.facingRight);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        //공격 가능 범위 사각형을 에디터 상에서 표시
        float xOffset = enemyAIScript.facingRight ? 1 : -1;
        Vector2 attackRangeboxCenter = new Vector2(transform.position.x + (xOffset*attackRangeOffset_X), transform.position.y + attackRangeOffset_Y);
    
        Gizmos.color = Color.blue;
    
        Gizmos.DrawWireCube(attackRangeboxCenter, attackRangeBoxSize);
    }

    
    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시
        float xOffset = enemyAIScript.facingRight ? 1 : -1;
        Vector2 hitboxCenter = new Vector2(transform.position.x + (xOffset * hitBoxOffset_X), transform.position.y + hitBoxOffset_Y);
        
        Gizmos.color = Color.cyan;
        
        Gizmos.DrawWireCube(hitboxCenter, hitBoxSize);
    }
}
