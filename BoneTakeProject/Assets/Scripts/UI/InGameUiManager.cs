using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUiManager : MonoBehaviour
{
    public Image weaponIcon;
    public Transform hpPosition;
    public GameObject lifePointPrefab;

    private CharacterController2D charCon2D;
    private PlayerGameData playerGameData;
    private WeaponManager weaponManager;
    private WeaponData weaponData;

    private float blinkTimer = 0f;
    private bool isRed = false;
    private float blinkInterval = 0.5f; // 번갈아가며 변경될 시간 간격
    private List<GameObject> lifePoints = new List<GameObject>();
    private int lastRecordedHp;
    
    private void Start()
    {
        playerGameData = GameManager.Instance.GetPlayerGameData();
        charCon2D = GameManager.Instance.GetCharacterController2D();
        weaponManager = charCon2D.playerAttack.weaponManager;
        weaponData = GameManager.Instance.GetWeaponData();
        
        lastRecordedHp = playerGameData.playerData.playerHP;
        CreateLifePoints();
    }

    void Update()
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
            BlinkWeaponIcon();
        }
        
        
        //...TESTCODE
        if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic)
        {
            Debug.Log("hpPercentage : " + hpPercentage*100 + "% \n (float)weaponManager.weaponLife : " + (float)weaponManager.weaponLife + "\n originalWeaponHP : " + weaponData.GetName_WeaponLifeCount(charCon2D.playerAttack.weapon_name));
        }
        if (lastRecordedHp != playerGameData.playerData.playerHP)
        {
            UpdateLifePoints();
            lastRecordedHp = playerGameData.playerData.playerHP;
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
    
    private void CreateLifePoints()
    {
        for (int i = 0; i < playerGameData.playerData.playerHP; i++)
        {
            GameObject lifePoint = Instantiate(lifePointPrefab, hpPosition);
            lifePoints.Add(lifePoint);
        }
    }
    
    private void UpdateLifePoints()
    {
        for (int i = 0; i < lifePoints.Count; i++)
        {
            if (i < playerGameData.playerData.playerHP)
            {
                lifePoints[i].GetComponent<Image>().color = Color.red;
            }
            else
            {
                lifePoints[i].GetComponent<Image>().color = Color.grey;
            }
        }
    }
}
