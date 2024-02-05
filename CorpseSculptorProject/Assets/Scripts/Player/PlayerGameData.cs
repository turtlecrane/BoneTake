using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 데이터 저장
/// </summary>
[System.Serializable]
public class PlayerData
{
    //플레이어 조작
    public float playerSpeed;
    public float playerJumpForce;
    public float playerDashForce;
    
    //현재 장착중인 무기가 무엇인지
    public Weapon_Type weaponType;
    
    //현재 공격력
    public int playerATK;

    //현재 체력
    //현재 위치
}


public class PlayerGameData : MonoBehaviour
{
    public PlayerData playerData;
    
    
}
