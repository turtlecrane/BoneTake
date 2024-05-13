using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBoolSetWeaponAnimation : StateMachineBehaviour
{
    [Header("무기 애니메이터에서 실행될 Bool Parameter 이름")]
    public string actionName = "";
    public bool boolState;
    
    // 애니메이션이 시작될 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 먼저 PlayerTarget 오브젝트를 찾기
        Transform playerTarget = animator.gameObject.transform.Find("ShotPoint");
    
        // PlayerTarget 하위에서 Weapon 오브젝트를 찾기
        WeaponManager weapon = null;
        if (playerTarget != null)
        {
            Transform weaponTransform = playerTarget.Find("Weapon");
            if (weaponTransform != null)
            {
                weapon = weaponTransform.GetComponent<WeaponManager>();
            }
        }

        if (weapon != null)
        {
            Animator weaponAnimator = weapon.GetComponent<Animator>();
            PlayerAttack playerAttackScript = animator.GetComponent<PlayerAttack>();
        
            // Weapon 애니메이터에 현재 "착용중인 무기 + 실행될 트리거의 이름" 트리거를 설정합니다.
            if (weaponAnimator != null)
            {
                weaponAnimator.SetBool( playerAttackScript.weapon_name + "_" + actionName, boolState);
            }
        }
    }
}
