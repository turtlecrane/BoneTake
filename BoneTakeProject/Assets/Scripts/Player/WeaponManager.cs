using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Component")] 
    public Animator wpAnimator;
    private CharacterController2D charCon2D;
    private Animator playerAnimator;
    private PlayerAttack playerAtkScript;

    [Header("TestState")] 
    public bool isWp01;
    public bool isWp02;
    public bool isWp01Attacking;
    public int numOfHit;
    
    private void Awake()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        playerAnimator = charCon2D.GetComponent<Animator>();
        playerAtkScript = charCon2D.playerAttack;
    }

    void Update()
    {
        wpAnimator.SetBool("IsWp01", playerAtkScript.weapon_type == Weapon_Type.Knife);
        wpAnimator.SetBool("IsWp02", playerAtkScript.weapon_type == Weapon_Type.Bow);
        wpAnimator.SetBool("IsWp01Attacking", playerAnimator.GetBool("IsWp01Attacking"));
        //wpAnimator.SetBool("IsWp02Attacking", playerAnimator.GetBool("IsWp02Attacking"));
        wpAnimator.SetInteger("Num of Hits", playerAnimator.GetInteger("Num of Hits"));
        
        
        //...TESTCODE
        isWp01 = playerAtkScript.weapon_type == Weapon_Type.Knife;
        isWp02 = playerAtkScript.weapon_type == Weapon_Type.Bow;
        isWp01Attacking = playerAnimator.GetBool("IsWp01Attacking");
        numOfHit = playerAnimator.GetInteger("Num of Hits");
    }
    
}
