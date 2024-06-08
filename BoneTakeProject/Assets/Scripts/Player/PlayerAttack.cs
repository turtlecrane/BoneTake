using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    [Header("Component")]
    public Animator weaponAnimator;
    public WeaponManager weaponManager;
    public ParticleSystem attackParticle;
    public InGameUiManager inGameUiManager;
    
    [Header("Value")]
    public Weapon_Type weapon_type; //현재 착용중인 무기
    public Weapon_Name weapon_name;
    [HideInInspector] public Vector2 hitBoxSize; //공격이 맞는 히트박스의 크기 조절
    [HideInInspector] public float playerOffset_X;//공격 X축 반경을 조절
    [HideInInspector] public float playerOffset_Y; //공격 Y축 반경을 조절
    public float multiAtk_maxTime;
    
    [Header("State")]
    public bool canAttack = true; //공격을 할 수 있는 상태인지
    public bool isAbleMultipleAttack; //다중타수가 가능한 상태인지 판별
    public bool isAttacking; //공격중인지
    public bool isJumpAttacking; //공격중인지
    public bool isAiming; //조준중인지 (활 무기 착용시)
    public int count = 0; //현재 공격이 몇타째 인지
    
    private float AbleMultipleAttack_Time;
    private float comparisonTimer;
    private bool previousIsAttacking = false; // isAttacking의 이전 상태를 추적하기 위한 변수
    private CharacterController2D charCon2D; 
    private WeaponData weaponDataScript;
    
    void Start()
    {
        charCon2D = GetComponent<CharacterController2D>();
        weaponDataScript = WeaponData.instance;
        
        //플레이어 캐릭터 컨트롤러에서 현재 플레이어가 착용중인 무기 정보를 가져옴
        weapon_type = charCon2D.playerdata.weaponType;
        weapon_name = charCon2D.playerdata.weaponName;
    }

    void Update()
    {
        isAttacking = IsCurrentAnimationTag("attack");
        isJumpAttacking = IsCurrentAnimationTag("jumpAttack");
        charCon2D.animator.SetBool("IsAttacking", isAttacking || isJumpAttacking);
        charCon2D.animator.SetBool("IsBowAiming",isAiming);
        
        charCon2D.canMove = 
            !isAttacking && 
            !isJumpAttacking &&
            !charCon2D.isBossDirecting &&
            !charCon2D.isBigLanding &&
            !charCon2D.playerHitHandler.isDead &&
            !charCon2D.playerHitHandler.isBigKnockBack &&
            !charCon2D.playerHitHandler.isSmallKnockBack &&
            !inGameUiManager.CheckForActiveUILayer(LayerMask.GetMask("UI")); //&&
            
        if (isAttacking) charCon2D.m_Rigidbody2D.velocity = new Vector2(0, charCon2D.m_Rigidbody2D.velocity.y);
        
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0) && canAttack && charCon2D.canMove)
        {
            //타격 시 카운팅 시작
            count = Mathf.Min(count + 1, weaponDataScript.GetType_AttackCount(weapon_type)); // count를 1 증가시키되, 무기 최대 타수를 초과하지 않도록 함
            
            isAbleMultipleAttack = true;
            
            if (weapon_type == Weapon_Type.Basic)//착용중인 무기가 기본 무기인경우 (손톱)
            {
                Player_BasicAttack();
            }
            else if (weapon_type == Weapon_Type.Knife)
            {
                Player_KnifeAttack();
            }
            else if (weapon_type == Weapon_Type.Bow)
            {
                if (charCon2D.m_Grounded)
                {
                    if (!isAiming) Player_BowBasicAttack();
                }
            }  
            else
            {
                Debug.Log("제작중인 무기 혹은 존재하지 않는 무기 종류입니다.");
            }
            
            canAttack = false;
            StartCoroutine(AttackCooldown());
        }
        
        // 조준중인지 확인
        if (charCon2D.m_Grounded && Input.GetAxis("Mouse ScrollWheel") < 0  && weapon_type == Weapon_Type.Bow)
        {
            isAiming = true;
        }
        else if(charCon2D.m_Grounded && Input.GetAxis("Mouse ScrollWheel") > 0  && weapon_type == Weapon_Type.Bow)
        {
            isAiming = false;
        }
        
        // 다중 공격 가능 상태 업데이트
        UpdateMultiAttackState(ref isAbleMultipleAttack, ref AbleMultipleAttack_Time, multiAtk_maxTime);
    }
    
    /// <summary>
    /// 공격 상태를 업데이트하는 함수
    /// </summary>
    /// <param name="extraDuration">연속 공격 가능 상태에 대해 추가 시간</param>
    void UpdateMultiAttackState(ref bool stateFlag, ref float timer, float extraDuration = 0)
    {
        //이동중이면 다중 공격이 아닌 첫타로 구분 
        if (charCon2D.playerMovement.horizontalMove != 0)
        {
            InitMultiAttackState(ref stateFlag, ref timer);
            return;
        }

        //공격중이며 이전에 공격한적이 없다면 타이머 시작(트리거)
        if ((isAttacking || isJumpAttacking) && !previousIsAttacking)
        {
            timer = 0;
            stateFlag = true;
            previousIsAttacking = true;
        }
        
        //다중공격 가능시간으로 다중공격이 가능한 상태인지 판단
        if (stateFlag)
        {
            comparisonTimer += Time.deltaTime;
            if (isAttacking || isJumpAttacking)
            {
                timer += Time.deltaTime;
            }
            else
            {
                //각 무기에 해당하는 마지막타수의 경우에는 추가시간을 기다리지 않고 바로 다중 공격 상태 종료
                if ((count == weaponDataScript.GetType_AttackCount(weapon_type) && comparisonTimer > timer) ||
                    (count != weaponDataScript.GetType_AttackCount(weapon_type) && comparisonTimer > timer + extraDuration))
                {
                    InitMultiAttackState(ref stateFlag, ref timer);
                }
            }
        }
    }
    
    /// <summary>
    /// 다중 타수를 판단하는 속성들 초기화
    /// </summary>
    void InitMultiAttackState(ref bool _stateFlag, ref float _timer)
    {
        _stateFlag = false;
        previousIsAttacking = false;
        _timer = 0;
        comparisonTimer = 0;
        count = 0;
    }
    
    /// <summary>
    /// 플레이어 공격 - 기본 무기(손톱)
    /// </summary>
    public void Player_BasicAttack()
    {
        // 기본 공격 모션으로 전환
        charCon2D.animator.SetTrigger("IsBasicAttacking");
        charCon2D.m_Rigidbody2D.gravityScale = 5;
    
        // count가 2 이상일 때만 Num of Hits 설정
        if (count == weaponDataScript.GetType_AttackCount(weapon_type))
        {
            charCon2D.animator.SetInteger("Num of Hits", count);
        }
    }

    /// <summary>
    /// 플레이어 공격 - 단검 공격
    /// </summary>
    public void Player_KnifeAttack()
    {
        // 단검 공격 모션으로 전환
        charCon2D.animator.SetTrigger("IsKnifeAttacking");
        charCon2D.m_Rigidbody2D.gravityScale = 5;
    
        // count가 2 이상일 때만 Num of Hits 설정
        if (count == weaponDataScript.GetType_AttackCount(weapon_type))
        {
            charCon2D.animator.SetInteger("Num of Hits", count);
        }
    }

    /// <summary>
    /// 플레이어 공격 - 활 기본공격
    /// </summary>
    public void Player_BowBasicAttack()
    {
        // 활 공격 모션으로 전환
        charCon2D.animator.SetTrigger("IsBowAttacking");
        charCon2D.m_Rigidbody2D.gravityScale = 5;
    }
    
    /// <summary>
    /// Animator에서 현재 재생 중인 애니메이션의 태그를 판단
    /// </summary>
    /// <returns></returns>
    bool IsCurrentAnimationTag(string tag)
    {
        AnimatorStateInfo stateInfo = charCon2D.animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(tag);
    }
    
    /// <summary>
    /// 플레이어 공격의 쿨타임을 결정
    /// </summary>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시 2.125f : -2.125f;
        Gizmos.color = Color.cyan;
        float xOffset = CharacterController2D.instance.m_FacingRight ? 1 : -1;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (xOffset * playerOffset_X), transform.position.y + 1f + playerOffset_Y), hitBoxSize);
    }
}