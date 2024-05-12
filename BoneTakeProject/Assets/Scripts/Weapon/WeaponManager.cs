using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeathenEngineering.PhysKit;
using HeathenEngineering.PhysKit.API;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon_Type weaponType;
    [SerializeField] private Weapon_Name weaponName;
    
    public int weaponLife;
    [HideInInspector] public Vector2 hitBoxSize; //공격이 맞는 무기의 히트박스의 크기 조절
    [HideInInspector] public float playerOffset_X;//공격 X축 무기 반경을 조절
    [HideInInspector] public float playerOffset_Y; //공격 Y축 무기 반경을 조절

    public Animator weaponGetEffectAnimator;
    public SpriteRenderer weaponGetEffectSprite;
    
    private CharacterController2D charCon2D;
    private WeaponData weaponDataScript;
    private Animator weaponAnimator;
    
    //TESTCODE
    public Transform projector;
    public Transform emitter;
    public TrickShot2D trickShot;
    
    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        weaponDataScript = GameManager.Instance.GetWeaponData();
        weaponAnimator = GetComponent<Animator>();
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
        
        weaponAnimator.SetBool("IsWp01", weaponName == Weapon_Name.Wp01);
        weaponAnimator.SetBool("IsWp02", weaponName == Weapon_Name.Wp02);

        if (charCon2D.playerAttack.isAiming)
        {
            Aim();
            if (Input.GetMouseButtonDown(0))
            {
                weaponAnimator.SetTrigger("Wp02_attack_Aiming_Shot");
                trickShot.Shoot();
            }
        }
        else
        {
            AimingInit();
        }
    }

    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시 2.125f : -2.125f;
        Gizmos.color = Color.magenta;
        float xOffset = GameManager.Instance.GetCharacterController2D().m_FacingRight ? 1 : -1;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (xOffset * playerOffset_X), transform.position.y + 1f + playerOffset_Y), hitBoxSize);
    }
    
    private void Aim()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ballistics.Solution2D(emitter.position, trickShot.speed, mousePos, Physics2D.gravity.magnitude, out Quaternion low, out Quaternion _);
        trickShot.distance = 20f;
        projector.rotation = new Quaternion(low.x,low.y,low.z,low.w);
    }

    private void AimingInit()
    {
        trickShot.distance = 0f;
        float parentScaleX = projector.transform.parent.localScale.x;
        float rotationZ = parentScaleX == -1 ? 90 : -90;
        projector.rotation = Quaternion.Euler(0, 0, rotationZ);
    }

    public void Shot_BasicArrow()
    {
        trickShot.Shoot();
    }

    public void WeaponGetEffect(Weapon_Name weaponName)
    {
        weaponGetEffectAnimator.gameObject.SetActive(true);
        weaponGetEffectSprite.sprite = weaponDataScript.weaponGFXSource.freshIcon[weaponDataScript.GetName_WeaponID(weaponName)];
        weaponGetEffectAnimator.SetTrigger("IsAcquisite");
    }
}
