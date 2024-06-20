using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    [Header("Component")] 
    public DumpedWeapon dumpedWeapon;
    
    [Header("State")]
    public bool isUsed;
    public bool isTrigger;
    
    private Rigidbody2D rb;
    private bool isNailed = false;
    private CharacterController2D charCon2D;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        charCon2D = CharacterController2D.instance;
    }
    
    void Update()
    {
        if (!isNailed)
        {
            transform.right = rb.velocity;
        }
        
        float zRotation = transform.eulerAngles.z;
        // zRotation이 90도 초과 또는 270도 미만일 경우 반전
        if (zRotation > 90 && zRotation < 270)
        {
            transform.localScale = new Vector3(1, -1, 1); // 반전
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // 원래 방향
        }

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        int collisionLayer = collision.gameObject.layer;
        string layerName = LayerMask.LayerToName(collisionLayer);

        if (layerName == "Wall" || layerName == "Ground")
        {
            //dumpedWeapon.gameObject.tag = "DumpedWeapon"; //오브젝트의 태그를 버려진 무기로 바꿈
            gameObject.tag = "DumpedWeapon"; //오브젝트의 태그를 버려진 무기로 바꿈
            isNailed = true;
            rb.isKinematic = true; // 물리적 영향을 받지 않도록 설정
            rb.velocity = Vector2.zero; // 벽이나 땅에 박힘
            isUsed = true;
            
        }
        else if (layerName == "Enemy_Standing" || layerName == "Enemy_Flight" || collision.CompareTag("Boss"))
        {
            if (!isUsed) //땅에 박힌 상태면 데미지를 안줌
            {
                DamageEnemy(collision);
            }
        }
    }
    
    private void DamageEnemy(Collider2D collision)
    {
        WeaponData weaponData = WeaponData.instance;
        int damage = weaponData.GetName_DamageCount(dumpedWeapon.weaponName) + charCon2D.playerdata.playerATK;

        Component hitHandler = collision.GetComponent<EnemyHitHandler>() ?? (Component)collision.GetComponentInParent<BossHitHandler>();

        if (hitHandler != null && !IsCorpseState(hitHandler))
        {
            AudioManager.instance.PlaySFX("Hit");
            hitHandler.gameObject.SendMessage("Enemy_ApplyDamage_Throw", damage);
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
