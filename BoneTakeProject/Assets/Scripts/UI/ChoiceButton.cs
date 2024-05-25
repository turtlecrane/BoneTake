using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
