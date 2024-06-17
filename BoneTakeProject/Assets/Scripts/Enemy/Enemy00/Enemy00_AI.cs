using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class Enemy00_AI : EnemyAI
{
    
    private void Update()
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", !isGrounded);
        isAttacking = IsCurrentAnimationTag("attack");

        if (animator.GetBool("IsJumping") && isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }
        
        if (isRunning)
        {
            movingCount += Time.deltaTime;
        }
        else
        {
            movingCount = 0;
        }

        isLanding = rb.velocity.y < 0f;
        
        // movingCount가 1과 2사이일 때
        if (movingCount > randomMovingValue_MIN && movingCount <= randomMovingValue_MAX)
        {
            if (movingCount >= randomValue)
            {
                StartCoroutine(StopMoving());
            }
        }
    }

    void FixedUpdate()
    {
        // x축 최대 속도 조정
        float clampedSpeed = Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed);
        rb.velocity = new Vector2(clampedSpeed, rb.velocity.y);
        CheckisGround();
        if(!isGrounded && isLanding) CheckisLanding();
        Flip();
        
        if (path == null && !canTracking && enemyHitHandler.life > 0)
        {
            //무작위 이동
            NonTrackingAutoMove();
        }
        else if (path != null)
        {
            // 경로의 마지막에 도달했는지 여부를 설정
            reachedEndOfPath = currentWaypoint >= path.vectorPath.Count;
            if (reachedEndOfPath) return;
            
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
            if (distance < nextWaypointDistance)
            {
                if (currentWaypoint + 1 < path.vectorPath.Count) // 경로의 마지막 waypoint를 체크
                {
                    currentWaypoint++;
                }
                else
                {
                    // 경로의 마지막 waypoint에 도달했을 때의 처리를 여기에 추가
                    Debug.Log("적이 플레이어에 도달했거나, 경로 추적을 중단");
                    canMove = false;
                    canTracking = false;
                }
            }
            
            CalculatePathDirection();
            
            if (canTracking && canMove && !charCon2D.playerHitHandler.isDead && enemyHitHandler.life > 0)
            {
                EnemyMoving();
                EnemyJumping();
            }
            else if (!canTracking && !charCon2D.playerHitHandler.isDead && enemyHitHandler.life > 0)
            {
                NonTrackingAutoMove();
            }

            //점프 가능 상태라고 알림
            if (((Vector2)path.vectorPath[currentWaypoint] - rb.position).y >= 0.5f)
            {
                jumpEnabled = true;
            }

            if (isAttacking) StartCoroutine(EnemyAttackCoolDown());
        }
    }
}
