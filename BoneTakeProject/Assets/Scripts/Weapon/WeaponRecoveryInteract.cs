using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponRecoveryInteract : InteractableObject
{
    [Header("WeaponRecovery Value ---- ")] 
    public DialogueGraph firstSaveDialogue;
    public DialogueGraph checkDialogue;
    
    private bool isRecovered = false;
    private DialoguePlayback playback;
    public Light2D spotlight;

    public void NpcInteraction(DialoguePlayback _dialoguePlayback)
    {
        if (!isRecovered)
        {
            if (!talkEnable) //RecoveryStatue 로직
            {
                if (CharacterController2D.instance.playerAttack.weapon_type != Weapon_Type.Basic ||
                    CharacterController2D.instance.playerAttack.weaponManager.weaponLife != WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName))
                {
                    RecoveryStatue();
                }
            }
            else //방부업자 로직
            {
                playback = _dialoguePlayback;
                _dialoguePlayback.gameObject.SetActive(true);
                if (!PlayerDataManager.instance.nowPlayer.wpRecovryTuto)
                {
                    _dialoguePlayback.PlayDialogue(firstSaveDialogue);
                    PlayerDataManager.instance.nowPlayer.wpRecovryTuto = true;
                    isRecovered = true;
                }
                else if (CharacterController2D.instance.playerAttack.weapon_type == Weapon_Type.Basic)
                {
                    _dialoguePlayback.PlayDialogue(dialogue[0]);
                    isRecovered = false;
                }
                else
                {
                    //1. 무기체크하는 다이얼로그 실행
                    _dialoguePlayback.PlayDialogue(checkDialogue);
                }
            }
            
            //TestCode...
            //gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            Debug.Log("1회만 회복 가능합니다.");
        }
    }

    public void FullRecoveryWeapon()
    {
        AudioManager.instance.PlaySFX("WeaponRepair");
        CharacterController2D.instance.playerAttack.weaponManager.weaponLife = WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName);
    }

    public void CheckWpState()
    {
        StartCoroutine(PlayWpCheackDialogue());
    }

    private IEnumerator PlayWpCheackDialogue()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (CharacterController2D.instance.playerAttack.weaponManager.weaponLife == WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName))
        {
            //0.5초 대기 후 실행
            playback.gameObject.SetActive(true);
            playback.PlayDialogue(dialogue[1]);
            isRecovered = false;
        }
        else
        {
            //0.5초 대기 후 실행
            playback.gameObject.SetActive(true);
            playback.PlayDialogue(dialogue[2]);
            isRecovered = true;
        }
    }

    public void RecoveryStatue()
    {
        int maxWeaponHp = 0;
        int nowWeaponHp = 0;
        int recoveryValue = 0;
        
        AudioManager.instance.PlaySFX("WeaponRepair");
        //착용중인 무기의 최대 무기HP 가져오기
        maxWeaponHp = WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName);
            
        //현재 착용중인 무기의 현재 무기HP 가져오기
        nowWeaponHp = PlayerDataManager.instance.nowPlayer.weaponHP;
            
        recoveryValue = Mathf.FloorToInt((maxWeaponHp - nowWeaponHp) / 2.0f);
        Debug.Log("maxWeaponHp : " + maxWeaponHp + ", nowWeaponHp : " + nowWeaponHp + ", recoveryValue : " + recoveryValue );
            
        CharacterController2D.instance.playerAttack.weaponManager.weaponLife += recoveryValue;
        isRecovered = true;
        if(spotlight != null) DOTween.To(()=> spotlight.intensity , x=> spotlight.intensity  = x, 0f, 1f);
        Destroy(gameObject.GetComponent<Collider2D>());
    }
}
