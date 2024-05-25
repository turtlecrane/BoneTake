using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevSystemSetting : MonoBehaviour
{
    [Range(0.0f, 1.0f)] public float Dev_WorldTime;
    private InGameUiManager inGameUiManager;

    private void Awake()
    {
        inGameUiManager = GameManager.Instance.GetInGameUiManager();
    }

    void Update()
    {
        if (inGameUiManager.CheckForActiveUILayer(LayerMask.GetMask("UI")))
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = Dev_WorldTime;
        }
    }
}
