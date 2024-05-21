using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class Enemy01_AI : EnemyAI
{ 
    public LayerMask groundLayer;
    public float groundCheckDistance = 1f; // Ground 체크를 위한 거리
    
    private void Update()
    {
        animator.SetBool("IsRunning", isRunning);
        isAttacking = IsCurrentAnimationTag("attack");
        
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
        // 이동 최대 속도 조정
        float clampedSpeed = Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed);
        rb.velocity = new Vector2(clampedSpeed, rb.velocity.y);
        CheckForGroundAhead();
        CheckisGround();
        Flip();
        
        if (path == null && !canTracking)
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
            }
            CalculatePathDirection();
            if (canTracking && canMove && !charCon2D.playerHitHandler.isDead) EnemyMoving();
            else if (!canTracking && !charCon2D.playerHitHandler.isDead)
            {
                NonTrackingAutoMove();
            }
            if (isAttacking) StartCoroutine(EnemyAttackCoolDown());
        }
    }
    
    /// <summary>
    /// 앞쪽에 땅이 있는지 검사
    /// </summary>
    private void CheckForGroundAhead()
    {
        float xOffset = facingRight ? 1.2f : -1.2f;
        Vector2 position = (Vector2)enemyTrackingPosition.position + new Vector2(xOffset, 0);

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(position, Vector2.down * groundCheckDistance, Color.yellow);

        if (hit.collider != null)
        {
            canMove = !canAttack;
            return;
        }

        rb.velocity = Vector2.zero;
        facingRight = !facingRight;
        isRunning = false;
        canMove = canAttack ? canMove : false;
    }
}
