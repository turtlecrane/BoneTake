using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject optionPopup;
    public bool isPaused = false; // 일시정지 상태를 추적
    public Image fade;

    public void PauseM_ContinueBtn()
    {
        isPaused = false;
        gameObject.SetActive(false);
    }
    
    public void PauseM_OptionBtn()
    {
        optionPopup.SetActive(true);
    }
    
    public void PauseM_MainTitleBtn()
    {
        PopupManager popup = GameManager.Instance.GetPopupManager();
        popup.SetPopup("정말 메인화면으로 이동하시겠습니까? \n 저장하지 않은 데이터는 삭제됩니다.", false, 
            () =>
            {
                AudioManager.instance.StopAndRemoveEnvironSound("HeartBeat");
                popup.gameObject.layer = LayerMask.NameToLayer("Cursor");
                StartCoroutine(FadeOut(()=>{LoadingSceneController.LoadScene("MainTitle");}));
            },
            () => { });
    }
    
    public void PauseM_QuitBtn()
    {
        PopupManager popup = GameManager.Instance.GetPopupManager();
        popup.SetPopup("정말 게임을 종료하시겠습니까? \n 저장하지 않은 데이터는 삭제됩니다.", false, 
            () =>
            {
                popup.gameObject.layer = LayerMask.NameToLayer("Cursor");
                StartCoroutine(FadeOut(() =>
                {
                    #if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
                    #else
                    // 빌드된 어플리케이션에서 실행 중인 경우
                    Application.Quit();
                    #endif
                }));
            },
            () => { });
    }

    IEnumerator FadeOut(Action action = null)
    {
        gameObject.layer = LayerMask.NameToLayer("Cursor");
        fade.gameObject.SetActive(true);
        Color color = fade.color;
        color.a = 0f;
        fade.color = color;
        fade.DOFade(1f, 1f);
        yield return new WaitForSeconds(1.5f);
        action?.Invoke();
    }

    public void PlayButtonClickAudio()
    {
        AudioManager.instance.PlayButtonSound("ButtonClick");
    }

    public void PlayButtonHoverAudio()
    {
        AudioManager.instance.PlayButtonSound("ButtonHover");
    }
}
