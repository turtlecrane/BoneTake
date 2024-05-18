using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifePointState : MonoBehaviour
{
    public bool isDisable;
    public GameObject disableGFX;

    private void Update()
    {
        disableGFX.SetActive(isDisable);
    }
}
