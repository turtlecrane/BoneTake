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
        Vector2 hitBoxPosition =
            new Vector2(
                transform.position.x + (charCon2D.m_FacingRight ? 1 : -1) * charCon2D.playerAttack.playerOffset_X,
                transform.position.y + 1 + charCon2D.playerAttack.playerOffset_Y);
        ApplyDamage(hitBoxPosition, charCon2D.playerAttack.hitBoxSize, damage);
    }

    /// <summary>
    /// 단검류 무기로 데미지를 주는 함수
    /// </summary>
    public void Player_DoKnifeDamage()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        WeaponData weaponDataScript = WeaponData.instance;
        charCon2D.playerAttack.attackParticle.Play();
        float damage = weaponDataScript.GetName_DamageCount(charCon2D.playerAttack.weapon_name) +
                       charCon2D.playerdata.playerATK;
        Vector2 hitBoxPosition =
            new Vector2(
                transform.position.x + (charCon2D.m_FacingRight ? 1 : -1) *
                charCon2D.playerAttack.weaponManager.playerOffset_X,
                transform.position.y + 1 + charCon2D.playerAttack.weaponManager.playerOffset_Y);
        ApplyDamage(hitBoxPosition, charCon2D.playerAttack.weaponManager.hitBoxSize, damage);
    }
    
    private void ApplyDamage(Vector2 hitBoxPosition, Vector2 hitBoxSize, float damage)
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(hitBoxPosition, hitBoxSize, 0f);
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject != null && (collider.CompareTag("Enemy") || collider.CompareTag("Boss")))
            {
                AudioManager.instance.PlaySFX("Hit");
                string methodName = "Enemy_ApplyDamage";
                if (collider.CompareTag("Enemy"))
                {
                    collider.gameObject.SendMessage(methodName, damage);
                }
                else if (collider.CompareTag("Boss"))
                {
                    collider.gameObject.GetComponentInParent<BossHitHandler>().gameObject
                        .SendMessage(methodName, damage);
                }
            }
            else if (collider.gameObject != null && collider.CompareTag("BreakableObj"))
            {
                AudioManager.instance.PlaySFX("Hit");
                string methodName = "Obj_ApplyDamage";
                collider.gameObject.SendMessage(methodName, damage);
            }
        }
    }

    public void ForceResumeGravity()
    {
        CharacterController2D.instance.m_Rigidbody2D.gravityScale = CharacterController2D.instance.m_playerRigidGravity;
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

    public void PlayFootstepsAudio()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        string sfxName = "";

        if (charCon2D.inDefaultGround)
        {
            sfxName = charCon2D.inWater ? "WaterWalk" : "BasicWalk";
        }
        else if (charCon2D.inGrassGround)
        {
            sfxName = "GrassWalk";
        }
        else if (charCon2D.inHardGround)
        {
            sfxName = "HardWalk";
        }

        if (!string.IsNullOrEmpty(sfxName))
        {
            AudioManager.instance.PlaySFX(sfxName, Random.Range(0, 4));
        }
    }

    public void PlayDashAudio()
    {
        AudioManager.instance.PlaySFX("Dash", Random.Range(0, 3));
    }

    public void PlayBigLandingAudio()
    {
        AudioManager.instance.PlaySFX("Bomb", Random.Range(0,3));
    }

    public void PlayBowZoomAudio()
    {
        AudioManager.instance.PlaySFX("BowZoom");
    }

    public void PlayBowShotAudio()
    {
        AudioManager.instance.PlaySFX("BowShot");
    }

    public void PlayWeaponLongAudio()
    {
        AudioManager.instance.PlaySFX("WeaponLong", Random.Range(0, 2));
    }

    public void PlayWeaponShortAudio()
    {
        AudioManager.instance.PlaySFX("WeaponShort", Random.Range(0, 2));
    }

    public void PlayBoneTakingAudio()
    {
        AudioManager.instance.PlayEnvironSound("BoneTaking");
    }
}

