using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifePointState : MonoBehaviour
{
    public bool isDisable;
    public GameObject disableGFX;
    public Animator animator;

    private void Update()
    {
        disableGFX.SetActive(isDisable);
        animator.SetBool("IsDisable", isDisable);
    }
}
