#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DG.Tweening;

public class MainTitle : MonoBehaviour
{
    public Button[] buttons;
    public Image fade;

    private void OnEnable()
    {
        fade.DOFade(0f, 1.5f).OnComplete(() =>
        {
            fade.gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        bool dataExists = false;
        
        //저장된 데이터가 존재하는지 판단
        for (int i = 0; i < 3; i++)
        {
            if (File.Exists(PlayerDataManager.instance.path + $"{i}")) dataExists = true;
        }
        
        // 데이터 존재 여부에 따라 titleText의 내용을 설정
        if (dataExists) // 1개라도 데이터가 있으면
        {
            buttons[1].interactable = true;
        }
        else // 데이터가 전혀 없으면
        {
            buttons[1].interactable = false;
        }
    }

    /// <summary>
    /// 데이터슬롯 진입 (버튼에서 호출)
    /// </summary>
    /// <param name="buttonType">진입하는 버튼의 종류) 0 : NewGame, 1 : Continue</param>
    public void GoDataSlot(int buttonType)
    {
        PlayerPrefs.SetInt("DataSlotEntryType", buttonType);
        SceneManager.LoadScene("DataSlot");
    }
    
    public void Quit()
    {
        // 에디터에서 실행 중인 경우
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        // 빌드된 어플리케이션에서 실행 중인 경우
        Application.Quit();
        #endif
    }
}
