using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameOverUI : MonoBehaviour
{
    public GameObject playerSystem;
    private Image panel;
    public TMP_Text title;
    public TMP_Text mainBtn;
    public TMP_Text reGameBtn;
    public Image fade;
    
    private void OnEnable()
    {
        panel = gameObject.GetComponent<Image>();
        StartCoroutine(GameOver());
    }

    public void GameOver_MainTitleBtn()
    {
        StartCoroutine(FadeOut(() =>
        {
            LoadingSceneController.LoadScene("MainTitle");
            //gameObject.SetActive();
        }));
    }
    
    public void GameOver_ReGameBtn()
    {
        StartCoroutine(FadeOut(() =>
        {
            PlayerDataManager.instance.LoadData();
            LoadingSceneController.LoadScene(PlayerDataManager.instance.nowPlayer.mapName);
            Destroy(playerSystem);
        }));
    }
    
    IEnumerator GameOver()
    {
        panel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        title.DOFade(1f, 1f);
        mainBtn.gameObject.SetActive(true);
        reGameBtn.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        mainBtn.DOFade(0.5f, 1f);
        reGameBtn.DOFade(0.5f, 1f);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator FadeOut(Action action = null)
    {
        fade.gameObject.SetActive(true);
        fade.DOFade(1f, 1f);
        yield return new WaitForSeconds(1.5f);
        //Init();
        action?.Invoke();
    }
    
    private void Init()
    {
        Color color = panel.color;
        color.a = 0f;
        panel.color = color;
        title.alpha = 0f;
        mainBtn.alpha = 0f;
        mainBtn.gameObject.SetActive(false);
        reGameBtn.alpha = 0f;
        reGameBtn.gameObject.SetActive(false);
        fade.color = color;
        fade.gameObject.SetActive(false);
    }
}
