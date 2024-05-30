using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public bool isUsed;
    private Rigidbody2D rb;
    private bool isNailed = false;
    public BossAttack bossAttackScript;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Invoke("TimeKill", 10);
    }
    
    void TimeKill()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
           isNailed = true;
           rb.isKinematic = true; // 물리적 영향을 받지 않도록 설정
           rb.velocity = Vector2.zero; // 선택적으로 속도를 0으로 설정
           isUsed = true;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerHitHandler playerHitHandler = collision.gameObject.GetComponent<PlayerHitHandler>();
            if (!isUsed)
            {
                playerHitHandler.Player_ApplyDamage(bossAttackScript.damage, true, bossAttackScript.facingRight);
            }
            //파괴
            Destroy(this.gameObject);
        }
    }
}
