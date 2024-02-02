using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Weapon_Type
{
    Basic,
    etc
}

public class PlayerAttack : MonoBehaviour
{
    public Weapon_Type _weapon_type;
    private Rigidbody2D m_Rigidbody2D;
    private CharacterController2D playercCharacterController2D;
    public bool canAttack = true;
    
    void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playercCharacterController2D = GetComponent<CharacterController2D>();
        _weapon_type = playercCharacterController2D.playerdata.weaponType;
    }

    void Update()
    {
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Debug.Log("왼쪽 마우스가 클릭됨");
            canAttack = false;
            StartCoroutine(AttackCooldown());
            if (_weapon_type == Weapon_Type.Basic)
            {
                Debug.Log("기본무기로 공격함.");
                playercCharacterController2D.animator.SetBool("IsBasicAttacking", true);
            }
            else
            {
                Debug.Log("제작중인 무기 혹은 존재하지 않는 무기 종류입니다.");
            }
        }
    }
    
    /// <summary>
    /// 플레이어 공격의 쿨타임을 결정
    /// </summary>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
    
    /// <summary>
    /// 애니메이션의 Event호출로 호출되는 플레이어 공격 함수
    /// </summary>
    public void Player_DoBasicDamege()
    {
        Debug.Log("플레이어 공격 함수 실행됨.");
    }
    
}