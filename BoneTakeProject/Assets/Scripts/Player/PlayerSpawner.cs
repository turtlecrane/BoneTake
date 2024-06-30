using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public Image fadePanel;
    public Vector3 spawnPoint;
    public GameObject playerSystemPrefab;
    public string changeBGMName; //배경음을 바꿀건지 + 바꿀거면 무슨 배경음으로 바꿀건지
    
    //private GameObject player;
    

    private void Awake()
    {
        if (!GameObject.FindWithTag("Player"))
        {
            GameObject playerSystemInstance = Instantiate(playerSystemPrefab);
            Transform player = playerSystemInstance.transform.Find("Player");
            if (player != null)
            {
                player.transform.position = spawnPoint;
            }
            else
            {
                Debug.Log("인스턴스화된 프리팹에서 플레이어 개체를 찾지못함.");
            }
        }
        else
        {
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private void OnEnable()
    {
        //바꿀 브금 이름이 적혀있으면 브금을 바꿀거라고 판단
        if (changeBGMName.Length != 0)
        {
            if (changeBGMName == "Stop")
            {
                StartCoroutine(AudioManager.instance.FadeOut(1f)); 
                return;
            }
            
            if (AudioManager.instance.bgmSource.clip != null)
            {
                if (changeBGMName == AudioManager.instance.bgmSource.clip.name)
                {
                    return;
                }
                StartCoroutine(ChangeBGM());
            }
            else
            {
                StartCoroutine(ChangeBGM());
            }
        }
    }

    private IEnumerator ChangeBGM()
    {
        if (AudioManager.instance.isBGMChanging)
        { 
            yield return new WaitUntil(()=>!AudioManager.instance.isBGMChanging);
        }
        else
        { 
            StartCoroutine(AudioManager.instance.FadeOut(1f)); 
            yield return new WaitUntil(()=>!AudioManager.instance.isBGMChanging);
        }
        
        InvokeRepeating("InvokePlayInGameBGM", 1f, 300f);
    }
    
    private void InvokePlayInGameBGM()
    {
        Debug.Log("인게임 BGM 재생");
        StartCoroutine(AudioManager.instance.PlayBGM(changeBGMName));
    }

    public IEnumerator FadeIn()
    {
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;
        
        fadePanel.gameObject.SetActive(true);

        fadePanel.DOFade(0, 1f);
        Debug.Log(fadePanel.color.a);
        yield return new WaitForSeconds(1.1f);
        fadePanel.gameObject.SetActive(false);
    }
}
