using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class FireLight : MonoBehaviour
{
    public Light2D light2D;

    private void Start()
    {
        InvokeRepeating("BlinkingLight", 0f, 0.2f);
    }

    void Update()
    {
        
    }

    private void BlinkingLight()
    {
        light2D.intensity = Random.Range(4.0f, 6.0f);
    }
}
