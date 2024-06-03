using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string Name;
    public List<AudioClip> Clip;
}

public class AudioManager : MonoBehaviour
{
    public Sound[] bgmSounds, sfxSounds, environmentSounds;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource environSource;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bgmSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        bgmSource.mute = PlayerPrefs.GetInt("BGMMute", 0) == 1;
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxSource.mute = PlayerPrefs.GetInt("SFXMute", 0) == 1;
    }

    public void PlayBGM(string name, int arrayNum = 0)
    {
        Sound s = Array.Find(bgmSounds, x => x.Name == name);

        if (s == null)
        {
            Debug.Log("잘못된 이름의 소리재생 접근");
        }
        else
        {
            if (s.Clip[0] == null)
            {
                Debug.Log("이름을 찾았지만, 잘못된 번호의 소리재생 접근");
            }
            else
            {
                bgmSource.clip = s.Clip[arrayNum];
                bgmSource.Play();
            }
        }
    }

    public void PlaySFX(string name, int arrayNum = 0)
    {
        Sound s = Array.Find(sfxSounds, x => x.Name == name);

        if (s == null)
        {
            Debug.Log("잘못된 이름의 소리재생 접근");
        }
        else
        {
            if (s.Clip[arrayNum] == null)
            {
                Debug.Log("이름을 찾았지만, 잘못된 번호의 소리재생 접근");
            }
            else
            {
                sfxSource.PlayOneShot(s.Clip[arrayNum]);
            }
        }
    }

    public void PlayEnvironSound(string name, int arrayNum = 0)
    {
        Sound s = Array.Find(environmentSounds, x => x.Name == name);

        if (s == null)
        {
            Debug.Log("잘못된 이름의 소리재생 접근");
        }
        else
        {
            if (s.Clip[arrayNum] == null)
            {
                Debug.Log("이름을 찾았지만, 잘못된 번호의 소리재생 접근");
            }
            else
            {
                environSource.clip = s.Clip[arrayNum];
                environSource.Play();
            }
        }
    }

    public void PlayButtonSound(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.Name == name);

        if (s == null)
        {
            Debug.Log("잘못된 이름의 소리재생 접근");
        }
        else
        {
            sfxSource.PlayOneShot(s.Clip[0]);
        }
    }

public void BgmFadeOut(float duration)
    {
        StartCoroutine(FadeOut(duration));
    }

    public void BgmFadeIn(float duration, Action action = null)
    {
        StartCoroutine(FadeIn(duration, action));
    }

    public IEnumerator FadeOut(float duration)
    {
        float startVolume = bgmSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        bgmSource.volume = 0;
        bgmSource.clip = null;
    }

    public IEnumerator FadeIn(float duration, Action action = null)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, PlayerPrefs.GetFloat("BGMVolume", 1f), t / duration);
            yield return null;
        }
        bgmSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        action?.Invoke();
    }

    public void ToggleBGM()
    {
        bgmSource.mute = !bgmSource.mute;
        
        PlayerPrefs.SetInt("BGMMute", bgmSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
        PlayerPrefs.SetInt("SFXMute", sfxSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void BGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
}
