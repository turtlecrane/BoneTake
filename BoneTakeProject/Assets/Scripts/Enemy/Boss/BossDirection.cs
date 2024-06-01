using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BossDirection : MonoBehaviour
{
    public Slider hpSlider;
    public GameObject bossTitle;
    public Animator bossGFX;
    public GameObject door;
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
        door.transform.DOLocalMoveY(0f, 1).SetEase(Ease.InExpo);
        StartDirection();
        yield return TransitionScreen(0.9f);
        ActivateBossUI();
        yield return new WaitForSeconds(3.6f);
        EndWakeDirection();
    }

    public IEnumerator DeadDirection()
    {
        door.transform.DOLocalMoveY(10f, 1).SetEase(Ease.InExpo);
        StartDirection();
        yield return TransitionScreen(0.9f);
        EndBossLife();
        yield return new WaitForSeconds(3f);
        DeactivateBossUI();
        InitDirection();
    }

    private void StartDirection()
    {
        isDirecting = true;
        CharacterController2D.instance.isBossDirecting = true;
        CharacterController2D.instance.playerHitHandler.isInvincible = true;
        bossCamera.gameObject.SetActive(true);
    }

    private IEnumerator TransitionScreen(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    private void ActivateBossUI()
    {
        bossTitle.SetActive(true);
        hpSlider.gameObject.SetActive(true);
        bossGFX.SetBool("IsWake", true);
    }

    private void EndWakeDirection()
    {
        bossGFX.GetComponentInParent<BossHitHandler>().isInvincible = false;
        bossGFX.SetBool("IsWake", false);
        InitDirection();
    }

    private void EndBossLife()
    {
        hpSlider.GetComponent<Animator>().SetBool("IsEnd", true);
        bossGFX.SetBool("IsDead", true);
    }

    private void DeactivateBossUI()
    {
        hpSlider.gameObject.SetActive(false);
    }

    private void InitDirection()
    {
        bossCamera.gameObject.SetActive(false);
        bossTitle.SetActive(false);
        isDirecting = false;
        gameObject.SetActive(false);
        CharacterController2D.instance.isBossDirecting = false;
        CharacterController2D.instance.playerHitHandler.isInvincible = false;
    }
}

