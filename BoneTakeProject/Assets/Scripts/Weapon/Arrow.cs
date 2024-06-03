using System;
using System.Collections;
using System.Collections.Generic;
using HeathenEngineering.PhysKit;
using Unity.Mathematics;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public bool isUsed;
    public bool isTrigger;
    private Rigidbody2D rb;
    private bool isNailed = false;
    private CharacterController2D charCon2D;
        
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        charCon2D = CharacterController2D.instance;
    }

    private void Start()
    {
        isTrigger = charCon2D.playerAttack.isAiming;
        Invoke("TimeKill", 10); //활은 10초후 자동삭제
    }

    void Update()
    {
        if (!isNailed)
        {
            transform.up = rb.velocity;
        }
    }
    
    void TimeKill()
    {
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        int collisionLayer = collision.gameObject.layer;
        string layerName = LayerMask.LayerToName(collisionLayer);

        if (layerName == "Wall" || layerName == "Ground")
        {
            HandleWallOrGroundCollision();
        }
        else if (layerName == "Enemy" || collision.CompareTag("Boss"))
        {
            HandleEnemyCollision(collision);
        }
    }

    private void HandleWallOrGroundCollision()
    {
        if (!isTrigger)
        {
            isNailed = true;
            rb.isKinematic = true; // 물리적 영향을 받지 않도록 설정
            rb.velocity = Vector2.zero; // 선택적으로 속도를 0으로 설정
            isUsed = true;
        }
    }

    private void HandleEnemyCollision(Collider2D collision)
    {
        if (!isUsed)
        {
            DamageEnemy(collision);
            Destroy(gameObject);
        }
    }

    private void DamageEnemy(Collider2D collision)
    {
        WeaponData weaponData = WeaponData.instance;
        int damage = weaponData.GetName_DamageCount(charCon2D.playerAttack.weapon_name) + charCon2D.playerdata.playerATK;

        Component hitHandler = collision.GetComponent<EnemyHitHandler>() ?? (Component)collision.GetComponentInParent<BossHitHandler>();

        if (hitHandler != null && !IsCorpseState(hitHandler))
        {
            AudioManager.instance.PlaySFX("Hit");
            hitHandler.gameObject.SendMessage("Enemy_ApplyDamage", damage);
        }
    }

    private bool IsCorpseState(Component hitHandler)
    {
        if (hitHandler is EnemyHitHandler enemyHitHandler)
        {
            return enemyHitHandler.isCorpseState;
        }
        else if (hitHandler is BossHitHandler bossHitHandler)
        {
            return bossHitHandler.isCorpseState;
        }

        return false;
    }
}
