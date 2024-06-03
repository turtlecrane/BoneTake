using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public TMP_Text popupContent;
    public Button Button_Yes;
    public Button Button_No;
    
    /// <summary>
    /// 팝업창을 띄우는 함수
    /// </summary>
    /// <param name="_content">팝업 내용 Title</param>
    /// <param name="_isOpen">팝업을 열건지 닫을건지</param>
    /// <param name="_isSingle">버튼이 1개(true)인지 2(false)개인지</param>
    /// <param name="_yesButtonAction">예 버튼이 실행할 로직</param>
    /// <param name="_noButtonAction">아니오 버튼이 실행할 로직</param>
    public void SetPopup(string _content, bool _isSingle, Action _yesButtonAction, [CanBeNull] Action _noButtonAction)
    {
        popupContent.text = _content;
        gameObject.SetActive(true);
        
        if (gameObject.activeSelf)
        {
            if (_isSingle)
            {
                // _isSingle 조건일 때 Button_No를 비활성화하고, Button_Yes의 클릭 이벤트 설정
                Button_No.gameObject.SetActive(false);
            
                Button_Yes.onClick.RemoveAllListeners(); // 기존 이벤트 제거
                Button_Yes.onClick.AddListener(() => _yesButtonAction()); // 새 이벤트 추가
            }
            else
            {
                // _isSingle 조건이 아닐 때, Button_Yes와 Button_No 모두 활성화하고 각각의 클릭 이벤트 설정
                Button_No.gameObject.SetActive(true);
            
                Button_Yes.onClick.RemoveAllListeners(); // 기존 이벤트 제거
                Button_Yes.onClick.AddListener(() => _yesButtonAction()); // 새 이벤트 추가
            
                //아니오 버튼은 기본적으로 팝업창이 종료됨
                Button_No.onClick.RemoveAllListeners(); // 기존 이벤트 제거
                if (_noButtonAction != null)
                {
                    Button_No.onClick.AddListener(() => _noButtonAction()); // 새 이벤트 추가
                }
            }
        }
    }

    public void ClosePopup()
    {
        Button_Yes.onClick.RemoveAllListeners(); // 기존 이벤트 제거
        Button_No.onClick.RemoveAllListeners(); // 기존 이벤트 제거
        gameObject.SetActive(false);
    }

    public void PlayMouseClickAudio()
    {
        AudioManager.instance.PlayButtonSound("MouseClick");
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
