using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon_Type weaponType;
    [SerializeField] private Weapon_Name weaponName;
    
    public int weaponLife;
    public Vector2 hitBoxSize; //공격이 맞는 무기의 히트박스의 크기 조절
    public float playerOffset_X;//공격 X축 무기 반경을 조절
    public float playerOffset_Y; //공격 Y축 무기 반경을 조절

    private CharacterController2D charCon2D;
    private Animator animator;

    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        animator = GetComponent<Animator>();
        weaponLife = charCon2D.playerdata.weaponHP;
    }

    private void Update()
    {
        weaponType = charCon2D.playerAttack.weapon_type;
        weaponName = charCon2D.playerAttack.weapon_name;
        
        charCon2D.playerdata.weaponType = charCon2D.playerAttack.weapon_type;
        charCon2D.playerdata.weaponName = charCon2D.playerAttack.weapon_name;
        charCon2D.playerdata.weaponHP = weaponLife;

        if (weaponLife <= 0) //무기파괴
        {
            weaponLife = -1;
            charCon2D.playerAttack.weapon_type = Weapon_Type.Basic;
            charCon2D.playerAttack.weapon_name = Weapon_Name.Basic;
        }
        
        animator.SetBool("IsKnife", weaponType == Weapon_Type.Knife);
        animator.SetBool("IsBow", weaponType == Weapon_Type.Bow);
        animator.SetBool("IsKnifeBunnyKnife", weaponName == Weapon_Name.KnifeBunnyKnife);
        animator.SetBool("IsBowow", weaponName == Weapon_Name.Bowowbow);
        animator.SetBool("IsinAir", charCon2D.isJumping || charCon2D.isFalling);
    }

    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시 2.125f : -2.125f;
        Gizmos.color = Color.magenta;
        float xOffset = GameManager.Instance.GetCharacterController2D().m_FacingRight ? 1 : -1;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (xOffset * playerOffset_X), transform.position.y + 1f + playerOffset_Y), hitBoxSize);
    }
}
