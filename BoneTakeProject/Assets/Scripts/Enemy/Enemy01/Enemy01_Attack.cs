using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Enemy01_Attack : MonoBehaviour
{
    public EnemyAI enemyAIScript;
    public int damage;
    
    //public float attackCoolTime; 1.6 1.8
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

    void Update()
    {
        CheckAttackEnable();
        if (enemyAIScript.canAttack)
        {
            animator.SetTrigger("IsAttacking");
        }
    }
    
    /// <summary>
    /// 공격 가능 상태인지 검사 (공격범위에 들어왔는지)
    /// </summary>
    void CheckAttackEnable()
    {
        if (enemyAIScript.canTracking)
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
                    if (!enemyAIScript.canAttack) enemyAIScript.canAttack = true; // 상태 업데이트
                    break; // 플레이어를 찾았으니 루프 종료
                }
            }

            // 플레이어가 이전 프레임에서는 범위 안에 있었지만, 이번 프레임에서는 범위 안에 없는 경우
            if (enemyAIScript.canAttack && !playerFound) enemyAIScript.canAttack = false; // 상태 업데이트
        }
    }
    
    void OnDrawGizmos()
    {
        //공격 가능 범위 사각형을 에디터 상에서 표시
        float xOffset = enemyAIScript.facingRight ? 1 : -1;
        Vector2 boxCenter = new Vector2(transform.position.x + (xOffset*attackRangeOffset_X), transform.position.y + attackRangeOffset_Y);
    
        Gizmos.color = Color.blue;
    
        Gizmos.DrawWireCube(boxCenter, attackRangeBoxSize);
    }

    
    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시
        Gizmos.color = Color.cyan;
        float xOffset = enemyAIScript.facingRight ? 1 : -1;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (xOffset * hitBoxOffset_X), transform.position.y + hitBoxOffset_Y), hitBoxSize);
    }
}
