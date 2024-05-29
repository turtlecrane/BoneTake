using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BossWakeTrigger : MonoBehaviour
{
    public GameObject bossTitle;
    public Animator bossGFX;
    public CinemachineVirtualCamera bossCamera;
    public bool isDirecting;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(WakeDirection());
        }
    }

    public IEnumerator WakeDirection()
    {
        //연출시작
        isDirecting = true;
        
        //플레이어 제어
        CharacterController2D.instance.isBossWake = true;
        CharacterController2D.instance.playerHitHandler.isInvincible = true;
        
        //화면전환
        bossCamera.gameObject.SetActive(true);
        //화면 전환까지 걸리는 시간
        yield return new WaitForSeconds(0.9f);
        
        bossTitle.gameObject.SetActive(true);
        bossGFX.SetTrigger("IsWake");
        yield return new WaitForSeconds(3.6f);
        InitWakeDirecting();
    }

    public void InitWakeDirecting()
    {
        bossCamera.gameObject.SetActive(false);
        bossTitle.gameObject.SetActive(false);
        isDirecting = false;
        gameObject.SetActive(false);
        CharacterController2D.instance.isBossWake = false;
        CharacterController2D.instance.playerHitHandler.isInvincible = false;
    }
    
    //public void 
}
