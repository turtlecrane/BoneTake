using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Component")]
    public EnemyAI enemyAIScript;
    [HideInInspector]public Animator animator;
    
    [Header("Setting Value")]
    public int damage;
    public bool isCritDamage;
    public float attackCoolTime;
    
    [Space(10.0f)]
    public Vector2 attackRangeBoxSize;
    public float attackRangeOffset_X;
    public float attackRangeOffset_Y;
    
    [Space(10.0f)]
    public Vector2 hitBoxSize; //공격이 맞는 히트박스의 크기 조절
    public float hitBoxOffset_X;//공격 X축 반경을 조절
    public float hitBoxOffset_Y; //공격 Y축 반경을 조절
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
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
