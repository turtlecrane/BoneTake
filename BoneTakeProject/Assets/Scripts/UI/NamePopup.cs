using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NamePopup : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button confirmButton;
    
    public bool NameMakeCheck()
    {
        return !string.IsNullOrWhiteSpace(inputField.text);
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
