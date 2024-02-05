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

    public bool isAbleMultipleAttack; //다중타수가 가능한 상태인지 판별
    
    public float attackCount; //공격하고 다시 공격할때까지 카운트 세기
    
    void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playercCharacterController2D = GetComponent<CharacterController2D>();
        
        //플레이어 캐릭터 컨트롤러에서 현재 플레이어가 착용중인 무기 정보를 가져옴
        _weapon_type = playercCharacterController2D.playerdata.weaponType; 
    }

    void Update()
    {
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            canAttack = false;
            StartCoroutine(AttackCooldown());
            if (_weapon_type == Weapon_Type.Basic)//착용중인 무기가 기본 무기인경우 (손톱)
            {
                Player_BasicAttack();
            }
            else
            {
                //TODO 이곳에 다른 무기도 추가
                Debug.Log("제작중인 무기 혹은 존재하지 않는 무기 종류입니다.");
            }
        }

        EnableMultiAttackMode();

    }

    /// <summary>
    /// 다중 공격이 가능한 상태가되면 정해진 카운트만큼 다중공격 가능상태로 전환 <br/>
    /// 공격하는 애니메이션 길이만큼 다중공격 가능상태로 간주
    /// </summary>
    public void EnableMultiAttackMode()
    {
        if (isAbleMultipleAttack)//업데이트에서 다중 타수가 가능한지 계속 추적하다가 다중타수가 가능한 상태가 되면
        {
            attackCount += Time.deltaTime;
            if (attackCount > GetCurrentAnimationLength())
            {
                isAbleMultipleAttack = false;
                attackCount = 0;
            }
        }
    }

    /// <summary>
    /// 플레이어 공격 - 기본 무기(손톱)
    /// </summary>
    public void Player_BasicAttack()
    {
        playercCharacterController2D.animator.SetBool("IsBasicAttacking", true); //기본공격 모션으로 전환
        if (isAbleMultipleAttack) //다중타수의 경우 -> 기본공격은 타수가 2타가 최대임
        {
            playercCharacterController2D.animator.SetInteger("Num of Hits", 1);
        }
        else
        {
            isAbleMultipleAttack = true; //다중타수 가능한 상태임을 알림
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
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x+2.125f, transform.position.y), new Vector2(2.25f, 2f), 0f);

        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null)
            {
                if (basicHitBox[i].CompareTag("Enemy"))
                {
                    //Debug.Log($"Enemy Detected, Name : {basicHitBox[i].gameObject.name}");
                    //해당 오브젝트의 상태 스크립트에 접근해서 HP를 깎아야함.
                    //HP를 줄이는건 0+데이터ATK로 깎는다.
                    //0인이유는 기본공격이라서. 다른 무기들은 도끼) 3+ATK 이런식이다
                    basicHitBox[i].gameObject.SendMessage("ApplyDamage", 0+playercCharacterController2D.playerdata.playerATK);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x+2.125f, transform.position.y), new Vector2(2.25f, 2f));
    }

}