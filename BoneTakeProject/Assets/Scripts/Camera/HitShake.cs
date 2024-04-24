using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class HitShake : MonoBehaviour
{
    public CinemachineCollisionImpulseSource screenShake;

    /// <summary>
    /// 화면의 기본적인 흔들림을 주기
    /// </summary>
    public void HitScreenShake()
    {
        screenShake.GenerateImpulse();
    }
}
