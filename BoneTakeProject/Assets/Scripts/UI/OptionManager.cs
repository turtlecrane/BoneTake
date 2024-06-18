using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [Header("비디오 설정 관련")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenBtn;
    private List<Resolution> resolutions = new List<Resolution>();
    private int resolutionNum;
    private FullScreenMode screenMode;

    [Header("텍스트 설정 관련")] 
    public Toggle slow;
    public Toggle normal;
    public Toggle fast;


    [Header("음향 설정 관련")] 
    public Toggle bgmMute;
    public Toggle sfxMute;
    public Toggle envMute;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider envSlider;
    
    void Start()
    {
        Init_UI();
        Init_TextSpeed_ToggleState();
        Init_AudioValue();
    }

    private void Init_UI()
    {
        resolutions.AddRange(Screen.resolutions);
        resolutionDropdown.options.Clear();
        
        int optionNum = 0;
        
        foreach (Resolution item in resolutions)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = item.width + " X " + item.height + " - " + item.refreshRate + "hz";
            resolutionDropdown.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height) resolutionDropdown.value = optionNum;
            optionNum++;
        }
        resolutionDropdown.RefreshShownValue();

        fullscreenBtn.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;
    }
    
    private void Init_TextSpeed_ToggleState()
    {
        // PlayerPrefs에서 저장된 Toggle 상태 불러오기
        float savedToggle = PlayerPrefs.GetFloat("textSpeedSelected", 0.03f);

        // 저장된 값에 따라 해당 Toggle 활성화
        if(savedToggle == 0.1f)
        {
            slow.isOn = true;
        }
        else if(savedToggle == 0.03f)
        {
            normal.isOn = true;
        }
        else if(savedToggle == 0.01f)
        {
            fast.isOn = true;
        }

        // Toggle 변화 이벤트에 메서드 연결
        slow.onValueChanged.AddListener(delegate {OnToggleChanged(slow, 0.1f);});
        normal.onValueChanged.AddListener(delegate {OnToggleChanged(normal, 0.03f);});
        fast.onValueChanged.AddListener(delegate {OnToggleChanged(fast, 0.01f);});
    }

    private void Init_AudioValue()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        envSlider.value = PlayerPrefs.GetFloat("EnvVolume", 1f);
        
        bgmMute.isOn = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        sfxMute.isOn = PlayerPrefs.GetInt("SFXMute", 0) == 1;
        envMute.isOn = PlayerPrefs.GetInt("EnvMute", 0) == 1;
        
        bgmMute.onValueChanged.AddListener(delegate {ToggleBGM();});
        sfxMute.onValueChanged.AddListener(delegate {ToggleSFX();});
        envMute.onValueChanged.AddListener(delegate {ToggleEnv();});
    }

    // Toggle 상태 변화시 호출될 메서드
    void OnToggleChanged(Toggle changedToggle, float toggleName)
    {
        if(changedToggle.isOn)
        {
            PlayerPrefs.SetFloat("textSpeedSelected", toggleName);
            PlayerPrefs.Save();
        }
    }
    
    public void DropboxOptionChange(int x)
    {
        resolutionNum = x;
    }

    public void FullScreenBtn(bool isFull)
    {
        screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void OptionConfirm()
    {
        Screen.SetResolution(resolutions[resolutionNum].width, resolutions[resolutionNum].height, screenMode, resolutions[resolutionNum].refreshRate);
    }

    public void OptionAudioPlay(string _name)
    {
        AudioManager.instance.PlayButtonSound(_name);
    }

    public void ToggleBGM()
    {
        AudioManager.instance.ToggleBGM();
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
    }
    
    public void ToggleEnv()
    {
        AudioManager.instance.ToggleEnv();
    }

    public void BGMVolume()
    {
        AudioManager.instance.BGMVolume(bgmSlider.value);
    }
    
    public void SFXVolume()
    {
        AudioManager.instance.SFXVolume(sfxSlider.value);
    }
    
    public void EnvVolume()
    {
        AudioManager.instance.EnvVolume(envSlider.value);
    }
}
