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

public class MainTitle : MonoBehaviour
{
    public Button[] buttons;

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

    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
