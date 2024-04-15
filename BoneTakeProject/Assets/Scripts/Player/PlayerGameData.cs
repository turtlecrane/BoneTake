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
    public float playerSpeed; //높아질수록 이동속도가 증가함
    public float playerMaxJumpForce; //높아질수록 더 높이 올라감 (올라가는 힘은 변하지않음)
    public float playerDashForce; //높아질수록 파워가 세짐 (더 멀리감)
    
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
