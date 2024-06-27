using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy02_AI : EnemyAI_Flight
{
    public float upwardForce; // 위로 가할 힘의 크기

    private void FixedUpdate()
    {
        if (!isAttacking && enemyHitHandler.life > 0 && canMove) //평시
        {
            if (rb.velocity.y <= -0.5f)
            {
                //위로 힘을 가함
                rb.AddForce(Vector2.up * Random.Range(0f, 1.5f), ForceMode2D.Impulse);
            }
            if (rb.velocity.y >= 10f)
            {
                //StartCoroutine(DecelerateToZero(rb,1f));
                rb.velocity = Vector2.zero;
            }
            
            if (closeToGround)
            {
                rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
            }
        }
        /*else if(isAttacking) //공격중일때
        {
            if (isGrounded)
            {
                //StartCoroutine(DecelerateToZero(rb,0.5f));
            }
        }*/
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canAttack = true;
            // 플레이어가 들어온 위치를 체크
            enemyAttack.playerPosition = other.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canAttack = false;
            // 플레이어 위치 초기화
            enemyAttack.playerPosition = Vector3.zero;
        }
    }

    public void PlayBloodParticle()
    {
        enemyHitHandler.PlayBloodParticle();
    }
}
