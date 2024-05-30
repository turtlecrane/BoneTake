using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 다이얼로그에 사용되는 버튼기능 담당
/// </summary>
public class ChoiceButton : MonoBehaviour
{
    public TMP_Text contentText;
    public Button button;
    public UnityEvent<int> clickEvent = new ActivateChoiceIndexEvent();

    private class ActivateChoiceIndexEvent : UnityEvent<int> {
    }

    private void Awake () {
        button.onClick.AddListener(() => {
            clickEvent.Invoke(transform.GetSiblingIndex());
        });
    }
}
