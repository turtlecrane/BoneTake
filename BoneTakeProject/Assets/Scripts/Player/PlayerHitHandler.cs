using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

public class PlayerHitHandler : MonoBehaviour
{
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
    private PostProcessVolume hitVignette;
    private HitShake hitShakeScript;

    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        hitShakeScript = GameManager.Instance.GetPlayerFollowCameraController().virtualCamera.GetComponent<HitShake>();
        hitVignette = GameManager.Instance.GetPlayerFollowCameraController().mainCamera.GetComponent<PostProcessVolume>();
    }
    
    private void Update()
    {
        if (charCon2D.playerdata.playerHP <= 0)
        {
            isDead = true;
            Debug.Log("[사망 상태]");
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
        
        //카메라 흔들기
        hitShakeScript.HitScreenShake();
        
        //비네트 효과 적용
        hitVignette.weight = 1f;
        StartCoroutine(FadeOutVignette());
        
        float critical_knockbackForce = facingRight ? Critcal_knockbackForce+knockbackModifier : -(Critcal_knockbackForce+knockbackModifier);
        float basic_knockbackForce = facingRight ? Basic_knockbackForce+knockbackModifier : -(Basic_knockbackForce + knockbackModifier);
        
        if (criticalHit)
        {
            //넉백피격시
            charCon2D.animator.SetTrigger("Hit_KnockBack");
            charCon2D.m_Rigidbody2D.AddForce(new Vector2(critical_knockbackForce, 0)); // 넉백 적용
            isBigKnockBack = true;
            StartCoroutine(KnockbackEndCoroutine());
        }
        else
        {
            //일반피격시
            charCon2D.animator.SetTrigger("Hit");
            charCon2D.m_Rigidbody2D.AddForce(new Vector2(basic_knockbackForce, 0)); // 넉백 적용
        }
        
        charCon2D.playerdata.playerHP -= damage;
        charCon2D.m_Rigidbody2D.gravityScale = 5;//점프시 공격받으면 생기는 버그 fix
        charCon2D.m_Rigidbody2D.velocity = Vector2.zero;
        
        StartCoroutine(HitTime(criticalHit));
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
            GameManager.Instance.GetDevSetting().Dev_WorldTime = 0.5f;
            charCon2D.canMove = false;
            charCon2D.canDash = false;
            charCon2D.canDashAttack = false;
            charCon2D.playerAttack.canAttack = false;
            isInvincible = true;
            yield return new WaitUntil(() => isBigKnockBack == false); //넉백 끝날때동안 무적상태 + 움직일수없음 + 공격불가
            charCon2D.canMove = true;
            charCon2D.canDash = true;
            charCon2D.canDashAttack = true;
            charCon2D.playerAttack.canAttack = true;
            isInvincible = false;
            GameManager.Instance.GetDevSetting().Dev_WorldTime = 1f;
        }
        else
        {
            isSmallKnockBack = true;
            isInvincible = true;
            charCon2D.canMove = false;
            yield return new WaitForSeconds(hitInvincibleTime); //1초동안 무적상태
            charCon2D.canMove = true;
            isInvincible = false;
            isSmallKnockBack = false;
            charCon2D.animator.SetBool("IsKnockBackEnd", true);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collisionCount = 0f;
            EnemyAI enemyScript = collision.gameObject.GetComponent<EnemyAI>();
            if (!enemyScript.enemyHitHandler.isCorpseState)
            {
                Player_ApplyDamage(enemyScript.enemyAttack.damage, false, !charCon2D.m_FacingRight);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
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
