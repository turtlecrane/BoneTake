using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InGameUiManager : MonoBehaviour
{
    public Image weaponIcon;
    public Canvas canvas;
    public Transform hpPosition;
    public GameObject lifePointPrefab;
    public CinemachineVirtualCamera m_playerFollowCamera;
    public CinemachineVirtualCamera m_cursorFollowCamera;
    public List<GameObject> lifePoints = new List<GameObject>();
    
    private CharacterController2D charCon2D;
    private PlayerDataManager playerGameData;
    private WeaponManager weaponManager;
    private WeaponData weaponData;
    private float blinkTimer = 0f;
    private bool isRed = false;
    private float blinkInterval = 0.5f; // 번갈아가며 변경될 시간 간격
    private int lastRecordedMaxHp;
    private int lastRecordedHp;
    
    private void Start()
    {
        playerGameData = PlayerDataManager.instance;
        weaponData = WeaponData.instance;
        charCon2D = CharacterController2D.instance;
        weaponManager = charCon2D.playerAttack.weaponManager;
        
        lastRecordedMaxHp = playerGameData.nowPlayer.playerMaxHP;
        lastRecordedHp = playerGameData.nowPlayer.playerHP;
        CreateLifePoints(lastRecordedMaxHp);
        UpdateLifePoints();
    }

    void Update()
    {
        WeaponUISystem();
        LifePointUISystem();
        CursorCheckState();
    }

    private void WeaponUISystem()
    {
        // 무기 HP의 백분율 계산
        float hpPercentage = (float)weaponManager.weaponLife / weaponData.GetName_WeaponLifeCount(charCon2D.playerAttack.weapon_name);
        Weapon_Type weaponType = charCon2D.playerAttack.weapon_type;
        Weapon_Name weaponName = charCon2D.playerAttack.weapon_name;
        
        // 원래 HP의 35% 이상인 경우
        if (hpPercentage > 0.35f)
        {
            SetWeaponIconState(Color.white, weaponData.weaponGFXSource.freshIcon[weaponData.GetName_WeaponID(weaponName)]);
            ResetBlinkTimer();
        }
        // 원래 HP의 30 ~ 12.6% 경우
        else if (hpPercentage > 0.126f && weaponType != Weapon_Type.Basic)
        {
            SetWeaponIconState(Color.white, weaponData.weaponGFXSource.rottenIcon[weaponData.GetName_WeaponID(weaponName)]);
            ResetBlinkTimer();
        }
        // HP가 매우 낮을 때 (12.6% 이하)
        else if (weaponType != Weapon_Type.Basic)
        {
            weaponIcon.sprite = weaponData.weaponGFXSource.rottenIcon[weaponData.GetName_WeaponID(weaponName)];
            BlinkWeaponIcon();
        }
    }
    
    private void ResetBlinkTimer()
    {
        blinkTimer = 0f;
        isRed = false;
    }

    private void SetWeaponIconState(Color color, Sprite sprite)
    {
        weaponIcon.color = color;
        weaponIcon.sprite = sprite;
    }

    private void BlinkWeaponIcon()
    {
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= blinkInterval)
        {
            blinkTimer = 0f;
            weaponIcon.color = isRed ? Color.white : Color.red;
            isRed = !isRed;
        }
    }

    private void LifePointUISystem()
    {
        int currentMaxHp = playerGameData.nowPlayer.playerMaxHP;
        int currentHp = playerGameData.nowPlayer.playerHP;

        // 최대 체력의 증감 확인 및 처리
        if (lastRecordedMaxHp != currentMaxHp)
        {
            HandleMaxHpChange(currentMaxHp);
        }

        // 현재 체력의 변화 확인 및 처리
        if (lastRecordedHp != currentHp)
        {
            UpdateLifePoints();
            lastRecordedHp = currentHp;
        }
    }
    
    // 최대 체력의 변화를 처리
    void HandleMaxHpChange(int currentMaxHp)
    {
        if (lastRecordedMaxHp < currentMaxHp)
        {
            Debug.Log("최대체력 증가");
            CreateLifePoints(currentMaxHp - lastRecordedMaxHp);
        }
        else
        {
            Debug.Log("최대체력 감소");
            DestroyLifePoints(lastRecordedMaxHp - currentMaxHp);
        }
        lastRecordedMaxHp = currentMaxHp;
    }
    
    /// <summary>
    /// 원하는만큼 라이프포인트 만들기 (UI상으로 왼쪽부터 만들어짐)
    /// </summary>
    /// <param name="hpValue">만들 라이프포인트 수</param>
    private void CreateLifePoints(int hpValue)
    {
        for (int i = 0; i < hpValue; i++)
        {
            GameObject lifePoint = Instantiate(lifePointPrefab, hpPosition);
            lifePoints.Add(lifePoint);
        }
    }
    
    /// <summary>
    /// 원하는만큼 라이프포인트 제거하기 (UI상으로 오른쪽부터 제거됨)
    /// </summary>
    /// <param name="hpValue">제거할 라이프포인트 수</param>
    private void DestroyLifePoints(int hpValue)
    {
        // hpValue만큼 반복하되, 리스트에 오브젝트가 남아 있는 동안만 실행
        for (int i = 0; i < hpValue && lifePoints.Count > 0; i++)
        {
            // 리스트의 첫 번째 오브젝트를 찾아서 파괴
            GameObject objectToDestroy = lifePoints[0];
            Destroy(objectToDestroy);

            // 리스트에서 해당 오브젝트 제거
            lifePoints.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 라이프포인트의 변화 처리하기
    /// </summary>
    private void UpdateLifePoints()
    {
        for (int i = 0; i < lifePoints.Count; i++)
        {
            LifePointState hpScript = lifePoints[i].GetComponent<LifePointState>();
            if (i < lifePoints.Count - playerGameData.nowPlayer.playerHP)
            {
                lifePoints[i].GetComponent<Image>().color = Color.grey;
                hpScript.isDisable = true;
            }
            else
            {
                lifePoints[i].GetComponent<Image>().color = Color.red; //이미지라면 White로 Red는 Test용.
                hpScript.isDisable = false;
            }
        }
    }
    
    void CursorCheckState()
    {
        var isAiming = charCon2D.playerAttack.isAiming;
        var isInteractiveCamera = charCon2D.playerInteraction.isInteractiveCamera;
        var weaponManager = charCon2D.playerAttack.weaponManager;

        SetCameraActiveState(isAiming, isInteractiveCamera);
        SetCursorState(isAiming, weaponManager);
    }

    void SetCameraActiveState(bool isAiming, bool isInteractiveCamera)
    {
        if (!isInteractiveCamera)
        {
            m_playerFollowCamera.gameObject.SetActive(!isAiming);
            m_cursorFollowCamera.gameObject.SetActive(isAiming);
        }
    }

    void SetCursorState(bool isAiming, WeaponManager weaponManager)
    {
        bool uiLayerActive = CheckForActiveUILayer();
        Cursor.visible = isAiming || uiLayerActive;
        Cursor.lockState = isAiming || uiLayerActive ? CursorLockMode.None : CursorLockMode.Locked;

        if (isAiming)
        {
            Vector2 hotSpot = new Vector2(weaponManager.aimingCursor.width / 2, weaponManager.aimingCursor.height / 2);
            Cursor.SetCursor(weaponManager.aimingCursor, hotSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    public bool CheckForActiveUILayer()
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.gameObject.activeInHierarchy && child.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                //GameManager.Instance.GetDevSetting().Dev_WorldTime = 0f;
                return true; // "UI" 레이어에 속하고 활성화된 오브젝트가 있으면 true 반환
            }
        }
        //GameManager.Instance.GetDevSetting().Dev_WorldTime = 1f;
        return false; // "UI" 레이어에 속하고 활성화된 오브젝트가 없으면 false 반환
    }
}
