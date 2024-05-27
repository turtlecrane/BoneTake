using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public CinemachineCollisionImpulseSource bigLandingShakeSource; 
    private CharacterController2D player_CharacterController; //플레이어 스크립트
    private Collider2D mapSection;
    public float lensOrtho_InitSize;//화면 줌 초기값 저장
    
    [Range (0.0f, 0.5f)]
    public float LensOrtho_ZoomValue;//낙하시 줌 되는 정도

    [Range(0.0f, 10.0f)]
    public float Impulse_PowerValue;
    
    public CinemachineImpulseData InitialImpulseData; //Impulse데이터 초기값 저장

    private void Awake()
    {
        virtualCamera = m_playerFollowCamera.GetComponent<CinemachineVirtualCamera>();
        player_CharacterController = virtualCamera.Follow.gameObject.GetComponent<CharacterController2D>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += MapSectionLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= MapSectionLoad;
    }

    // 씬 로딩이 완료되었을 때 호출될 콜백 함수
    private void MapSectionLoad(Scene scene, LoadSceneMode mode)
    {
        mapSection = GameObject.FindWithTag("MapSection")?.GetComponent<Collider2D>();
        if (mapSection == null) return;
        //카메라 맵 섹션 정하기
        CinemachineConfiner confiner = virtualCamera.GetComponent<CinemachineConfiner>();
        if (confiner == null)
        {
            Debug.LogError("CinemachineConfiner2D 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        confiner.m_BoundingShape2D = mapSection;
    }
    
    void Start()
    {
        lensOrtho_InitSize = virtualCamera.m_Lens.OrthographicSize;//화면 줌 초기값 저장
        InitialImpulseData.AmplitudeGain = bigLandingShakeSource.m_ImpulseDefinition.m_AmplitudeGain;
        InitialImpulseData.FrequencyGain = bigLandingShakeSource.m_ImpulseDefinition.m_FrequencyGain;
        InitialImpulseData.SustainTime = bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime;
        InitialImpulseData.DecayTime = bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime;
    }

    void Update()
    {
        if (player_CharacterController.m_Rigidbody2D.velocity.y < -31.0f) //어느정도 속도부터 낙하시 줌이 되는지 조절 
        {
            virtualCamera.m_Lens.OrthographicSize -= LensOrtho_ZoomValue;
        }
        if (!m_playerFollowCamera.activeSelf)//플레이어 추적중인 카메라가 비활성화 되어있는 경우 (다른 카메라로 시네머신 브레인이 넘어간 경우)
        {
            virtualCamera.m_Lens.OrthographicSize = lensOrtho_InitSize; //화면 줌 값을 초기값으로 변경
        }
        
        //카메라 줌 제한
        if (virtualCamera.m_Lens.OrthographicSize < 4)
        {
            virtualCamera.m_Lens.OrthographicSize = 4f;
        }
    }

    public IEnumerator PlayerBigLandingNosie()
    {
        virtualCamera.m_Lens.OrthographicSize = lensOrtho_InitSize; //화면 줌 값을 초기값으로 변경
        
        //Impulse 진동 속성 조절
        bigLandingShakeSource.m_ImpulseDefinition.m_AmplitudeGain = Impulse_PowerValue;
        bigLandingShakeSource.m_ImpulseDefinition.m_FrequencyGain = Impulse_PowerValue;
        bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = player_CharacterController.bigFallCantMoveCoolTime / 2;
        bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = player_CharacterController.bigFallCantMoveCoolTime;//지속시간 (여기선 큰착지 쿨타임 시간과 동일하게)
        bigLandingShakeSource.GenerateImpulse();
        
        yield return new WaitForSeconds(player_CharacterController.bigFallCantMoveCoolTime);
        //Impulse데이터 초기값으로 초기화
        bigLandingShakeSource.m_ImpulseDefinition.m_AmplitudeGain = InitialImpulseData.AmplitudeGain;
        bigLandingShakeSource.m_ImpulseDefinition.m_FrequencyGain = InitialImpulseData.FrequencyGain;
        bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = InitialImpulseData.SustainTime;
        bigLandingShakeSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = InitialImpulseData.DecayTime;
    }

    /// <summary>
    /// duration동안 targetSize정도의 카메라 줌인이 되게하는 함수
    /// </summary>
    /// <param name="duration"> 목표까지 도달하는데 걸리는 시간 </param>
    /// <param name="targetSize">목표 사이즈 </param>
    public IEnumerator CameraTargetTimeZoomIn(float duration, float targetSize)
    {
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime; // 시간 업데이트
            float newSize = Mathf.Lerp(startSize, targetSize, currentTime / duration); // 새 사이즈 계산
            virtualCamera.m_Lens.OrthographicSize = newSize; // 카메라 사이즈 업데이트
            yield return null;
        }
    }
    
}
