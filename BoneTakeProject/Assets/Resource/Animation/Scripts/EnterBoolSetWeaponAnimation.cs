using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterBoolSetWeaponAnimation : StateMachineBehaviour
{
    [Header("무기 애니메이터에서 실행될 Bool Parameter 이름")]
    public string actionName = "";
    public bool boolState;
    
    // 애니메이션이 시작될 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Weapon 오브젝트를 찾기
        GameObject weapon = animator.gameObject.transform.Find("Weapon").gameObject;
        Animator weaponAnimator = weapon.GetComponent<Animator>();
        PlayerAttack playerAttackScript = animator.GetComponent<PlayerAttack>();
        
        // Weapon 애니메이터에 현재 "착용중인 무기 + 실행될 트리거의 이름" 트리거를 설정
        if (weaponAnimator != null)
        {
            weaponAnimator.SetBool( playerAttackScript.weapon_name + "_" + actionName, boolState);
        }
    }
}
