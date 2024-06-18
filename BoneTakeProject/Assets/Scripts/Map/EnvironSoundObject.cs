using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironSoundObject : MonoBehaviour
{
    public List<AudioSource> envSources;
    
    void Start()
    {
        
    }

    private void Update()
    {
        foreach (var src in envSources)
        {
            src.mute = PlayerPrefs.GetInt("EnvMute", 0) == 1;
            src.volume = PlayerPrefs.GetFloat("EnvVolume", 1f);
        }
    }
}
