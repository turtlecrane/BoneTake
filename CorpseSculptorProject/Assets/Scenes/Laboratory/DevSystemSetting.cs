using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevSystemSetting : MonoBehaviour
{
    [Range(0.0f, 1.0f)] public float Dev_WorldTime;

    void Update()
    {
        Time.timeScale = Dev_WorldTime;
    }
}
