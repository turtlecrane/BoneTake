using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHitHandler : MonoBehaviour
{
    [Header("Component")]
    public ParticleSystem damageParticle;
    public ParticleSystem bloodParticle;
    public SpriteRenderer bossSprite;
    public BossAttack bossAttack;
    public Slider hpBar;

    [Header("State")]
    public bool isCorpseState;
    public bool isExtracted;
    public bool isInvincible = true;
    public bool isStun;

    [Header("Value")] 
    public Weapon_Name[] weaponName;
    public float maxLife;
    public float nowLife;
    public float boneExtractionTime;

    private TMP_Text hpText;
    private float lastCheckedLife;

    private void Awake()
    {
        nowLife = maxLife;
        hpBar.maxValue = maxLife;
        hpText = hpBar.GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        UpdateHealthBar();
        CheckBossLife();
        CheckStunCondition();
    }

    private void UpdateHealthBar()
    {
        hpBar.value = nowLife;
        hpText.text = $"{nowLife} / {maxLife}";
    }

    private void CheckBossLife()
    {
        if (nowLife <= 0 && !isCorpseState && !bossAttack.isAttacking && bossAttack.isGrounded)
        {
            nowLife = 0;
            StartCoroutine(BossKnockdown());
        }
    }

    private void CheckStunCondition()
    {
        float oneThirdLife = maxLife / 3f;

        if ((nowLife <= 2 * oneThirdLife && lastCheckedLife > 2 * oneThirdLife) ||
            (nowLife <= oneThirdLife && lastCheckedLife > oneThirdLife))
        {
            StartCoroutine(StunCoroutine());
        }

        lastCheckedLife = nowLife;
    }

    public void Enemy_ApplyDamage(float damage)
    {
        ApplyDamageEffects(damage);
        CharacterController2D charCon2D = CharacterController2D.instance;
        if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic && 
            charCon2D.playerAttack.weapon_type != Weapon_Type.etc)
        {
            charCon2D.playerAttack.weaponManager.weaponLife -= 1;
        }
    }
    
    public void Enemy_ApplyDamage_Throw(float damage) {
        ApplyDamageEffects(damage);
        /*CharacterController2D charCon2D = CharacterController2D.instance;
        if (charCon2D.playerAttack.weapon_type == Weapon_Type.Spear) {
            charCon2D.playerAttack.weaponManager.weaponLife -= 3; // 무기 HP를 3 줄임
        }*/
        // 추후 다른 투척물 무기가 생기면 이곳에 추가
    }

    private void ApplyDamageEffects(float damage)
    {
        if (isCorpseState || isInvincible) return;

        CharacterController2D charCon2D = CharacterController2D.instance;
        hpText.gameObject.transform.DOShakePosition(0.3f, 50f, 50);
        damageParticle.Play();
        Blink();
        nowLife -= damage;
        Debug.Log($"보스가 공격당함. \n 데미지 : {damage} \n 플레이어의 무기 hp : {charCon2D.playerAttack.weaponManager.weaponLife}");

        
        if (nowLife <= 0)
        {
            PlayerDataManager.instance.nowPlayer.killedTypeOfBosses.Add(gameObject.name);
        }
        StartCoroutine(HitTime()); //중복 피격 방지
    }
    
    IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.1f); //0.1초동안 무적상태
        isInvincible = false;
    }

    private void Blink()
    {
        bossSprite.color = Color.red;
        bossSprite.DOColor(Color.white, 0.5f);
    }

    public void PlayBloodParticle()
    {
        bloodParticle.Play();
    }

    private IEnumerator BossKnockdown()
    {
        if (!bossAttack.isGrounded) yield return new WaitUntil(() => bossAttack.isGrounded);
        if (bossAttack.isAttacking) yield return new WaitUntil(() => !bossAttack.isAttacking);
        //CharacterController2D.instance.m_Rigidbody2D.velocity = Vector2.zero;
        StartCoroutine(CharacterController2D.instance.DecelerateToZero(CharacterController2D.instance.m_Rigidbody2D, 0.1f));
        
        bossAttack.animator.SetBool("IsDead", true);
        isInvincible = true;
        StartCoroutine(bossAttack.bossDirection.DeadDirection());

        SetCollidersTrigger(true);
        yield return new WaitForSeconds(3.2f);

        isCorpseState = true;
    }

    private void SetCollidersTrigger(bool isTrigger)
    {
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.isTrigger = isTrigger;
        }
        bossAttack.rb.velocity = Vector2.zero;
        bossAttack.rb.gravityScale = 0f;
    }

    private IEnumerator StunCoroutine()
    {
        if (isStun) yield break;
        isStun = true;
        bossAttack.animator.SetBool("IsStun", true);
        yield return new WaitForSeconds(2f);
        isStun = false;
        bossAttack.animator.SetBool("IsStun", false);
    }
}
