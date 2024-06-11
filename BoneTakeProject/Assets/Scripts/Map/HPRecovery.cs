using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPRecovery : MonoBehaviour
{
    //public int recoveryValue;
    
    public void NpcInteraction()
    {
        PlayerData playerData = CharacterController2D.instance.playerdata;
        if (playerData.playerMaxHP != playerData.playerHP)
        {
            if (playerData.playerHP <= 1) AudioManager.instance.StopAndRemoveEnvironSound("HeartBeat");
            AudioManager.instance.PlaySFX("HPRecovery");
            playerData.playerHP = playerData.playerMaxHP;
            Debug.Log( $"최대치만큼 모두 회복됨.");
        }
        else
        {
            Debug.Log("이미 모두 회복된 상태");
        }
        
    }
}
