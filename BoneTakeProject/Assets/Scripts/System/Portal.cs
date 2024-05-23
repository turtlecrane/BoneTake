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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            /*StartCoroutine(GameManager.Instance.GetInGameUiManager().FadeInFadeOut(() =>
            {
                
            }));*/
            StartCoroutine(FadeOut());
        }
    }
    
    public IEnumerator FadeOut()
    {
        Debug.Log("페이드 아웃");
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName);
        /*fadePanel.DOFade(0f, 1f);
        fadePanel.gameObject.SetActive(false);*/
    }
}
