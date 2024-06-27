using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy02_Attack : EnemyAttack
{
    [Header("Enemy02_Attack ---")] 
    public float dashForce;
    public float refreshTime;
    
    private void Start()
    {
        // 1초마다 UpdatePlayerPosition 메서드 호출
        InvokeRepeating("UpdatePlayerPosition", refreshTime, refreshTime);
    }

    private IEnumerator Enemy02_Attacking()
    {
        if (enemyAIScript_F.closeToGround) //공격하려고했는데 바닥과 너무 가깝다면
        {
            // 공격중지 바닥과 멀어질 때까지 기다림
            yield return new WaitUntil(() => enemyAIScript_F.closeToGround == false);
            yield return new WaitForSeconds(0.5f);
        }
        
        enemyAIScript_F.isAttacking = true;
        yield return new WaitForSeconds(0.1f);

        // 공격 애니메이션 시작
        animator.SetTrigger("IsAttacking");
        
        yield return new WaitForSeconds(2.0f);
        //animator.SetTrigger("IsAttackingEnd");
        //StartCoroutine(enemyAIScript_F.DecelerateToZero(enemyAIScript_F.rb,0.5f)); //브레이크 밟기
        enemyAIScript_F.isAttacking = false;
    }
    
    /// <summary>
    /// 플레이어 위치를 갱신
    /// </summary>
    private void UpdatePlayerPosition()
    {
        if (enemyAIScript_F.canAttack && !enemyAIScript_F.isAttacking && enemyAIScript_F.canTracking && enemyAIScript_F.canMove)
        {
            //플레이어 위치 갱신
            playerPosition = enemyAIScript_F.target.position;
            
            // 플레이어 위치쪽으로 회전하기
            Vector3 direction = (playerPosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemyAIScript_F.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            
            // localScale 조정
            if (angle > 90 || angle < -90)
            {
                enemyAIScript_F.gameObject.transform.localScale = new Vector3(-1, -1, 1); // 상하 반전
            }
            else
            {
                enemyAIScript_F.gameObject.transform.localScale = new Vector3(1, 1, 1); // 원래 방향
            }
            
            if (enemyAIScript_F.canAttack)
            {
                StartCoroutine(Enemy02_Attacking());
            }
        }
    }

    public void Enemy02_Dash()
    {
        Vector2 dashDirection = enemyAIScript_F.gameObject.transform.right; // 2D에서는 transform.right가 바라보는 방향
        enemyAIScript_F.rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
    }
}
