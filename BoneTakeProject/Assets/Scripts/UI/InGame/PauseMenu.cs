using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject optionPopup;
    public bool isPaused = false; // 일시정지 상태를 추적

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
                LoadingSceneController.LoadScene("MainTitle");
            },
            () => { });
    }
    
    public void PauseM_QuitBtn()
    {
        PopupManager popup = GameManager.Instance.GetPopupManager();
        popup.SetPopup("정말 게임을 종료하시겠습니까? \n 저장하지 않은 데이터는 삭제됩니다.", false, 
            () =>
            {
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #else
                // 빌드된 어플리케이션에서 실행 중인 경우
                Application.Quit();
                #endif
            },
            () => { });
    }
}
