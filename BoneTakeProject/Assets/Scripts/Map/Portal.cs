using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    public Image fadePanel;
    public string sceneName;
    public Vector3 switchPosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            StartCoroutine(FadeOut(other));
        }
    }
    
    public IEnumerator FadeOut(Collider2D other)
    {
        //Debug.Log("페이드 아웃");
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        
        other.transform.position = switchPosition;
        
        // other의 하위 오브젝트 중 PlayerFoot을 찾아서 위치 초기화
        Transform playerFoot = other.transform.Find("PlayerFoot");
        if (playerFoot != null)
        {
            playerFoot.localPosition = Vector3.zero; // 위치를 (0, 0, 0)으로 초기화
        }
        
        SceneManager.LoadScene(sceneName);
    }
}
