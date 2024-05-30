using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BossDirection : MonoBehaviour
{
    public Slider hpSlider;
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
        BossHitHandler hitHandler = bossGFX.GetComponentInParent<BossHitHandler>();
        //연출시작
        isDirecting = true;
        
        //플레이어 제어
        CharacterController2D.instance.isBossDirecting = true;
        CharacterController2D.instance.playerHitHandler.isInvincible = true;
        
        //화면전환
        bossCamera.gameObject.SetActive(true);
        //화면 전환까지 걸리는 시간
        yield return new WaitForSeconds(0.9f);
        
        bossTitle.gameObject.SetActive(true);
        hpSlider.gameObject.SetActive(true);
        bossGFX.SetBool("IsWake", true);
        yield return new WaitForSeconds(3.6f);
        //보스 무적상태 제거
        hitHandler.isInvincible = false;
        bossGFX.SetBool("IsWake", false);
        InitWakeDirecting();
    }

    public IEnumerator DeadDirection()
    {
        //연출시작
        isDirecting = true;
        
        //플레이어 제어
        CharacterController2D.instance.isBossDirecting = true;
        CharacterController2D.instance.playerHitHandler.isInvincible = true;
        
        //화면전환
        bossCamera.gameObject.SetActive(true);
        
        //화면 전환까지 걸리는 시간
        yield return new WaitForSeconds(0.9f);
        
        hpSlider.gameObject.GetComponent<Animator>().SetBool("IsEnd", true);
        //보스 사망 에니메이션 재생
        bossGFX.SetBool("IsDead", true);
        
        yield return new WaitForSeconds(3f);
        hpSlider.gameObject.SetActive(false);
        
        InitWakeDirecting();
    }

    public void InitWakeDirecting()
    {
        bossCamera.gameObject.SetActive(false);
        bossTitle.gameObject.SetActive(false);
        isDirecting = false;
        gameObject.SetActive(false);
        CharacterController2D.instance.isBossDirecting = false;
        CharacterController2D.instance.playerHitHandler.isInvincible = false;
    }
}
