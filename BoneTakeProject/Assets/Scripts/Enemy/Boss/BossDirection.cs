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
    
    private float player_lensOrtho_InitSize;//플레이어 화면 줌 초기값 저장

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
        //플레이어의 원래 줌 초기값을 저장해놓음 (보스가 사망하면 원래대로 되돌리기 위함)
        player_lensOrtho_InitSize = GameManager.Instance.GetPlayerFollowCameraController().lensOrtho_InitSize;
        StartDirection();
        yield return TransitionScreen(0.9f);
        PlayBossBGM();
        ActivateBossUI();
        yield return new WaitForSeconds(3.6f);
        EndWakeDirection();
    }

    public void PlayBossBGM()
    {
        StartCoroutine(AudioManager.instance.PlayBGM("boss_1",0, () =>
        {
            AudioManager.instance.bgmSource.loop = true;
        }));
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
        gameObject.SetActive(true);
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
        
        GameManager.Instance.GetPlayerFollowCameraController().lensOrtho_InitSize = 19f;
        StartCoroutine(GameManager.Instance.GetPlayerFollowCameraController().CameraTargetTimeZoomIn(1f, 19f));
        
        InitDirection();
    }

    private void EndBossLife()
    {
        hpSlider.GetComponent<Animator>().SetBool("IsEnd", true);
        
        GameManager.Instance.GetPlayerFollowCameraController().lensOrtho_InitSize = player_lensOrtho_InitSize;
        GameManager.Instance.GetPlayerFollowCameraController().virtualCamera.m_Lens.OrthographicSize = player_lensOrtho_InitSize;
        
        if (gameObject.activeSelf) StartCoroutine(AudioManager.instance.FadeOut(1f));
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

