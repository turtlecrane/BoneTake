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
        SceneManager.LoadScene(sceneName);
    }
}
