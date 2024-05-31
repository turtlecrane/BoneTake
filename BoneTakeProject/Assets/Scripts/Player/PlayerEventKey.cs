using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 애니메이션에서 호출되는 이벤트 함수 모음
/// </summary>
public class PlayerEventKey : MonoBehaviour
{
     /// <summary>
    /// 기본 공격의 데미지를 주는 함수
    /// </summary>
    public void Player_DoBasicDamege()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        charCon2D.playerAttack.attackParticle.Play();
        float damage = charCon2D.playerdata.playerATK;
        Vector2 hitBoxPosition = new Vector2(transform.position.x + (charCon2D.m_FacingRight ? 1 : -1) * charCon2D.playerAttack.playerOffset_X, transform.position.y + 1 + charCon2D.playerAttack.playerOffset_Y);
        ApplyDamage(hitBoxPosition, charCon2D.playerAttack.hitBoxSize, damage);
    }

    public void Player_DoKnifeDamage()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        WeaponData weaponDataScript = WeaponData.instance;
        charCon2D.playerAttack.attackParticle.Play();
        float damage = weaponDataScript.GetName_DamageCount(charCon2D.playerAttack.weapon_name) + charCon2D.playerdata.playerATK;
        Vector2 hitBoxPosition = new Vector2(transform.position.x + (charCon2D.m_FacingRight ? 1 : -1) * charCon2D.playerAttack.weaponManager.playerOffset_X, transform.position.y + 1 + charCon2D.playerAttack.weaponManager.playerOffset_Y);
        ApplyDamage(hitBoxPosition, charCon2D.playerAttack.weaponManager.hitBoxSize, damage);
    }

    private void ApplyDamage(Vector2 hitBoxPosition, Vector2 hitBoxSize, float damage)
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxPosition, hitBoxSize, 0f);
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject != null && (collider.CompareTag("Enemy") || collider.CompareTag("Boss")))
            {
                string methodName = "Enemy_ApplyDamage";
                if (collider.CompareTag("Enemy"))
                {
                    collider.gameObject.SendMessage(methodName, damage);
                }
                else if (collider.CompareTag("Boss"))
                {
                    collider.gameObject.GetComponentInParent<BossHitHandler>().gameObject.SendMessage(methodName, damage);
                }
            }
        }
    }
    
    /// <summary>
    /// 플레이어가 이동 가능하도록 함
    /// </summary>
    public void EnablePlayerMovement()
    {
        CharacterController2D.instance.canMove = true;
    }
    
    /// <summary>
    /// 플레이어를 조작불가 상태로 만듬
    /// </summary>
    public void DisablePlayerMovement()
    {
        CharacterController2D.instance.canMove = false;
        CharacterController2D.instance.m_Rigidbody2D.velocity = Vector2.zero;
    }
    
    //착지했을때 착지애니메이션 이 종료되면 착지상태를 false 상태로 만듬
    public void ExitPlayerLanding()
    {
        CharacterController2D.instance.isLanding = false;
        CharacterController2D.instance.isBigLanding = false;
    }
}
