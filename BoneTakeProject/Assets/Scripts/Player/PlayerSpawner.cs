using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public Image fadePanel;
    public Transform SpawnPoint;
    public GameObject playerSystemPrefab;
    private GameObject player;

    private void Awake()
    {
        StartCoroutine(FadeIn());
        if (!GameObject.FindWithTag("Player"))
        {
            Debug.Log("플레이어가 존재하지않습니다. 플레이어를 생성합니다.");
            player = Instantiate(playerSystemPrefab, SpawnPoint.position, SpawnPoint.rotation);
        }
        else
        {
            Debug.Log("플레이어가 이미 존재합니다.");
            return;
        }
    }

    public IEnumerator FadeIn()
    {
        Debug.Log("페이드인");
        fadePanel.gameObject.SetActive(true);
        
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;

        fadePanel.DOFade(0, 1f);
        yield return new WaitForSeconds(1f);
        fadePanel.gameObject.SetActive(false);
    }
}
