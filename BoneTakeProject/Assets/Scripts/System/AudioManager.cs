using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string Name;
    public List<AudioClip> Clip;
}

public class AudioManager : MonoBehaviour
{
    public Sound[] bgmSounds, sfxSounds, environmentSounds;

    [Space(10f)] 
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public Transform environSourceList;

    [Space(10f)] 
    public AudioMixerGroup environMixerGroup;
    public List<AudioSource> environSource;

    [Space(10f)] 
    public List<AudioMixerSnapshot> snapshots;
    

    public static AudioManager instance;

    public bool isBGMChanging;

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
    
    public IEnumerator PlayBGM(string bgmName, int arrayNum = 0, Action action = null)
    {
        yield return new WaitUntil(()=>!isBGMChanging);
        
        bgmSource.Stop();
        bgmSource.clip = null;
        
        bgmSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        
        if (bgmSource.clip == null)
        { 
            ChangeBGM(bgmName, arrayNum); 
            bgmSource.loop = false;
            action?.Invoke();
        }
    }

    private void ChangeBGM(string name, int arrayNum = 0)
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
                //이미 있는 환경음이면 추가 안함
                foreach (AudioSource es in environSource) if (name == es.name) return;
                
                // 오브젝트 생성 및 AudioSource 컴포넌트 추가
                GameObject newAudioObject = new GameObject(name);
                newAudioObject.transform.SetParent(environSourceList);
                AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
                newAudioSource.loop = true;
                newAudioSource.outputAudioMixerGroup = environMixerGroup;
                newAudioSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);

                // 리스트에 추가
                environSource.Add(newAudioSource);

                // 오디오 클립 설정 및 재생
                newAudioSource.clip = s.Clip[arrayNum];
                newAudioSource.Play();
            }
        }
    }

    public void StopAndRemoveEnvironSound(string name)
    {
        // environSource 리스트에서 이름이 같은 클립을 재생 중인 오디오 소스 검색
        AudioSource targetSource = environSource.Find(source => source.clip != null && source.clip.name == name);

        if (targetSource != null)
        {
            // 오디오 소스 정지 및 리스트에서 제거
            targetSource.Stop();
            environSource.Remove(targetSource);

            // 오브젝트 파괴
            Destroy(targetSource.gameObject);
        }
        else
        {
            Debug.Log("해당 이름의 소리를 재생중인 오브젝트가 없습니다.");
        }
    }

    public void AllRemoveEnvironSound()
    {
        if (environSource.Count > 0)
        {
            foreach (var enAudio in environSource)
            {
                enAudio.Stop();
                Destroy(enAudio);
                environSource.Remove(enAudio);
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

    public void ChangeAudioMixerSnapShot(int arrayNum)
    {
        snapshots[arrayNum].TransitionTo(0.001f);
    }

    public void BgmFadeOut(float duration)
    {
        StartCoroutine(FadeOut(duration));
    }

    public IEnumerator FadeOut(float duration)
    {
        isBGMChanging = true;
        bgmSource.loop = false;
        float startVolume = bgmSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        bgmSource.volume = 0;
        bgmSource.clip = null;
        isBGMChanging = false;
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
