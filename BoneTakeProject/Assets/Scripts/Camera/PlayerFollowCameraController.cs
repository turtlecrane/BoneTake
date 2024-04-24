using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[System.Serializable]
public class CinemachineImpulseData
{
    public float AmplitudeGain;
    public float FrequencyGain;
    public float SustainTime;
    public float DecayTime;
}

public class PlayerFollowCameraController : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject m_playerFollowCamera; //플레이어 추적중인 카메라 오브젝트

    public CinemachineVirtualCamera virtualCamera; //target이 플레이어로 되어있는 가상 시네머신 카메라
    private CinemachineCollisionImpulseSource screenShake; 
    private CharacterController2D player_CharacterController; //플레이어 스크립트
    
    public float lensOrtho_InitSize;//화면 줌 초기값 저장
    
    [Range (0.0f, 0.1f)]
    public float LensOrtho_ZoomValue;//낙하시 줌 되는 정도

    [Range(0.0f, 10.0f)]
    public float Impulse_PowerValue;
    
    public CinemachineImpulseData InitialImpulseData; //Impulse데이터 초기값 저장
    
    void Start()
    {
        virtualCamera = m_playerFollowCamera.GetComponent<CinemachineVirtualCamera>();
        lensOrtho_InitSize = virtualCamera.m_Lens.OrthographicSize;//화면 줌 초기값 저장

        screenShake = m_playerFollowCamera.GetComponent<CinemachineCollisionImpulseSource>();//Impulse데이터 초기값 저장
        InitialImpulseData.AmplitudeGain = screenShake.m_ImpulseDefinition.m_AmplitudeGain;
        InitialImpulseData.FrequencyGain = screenShake.m_ImpulseDefinition.m_FrequencyGain;
        InitialImpulseData.SustainTime = screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime;
        InitialImpulseData.DecayTime = screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime;
        
        player_CharacterController = virtualCamera.Follow.gameObject.GetComponent<CharacterController2D>();
    }

    void Update()
    {
        if (player_CharacterController.m_Rigidbody2D.velocity.y < -31.0f)//어느정도 속도부터 낙하시 줌이 되는지 조절 
        {
            virtualCamera.m_Lens.OrthographicSize -= LensOrtho_ZoomValue;
        }
        if (!m_playerFollowCamera.activeSelf)//플레이어 추적중인 카메라가 비활성화 되어있는 경우 (다른 카메라로 시네머신 브레인이 넘어간 경우)
        {
            virtualCamera.m_Lens.OrthographicSize = lensOrtho_InitSize; //화면 줌 값을 초기값으로 변경
        }
    }

    public IEnumerator PlayerBigLandingNosie()
    {
        virtualCamera.m_Lens.OrthographicSize = lensOrtho_InitSize; //화면 줌 값을 초기값으로 변경
        
        //Impulse 진동 속성 조절
        screenShake.m_ImpulseDefinition.m_AmplitudeGain = Impulse_PowerValue;
        screenShake.m_ImpulseDefinition.m_FrequencyGain = Impulse_PowerValue;
        screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = player_CharacterController.bigFallCantMoveCoolTime / 2;
        screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = player_CharacterController.bigFallCantMoveCoolTime;//지속시간 (여기선 큰착지 쿨타임 시간과 동일하게)
        screenShake.GenerateImpulse();
        
        yield return new WaitForSeconds(player_CharacterController.bigFallCantMoveCoolTime);
        //Impulse데이터 초기값으로 초기화
        screenShake.m_ImpulseDefinition.m_AmplitudeGain = InitialImpulseData.AmplitudeGain;
        screenShake.m_ImpulseDefinition.m_FrequencyGain = InitialImpulseData.FrequencyGain;
        screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = InitialImpulseData.SustainTime;
        screenShake.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = InitialImpulseData.DecayTime;
    }
    
}
