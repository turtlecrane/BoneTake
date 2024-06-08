using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public Image fadePanel;
    public Transform SpawnPoint;
    public GameObject playerSystemPrefab;
    public string changeBGMName; //배경음을 바꿀건지 + 바꿀거면 무슨 배경음으로 바꿀건지
    
    private GameObject player;
    

    private void Awake()
    {
        StartCoroutine(FadeIn());
        if (!GameObject.FindWithTag("Player"))
        {
            player = Instantiate(playerSystemPrefab, SpawnPoint.position, SpawnPoint.rotation);
            //SceneManager.LoadScene(PlayerDataManager.instance.nowPlayer.mapName);
        }
        else
        {
            return;
        }
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
                Debug.Log(changeBGMName.Length);
                StartCoroutine(ChangeBGM());
            }
            else
            {
                Debug.Log(changeBGMName.Length);
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
        
        StartCoroutine(AudioManager.instance.PlayBGM(changeBGMName));
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
