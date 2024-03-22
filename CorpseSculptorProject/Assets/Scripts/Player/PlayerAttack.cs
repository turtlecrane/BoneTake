using System;
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
    public bool canAttack = true;
    public bool isAbleMultipleAttack; //다중타수가 가능한 상태인지 판별
    
    public float attackCount; //공격 애니메이션 카운트 세기
    private CharacterController2D playercCharacterController2D;
    
    //...TESTCODE
    [Header("테스트코드")]
    public int count = 0;
    public bool isAttacking;
    public float AbleMultipleAttack_Time;
    public float multiAtk_maxTime;
    //public float lastClickTime; // 마지막으로 클릭한 시간
    
    void Start()
    {
        playercCharacterController2D = GetComponent<CharacterController2D>();
        
        //플레이어 캐릭터 컨트롤러에서 현재 플레이어가 착용중인 무기 정보를 가져옴
        _weapon_type = playercCharacterController2D.playerdata.weaponType; 
    }

    void Update()
    {
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            //------- ...TEST CODE [보류중]
            if(!playercCharacterController2D.m_Grounded) return; //점프공격
            if(playercCharacterController2D.isDashing) return; //대쉬공격
            if(playercCharacterController2D.isClimbing) return; //벽타기때 공격하는경우
            //---------------------------
                
            isAttacking = true;
            isAbleMultipleAttack = true;
            
            if (_weapon_type == Weapon_Type.Basic)//착용중인 무기가 기본 무기인경우 (손톱)
            {
                Player_BasicAttack();
            }
            else
            {
                //TODO 이곳에 다른 무기도 추가
                Debug.Log("제작중인 무기 혹은 존재하지 않는 무기 종류입니다.");
            }
            
            //공격중이면 플레이어를 멈추게함.
            playercCharacterController2D.m_Rigidbody2D.velocity = Vector2.zero;
            playercCharacterController2D.canMove = false;
            
            canAttack = false;
            StartCoroutine(AttackCooldown());
        }

        // 공격 상태 업데이트
        UpdateAttackState(ref isAttacking, ref attackCount);
        // 연속 공격 가능 상태 업데이트
        UpdateAttackState(ref isAbleMultipleAttack, ref AbleMultipleAttack_Time, multiAtk_maxTime);

        if (!isAbleMultipleAttack)
        {
            count = 0;
            playercCharacterController2D.canMove = true;
        }
    }
    
    /// <summary>
    /// 공격 상태를 업데이트하는 함수
    /// </summary>
    /// <param name="extraDuration">연속 공격 가능 상태에 대해 추가 시간</param>
    void UpdateAttackState(ref bool stateFlag, ref float timer, float extraDuration = 0)
    {
        if (stateFlag)
        {
            timer += Time.deltaTime;
            if (timer > GetCurrentAnimationLength() + extraDuration)
            {
                timer = 0;
                stateFlag = false;
            }
        }
    }
    
    /// <summary>
    /// 플레이어 공격 - 기본 무기(손톱)
    /// </summary>
    public void Player_BasicAttack()
    {
        // 첫 타격 시 카운팅 시작
        count = Mathf.Min(count + 1, 2); // count를 1 증가시키되, 2를 초과하지 않도록 함

        // 기본 공격 모션으로 전환
        playercCharacterController2D.animator.SetBool("IsBasicAttacking", true);
    
        // count가 2 이상일 때만 Num of Hits 설정
        if (count == 2)
        {
            playercCharacterController2D.animator.SetInteger("Num of Hits", count);
        }
    }
    
    /// <summary>
    /// Animator에서 현재 재생 중인 애니메이션의 길이를 가져오기
    /// </summary>
    /// <returns>현재 애니메이션의 길이</returns>
    float GetCurrentAnimationLength()
    {
        //(인자는 레이어의 번호)
        AnimatorStateInfo stateInfo = playercCharacterController2D.animator.GetCurrentAnimatorStateInfo(0);
        // 애니메이션의 길이(시간) 반환
        float animationLength = stateInfo.length;
        return animationLength;
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
        float xOffset = playercCharacterController2D.m_FacingRight ? 2.125f : -2.125f;
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + xOffset, transform.position.y), new Vector2(2.25f, 2f), 0f);
        
        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null && basicHitBox[i].CompareTag("Enemy"))
            {
                //해당 오브젝트의 상태 스크립트에 접근해서 HP를 깎아야함.
                //HP를 줄이는건 0+데이터ATK로 깎는다.
                //0인이유는 기본공격이라서. 다른 무기들은 도끼) 3+ATK 이런식이다
                basicHitBox[i].gameObject.SendMessage("ApplyDamage", 0+playercCharacterController2D.playerdata.playerATK);
            }
        }
    }
    
    
    
    //<---------애니메이션에서 호출 (Key Event)--------->
    
    public void EnablePleyerMovement()
    {
        playercCharacterController2D.canMove = true;
    }

    public void DisablePleyerMovement()
    {
        playercCharacterController2D.canMove = false;
        playercCharacterController2D.m_Rigidbody2D.velocity = Vector2.zero;
    }

}