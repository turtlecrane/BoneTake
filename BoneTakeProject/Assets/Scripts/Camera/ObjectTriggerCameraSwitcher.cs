using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// - 플레이어가 정해진 트리거에 입장하면
///   플레이어 추적 카메라는 비활성화, 해당 트리거 가상 카메라를 ON
/// - 플레이어가 트리거에서 벗어나면
///   플레이어 추적 카메라 활성화, 해당 트리거 가상 카메라 OFF
/// </summary>
public class ObjectTriggerCameraSwitcher : MonoBehaviour
{
    private GameObject m_playerFollowCamera;
    public GameObject m_thisVituralCam;
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player") && !other.isTrigger)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            playerInteraction.isInteractiveCamera = true;
            m_playerFollowCamera = GameObject.FindWithTag("PlayerFollowCamera");
            if (m_playerFollowCamera != null)
            {
                m_playerFollowCamera.SetActive(false);
            }
            m_thisVituralCam.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player") && !other.isTrigger)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            playerInteraction.isInteractiveCamera = false;
            m_thisVituralCam.SetActive(false);
            //m_playerFollowCamera.SetActive(true);
            m_playerFollowCamera = null;
        }
    }
}
