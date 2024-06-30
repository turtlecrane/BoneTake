using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerHitHandler : MonoBehaviour
{
    public Image gameOverPanel;
    [Header("전투 관련 상태")] 
    public bool isDead; //사망상태 인지
    public bool isInvincible; //무적상태 인지
    public bool isSmallKnockBack;
    public bool isBigKnockBack; //특수 공격에 피격당한 상태인지
    public float hitInvincibleTime; //피격시 무적상태 시간 조절
    public float Basic_knockbackForce; //일반 피격시 넉백값
    public float Critcal_knockbackForce; //크리티컬 피격시 넉백값
    
    private CharacterController2D charCon2D;
    private float collisionCount;
    private Volume hitVignette;
    private HitShake hitShakeScript;
    private PlayerFollowCameraController followCameraController;

    private void Start()
    {
        charCon2D = CharacterController2D.instance;
        hitShakeScript = GameManager.Instance.GetPlayerFollowCameraController().virtualCamera.GetComponent<HitShake>();
        hitVignette = GameManager.Instance.GetPlayerFollowCameraController().hitVolume;
        followCameraController = GameManager.Instance.GetPlayerFollowCameraController();
        if (charCon2D.playerdata.playerHP == 1)
        {
            AudioManager.instance.PlayEnvironSound("HeartBeat");
        }
    }
    
    /// <summary>
    /// 플레이어의 피격(Hit) 액션
    /// </summary>
    /// <param name="facingRight">넉백되는 방향 - false = 왼쪽</param>
    /// <param name="knockbackModifier">넉백 보정값 - Nullable</param>
    public void Player_ApplyDamage(int damage, bool criticalHit, bool facingRight, float knockbackModifier = 0)
    {
        //살아있는 상태 or 무적상태가 아닐때만 피격가능
        if (isDead || isInvincible) return;
        
        charCon2D.playerdata.playerHP -= damage;
        if (charCon2D.playerdata.playerHP <= 0)
        {
            AudioManager.instance.bgmSource.Stop();
            AudioManager.instance.bgmSource.clip = null;
            AudioManager.instance.PlaySFX("Dead");
            AudioManager.instance.StopAndRemoveEnvironSound("HeartBeat");
            StartCoroutine(DeathCameraEffect());
            isDead = true;
            charCon2D.animator.SetBool("IsDead", isDead);
        }

        if (charCon2D.playerdata.playerHP == 1)
        {
            Debug.Log("피 1");
            AudioManager.instance.PlayEnvironSound("HeartBeat");
        }
        
        //조준중에 피격당하면 조준 풀림
        if (charCon2D.playerAttack.isAiming)
        {
            charCon2D.playerAttack.isAiming = false;
            charCon2D.animator.SetBool("IsBowAiming", false);
            charCon2D.playerAttack.weaponAnimator.SetBool($"{charCon2D.playerAttack.weapon_name}_attack_Aiming_End", true);
        }
        
        charCon2D.isClimbing = false;
        
        //카메라 흔들기
        hitShakeScript.HitScreenShake();
        
        AudioManager.instance.StopAndRemoveEnvironSound("BoneTaking");
        
        //무기 애니메이터에게 Hit상태 알리기
        charCon2D.playerAttack.weaponAnimator.SetTrigger("Hit");
        
        //비네트 효과 적용
        hitVignette.weight = 1f;
        StartCoroutine(FadeOutVignette());
        
        float critical_knockbackForce = facingRight ? Critcal_knockbackForce+knockbackModifier : -(Critcal_knockbackForce+knockbackModifier);
        float basic_knockbackForce = facingRight ? Basic_knockbackForce+knockbackModifier : -(Basic_knockbackForce + knockbackModifier);
        
        if (criticalHit)
        {
            //넉백피격시
            charCon2D.animator.SetTrigger("Hit_KnockBack");
            if (!isDead)
            {
                charCon2D.m_Rigidbody2D.AddForce(new Vector2(critical_knockbackForce, 0)); // 넉백 적용
            }
            isBigKnockBack = true;
            StartCoroutine(KnockbackEndCoroutine());
        }
        else
        {
            //일반피격시
            charCon2D.animator.SetTrigger("Hit");
            charCon2D.m_Rigidbody2D.AddForce(new Vector2(basic_knockbackForce, 0)); // 넉백 적용
        }
        StartCoroutine(HitTime(criticalHit));
        
        charCon2D.m_Rigidbody2D.gravityScale = 5;//점프시 공격받으면 생기는 버그 fix
        charCon2D.m_Rigidbody2D.velocity = Vector2.zero;
    }
    
    /// <summary>
    /// 비네트 효과 서서히 사라지게하기
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeOutVignette()
    {
        float duration = hitInvincibleTime; // Vignette 효과가 사라지는 데 걸리는 시간
        float currentTime = 0f;

        while (currentTime < duration)
        {
            hitVignette.weight = Mathf.Lerp(1f, 0f, currentTime / duration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        hitVignette.weight = 0f; // 최종적으로 Vignette 효과를 완전히 제거
    }
    
    IEnumerator KnockbackEndCoroutine()
    {
        yield return new WaitForSeconds(0.1f); // 넉백이 시작되고 나서 체크하기 전에 잠깐 기다리는 시간
        while (charCon2D.m_Rigidbody2D.velocity.magnitude > 0.2f) // Rigidbody2D의 속도가 거의 0에 가까워질 때까지 기다림
        {
            yield return null; // 다음 프레임까지 기다림
        }
        isBigKnockBack = false;
        charCon2D.animator.SetBool("IsKnockBackEnd", true);
    }

    /// <summary>
    /// 피격당했을때 무적상태 
    /// </summary>
    IEnumerator HitTime(bool _criticalHit)
    {
        if (_criticalHit)
        {
            charCon2D.canMove = false;
            charCon2D.canDash = false;
            charCon2D.canDashAttack = false;
            charCon2D.playerAttack.canAttack = false;
            isInvincible = true; //무적상태 시작
            yield return new WaitUntil(() => isBigKnockBack == false); //넉백 끝날때동안 움직일수없음 + 공격불가
            charCon2D.canMove = true;
            charCon2D.canDash = true;
            charCon2D.canDashAttack = true;
            charCon2D.playerAttack.canAttack = true;
            //isInvincible = false;
        }
        else
        {
            isSmallKnockBack = true;
            isInvincible = true; //무적상태 시작
            yield return new WaitForSeconds(0.25f); //0.25초 동안 조작 불가
            //isInvincible = false;
            isSmallKnockBack = false;
            charCon2D.animator.SetBool("IsKnockBackEnd", true);
        }
        //<- 무적상태가 끝날때까지 playerSR를 깜빡깜빡 거리게 만들기 Dotween 플러그인 사용
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        
        // DOTween을 사용하여 깜빡이는 효과 적용
        float blinkDuration = 0.2f; // 깜빡이는 시간 간격
        float totalBlinkTime = hitInvincibleTime; // 총 무적 시간
        int blinkCount = Mathf.CeilToInt(totalBlinkTime / blinkDuration); // 깜빡이는 횟수 계산

        // 원래 색상을 저장하고, 검정색으로 변경하는 루프 설정
        Color originalColor = playerSR.color;
        Color blackColor = Color.black;
        
        playerSR.DOColor(blackColor, blinkDuration)
            .SetLoops(blinkCount * 2, LoopType.Yoyo)
            .SetEase(Ease.Linear);

        
        yield return new WaitForSeconds(hitInvincibleTime); //1초동안 무적상태
        isInvincible = false;
        playerSR.DOKill(); // 깜빡이기 효과 중지
        playerSR.color = originalColor; // 색상 초기화
    }
    
    IEnumerator DeathCameraEffect()
    {
        float startTime = Time.unscaledTime;
        float duration = 3f; // 줌인과 시간 느려지는 효과에 걸리는 시간 (초)
        float targetOrthographicSize = 9f; // 목표 OrthographicSize
        GameManager.Instance.GetDevSetting().Dev_WorldTime = 0.5f; // 시간 느려지게 함

        while (Time.unscaledTime - startTime < duration)
        {
            float t = (Time.unscaledTime - startTime) / duration;
            followCameraController.virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(followCameraController.lensOrtho_InitSize, targetOrthographicSize, t);
            yield return null;
        }

        // 마지막으로 목표값으로 확실히 설정
        followCameraController.virtualCamera.m_Lens.OrthographicSize = targetOrthographicSize;

        // 추가 대기 시간 없이 바로 원래 시간 속도로 복구
        GameManager.Instance.GetDevSetting().Dev_WorldTime = 1f;
        gameOverPanel.gameObject.SetActive(true);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collisionCount = 0f;
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy_Standing"))
            {
                EnemyAI enemyScript = collision.gameObject.GetComponent<EnemyAI>();
                if (!enemyScript.enemyHitHandler.isCorpseState && !charCon2D.isDashAttacking)
                {
                    Player_ApplyDamage(enemyScript.enemyAttack.damage, false, !charCon2D.m_FacingRight);
                }

            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy_Flight"))
            {
                EnemyAI_Flight enemyScript = collision.gameObject.GetComponent<EnemyAI_Flight>();
                if (!enemyScript.enemyHitHandler.isCorpseState && !charCon2D.isDashAttacking)
                {
                    Player_ApplyDamage(enemyScript.enemyAttack.damage, false, !charCon2D.m_FacingRight);
                }
            }
        }
        if (collision.gameObject.CompareTag("Boss"))
        {
            collisionCount = 0f;
            BossAttack bossAttackScript = collision.gameObject.GetComponentInParent<BossAttack>();
            BossHitHandler bossHitHandler = collision.gameObject.GetComponentInParent<BossHitHandler>();
            if (!bossHitHandler.isCorpseState && !charCon2D.isDashAttacking)
            {
                Player_ApplyDamage(bossAttackScript.damage, false, !charCon2D.m_FacingRight);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            collisionCount += Time.deltaTime;
            if (collisionCount > 0.5f)
            {
                int randomDirection = Random.Range(-1, 2);
                charCon2D.m_Rigidbody2D.AddForce(new Vector2(randomDirection*(Basic_knockbackForce+1000), charCon2D.minJumpForce));
                collisionCount = 0f;
            }
        }
    }
}
