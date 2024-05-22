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
        charCon2D = CharacterController2D.instance; //GameManager.Instance.GetCharacterController2D();
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!isTrigger)
            {
                isNailed = true;
                rb.isKinematic = true; // 물리적 영향을 받지 않도록 설정
                rb.velocity = Vector2.zero; // 선택적으로 속도를 0으로 설정
                isUsed = true;
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            
            WeaponData weaponData = WeaponData.instance;
            EnemyHitHandler enemyHitHandler = collision.GetComponent<EnemyHitHandler>();
            if (!enemyHitHandler.isCorpseState)
            {
                if (!isUsed)
                {
                    //타격 설정
                    collision.gameObject.SendMessage("Enemy_ApplyDamage", weaponData.GetName_DamageCount(charCon2D.playerAttack.weapon_name)+charCon2D.playerdata.playerATK);
                }
                
                //파괴
                Destroy(this.gameObject);
            }
        }
    }
}
