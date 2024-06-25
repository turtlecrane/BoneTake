using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownTree : MonoBehaviour
{
    public AudioSource treeAudio;
    
    void Start()
    {
        InvokeRepeating("PlayTreeAudio", 0f, 60f);
    }
    
    public void PlayTreeAudio()
    {
        treeAudio.Play();
    }
}
