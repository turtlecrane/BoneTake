using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Component")] 
    public Transform bossTranform;
    public Rigidbody2D rb;
    public Animator animator;
    public BossDirection bossDirection;
    public BossHitHandler bossHitHandler;
    
    [Header("Value")]
    public int damage; //공격 데미지
    public float dashSpeed; //중거리 공격의 돌진 속도
    public float closeAttackDistance; //근거리 공격의 판단 거리
    public float farAttackDistance; //원거리 공격의 판단 거리
    
    [Header("State")]
    public bool isAttacking; //공격중인지
    public bool facingRight; //어느방향을 보고있는지 (기본값 true)

    private void Start()
    {
        foreach (string bossName in PlayerDataManager.instance.nowPlayer.killedTypeOfBosses)
        {
            if (bossName == gameObject.name)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public bool IsCurrentAnimationTag(string tag)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(tag);
    }
}
