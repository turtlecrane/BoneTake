using System;
using System.Collections;
using System.Collections.Generic;
using HeathenEngineering.PhysKit;
using Unity.Mathematics;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isNailed = false;
        
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isNailed)
        {
            transform.up = rb.velocity;
        }
    }
    
    public void OnPathEnd(float2 velocity)
    {
        Invoke(nameof(TimeKill), 5);
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
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            CharacterController2D charCon2D = GameManager.Instance.GetCharacterController2D();
            WeaponData weaponData = GameManager.Instance.GetWeaponData();
            EnemyHitHandler enemyHitHandler = collision.GetComponent<EnemyHitHandler>();
            if (!enemyHitHandler.isCorpseState)
            {
                //타격 설정
                collision.gameObject.SendMessage("Enemy_ApplyDamage", weaponData.GetName_DamageCount(charCon2D.playerAttack.weapon_name)+charCon2D.playerdata.playerATK);
                //파괴
                Destroy(this.gameObject);
            }
        }
    }
}
