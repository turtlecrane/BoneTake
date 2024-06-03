using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeathenEngineering.PhysKit;
using HeathenEngineering.PhysKit.API;

public class WeaponManager : MonoBehaviour
{
    public int weaponLife;
    public Texture2D aimingCursor;
    public Animator weaponGetEffectAnimator;
    public SpriteRenderer weaponGetEffectSprite;
    
    [HideInInspector] public Vector2 hitBoxSize; //공격이 맞는 무기의 히트박스의 크기 조절
    [HideInInspector] public float playerOffset_X;//공격 X축 무기 반경을 조절
    [HideInInspector] public float playerOffset_Y; //공격 Y축 무기 반경을 조절
    
    private Weapon_Type weaponType;
    private Weapon_Name weaponName;
    private CharacterController2D charCon2D;
    private WeaponData weaponDataScript;
    private Animator weaponAnimator;
    private float lastShootTime = 0f;
    private float shootCooldown = 0.5f; // 0.5초 쿨다운
    
    //TESTCODE
    public Transform projector;
    public Transform emitter;
    public TrickShot2D trickShot;
    
    private void Start()
    {
        charCon2D = CharacterController2D.instance;
        weaponDataScript = WeaponData.instance;
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

        WeaponDestructionEffect();
        
        weaponAnimator.SetBool("IsWp01", weaponName == Weapon_Name.Wp01);
        weaponAnimator.SetBool("IsWp02", weaponName == Weapon_Name.Wp02);

        if (charCon2D.playerAttack.isAiming)
        {
            Aim();
            if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + (weaponDataScript.GetName_BowAimCoolTime(weaponName)+shootCooldown))
            {
                charCon2D.playerEventKey.PlayBowShotAudio();
                StartCoroutine(AimingShotCoolDown());
                trickShot.Shoot();
                lastShootTime = Time.time;
            }
        }
        else
        {
            AimingInit();
        }
    }

    private IEnumerator AimingShotCoolDown()
    {
        weaponAnimator.SetBool("Wp02_attack_Aiming_Shot", true);
        yield return new WaitForSeconds(weaponDataScript.GetName_BowAimCoolTime(weaponName));
        weaponAnimator.SetBool("Wp02_attack_Aiming_Shot", false);
    }
    
    private void Aim()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ballistics.Solution2D(emitter.position, trickShot.speed, mousePos, Physics2D.gravity.magnitude, out Quaternion low, out Quaternion _);
        trickShot.distance = 50f;
        projector.rotation = low;

        float zRotation = projector.eulerAngles.z;

        // 캐릭터가 오른쪽을 보고 있고, zRotation이 180도 이하일 때, 또는 캐릭터가 왼쪽을 보고 있고, zRotation이 180도를 초과할 때
        bool shouldFlip = charCon2D.m_FacingRight ? zRotation > 180 : zRotation <= 180;

        // shouldFlip이 참이면 localScale의 x 값을 양수로, 거짓이면 음수로 설정
        projector.localScale = new Vector3(shouldFlip ? Mathf.Abs(projector.localScale.x) : -Mathf.Abs(projector.localScale.x), projector.localScale.y, projector.localScale.z);
    }

    private void AimingInit()
    {
        trickShot.distance = 0f;
        float parentScaleX = projector.transform.parent.localScale.x;
        float rotationZ = parentScaleX == -1 ? 90 : -90;
        projector.rotation = Quaternion.Euler(0, 0, rotationZ);
        projector.localScale = new Vector3(Mathf.Abs(projector.localScale.x), projector.localScale.y, projector.localScale.z);
        charCon2D.playerAttack.isAiming = false;
        weaponAnimator.SetBool("Wp02_attack_Aiming_End", true);
    }

    public void Shot_BasicArrow()
    {
        trickShot.Shoot();
    }

    /// <summary>
    /// 무기 습득 효과
    /// </summary>
    public IEnumerator WeaponGetEffect(Weapon_Name weaponName)
    {
        weaponGetEffectAnimator.gameObject.SetActive(true);
        weaponGetEffectSprite.sprite = weaponDataScript.weaponGFXSource.freshIcon[weaponDataScript.GetName_WeaponID(weaponName)];
        weaponGetEffectAnimator.SetTrigger("IsAcquisite");
        yield return new WaitForSeconds(2.0f);
        weaponGetEffectAnimator.gameObject.SetActive(false);
    }

    /// <summary>
    /// 무기파괴
    /// </summary>
    public void WeaponDestructionEffect()
    {
        if (weaponLife <= 0) 
        {
            charCon2D.playerAttack.isAiming = false;
            weaponLife = -1;
            charCon2D.playerAttack.weapon_type = Weapon_Type.Basic;
            charCon2D.playerAttack.weapon_name = Weapon_Name.Basic;
        }
    }
    
    //히트박스 에디터 상에서 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        float xOffset = CharacterController2D.instance.m_FacingRight ? 1 : -1;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (xOffset * playerOffset_X), transform.position.y + 1f + playerOffset_Y), hitBoxSize);
    }
}
