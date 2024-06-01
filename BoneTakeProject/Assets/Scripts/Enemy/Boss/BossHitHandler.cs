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
        if (nowLife <= 0 && !isCorpseState)
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
        if (isCorpseState || isInvincible) return;

        CharacterController2D charCon2D = CharacterController2D.instance;
        Debug.Log($"보스가 공격당함. \n 데미지 : {damage} \n 플레이어의 무기 hp : {charCon2D.playerAttack.weaponManager.weaponLife}");

        hpText.gameObject.transform.DOShakePosition(0.3f, 50f, 50);
        damageParticle.Play();
        Blink();
        nowLife -= damage;

        if (nowLife <= 0)
        {
            PlayerDataManager.instance.nowPlayer.killedTypeOfBosses.Add(gameObject.name);
        }

        if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic && 
            charCon2D.playerAttack.weapon_type != Weapon_Type.etc)
        {
            charCon2D.playerAttack.weaponManager.weaponLife -= 1;
        }
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
        yield return new WaitForSeconds(5f);
        isStun = false;
        bossAttack.animator.SetBool("IsStun", false);
    }
}
