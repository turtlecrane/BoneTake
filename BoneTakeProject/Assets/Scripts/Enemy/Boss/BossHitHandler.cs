using System;
using System.Collections;
using System.Collections.Generic;
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
    public bool isCorpseState; //시체상태인지
    public bool isExtracted; //발골완료된 상태인지
    public bool isInvincible; //무적상태인지 (기본값 true)
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
        hpBar.value = nowLife;
        hpText.text = $"{nowLife} / {maxLife}";
        if (nowLife <= 0 && !isCorpseState)
        {
            nowLife = 0;
            //보스의 죽음 로직
            StartCoroutine(BossKnockdown());
        }
        
        float oneThirdLife = maxLife / 3f;
        
        // nowLife가 특정 범위에 도달했을 때 stun을 적용
        if (nowLife <= 2 * oneThirdLife && lastCheckedLife > 2 * oneThirdLife)
        {
            StartCoroutine(StunCoroutine());
        }
        else if (nowLife <= oneThirdLife && lastCheckedLife > oneThirdLife)
        {
            StartCoroutine(StunCoroutine());
        }

        // 마지막으로 체크한 nowLife 값을 업데이트
        lastCheckedLife = nowLife;
    }

    public void Enemy_ApplyDamage(float damage)
    {
        if(isCorpseState || isInvincible) return;
        CharacterController2D charCon2D = CharacterController2D.instance;
        Debug.Log("보스가 공격당함. \n 데미지 : " + damage + "\n 플레이어의 무기 hp : " + charCon2D.playerAttack.weaponManager.weaponLife);

        hpText.gameObject.transform.DOShakePosition(0.3f, 50f, 50);
        
        damageParticle.Play();
        Blink();
        
        nowLife -= damage; 
        
        if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic 
            || charCon2D.playerAttack.weapon_type != Weapon_Type.etc)
        {
            charCon2D.playerAttack.weaponManager.weaponLife -=  1; //무기 HP를 1 줄임
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
        
        //보스 사망 연출 시작
        StartCoroutine(bossAttack.bossDirection.DeadDirection());
        
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(); //보스에게 붙어있는 콜라이더 컴포넌트 모두 가져오기
        
        // 모든 Collider2D 컴포넌트의 isTrigger를 true로 설정
        foreach (var collider in colliders) 
        {
            collider.isTrigger = true;
        }
        
        //보스 사망 연출 끝날때까지 대기
        yield return new WaitForSeconds(3.2f);
        
        //발골 가능 상태(시체상태)로 전환
        isCorpseState = true;
    }
    
    private IEnumerator StunCoroutine()
    {
        if (!isStun) // 이미 stun 상태가 아니면 실행
        {
            Debug.Log("스턴시작");
            isStun = true;
            yield return new WaitForSeconds(2f);
            isStun = false;
            Debug.Log("스턴끝");
        }
    }
}
