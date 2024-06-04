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
            player = Instantiate(playerSystemPrefab, SpawnPoint.position, SpawnPoint.rotation);
        }
        else
        {
            return;
        }
    }

    public IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;

        fadePanel.DOFade(0, 1f);
        yield return new WaitForSeconds(1.1f);
        fadePanel.gameObject.SetActive(false);
    }
}
