using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShadowEffect : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DOTween.To(()=> GameManager.Instance.GetPlayerFollowCameraController().shadowVolume.weight,
                x=> GameManager.Instance.GetPlayerFollowCameraController().shadowVolume.weight = x,
                1f, 2f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DOTween.To(()=> GameManager.Instance.GetPlayerFollowCameraController().shadowVolume.weight,
                x=> GameManager.Instance.GetPlayerFollowCameraController().shadowVolume.weight = x,
                0f, 1f);
        }
    }
}
