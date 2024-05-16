using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


/// <summary>
/// 플레이어의 데이터 저장
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public string playTime;
    
    //플레이어 조작
    public float playerSpeed = 80f; //높아질수록 이동속도가 증가함
    public float playerMaxJumpForce = 90f; //높아질수록 더 높이 올라감 (올라가는 힘은 변하지않음)
    public float playerDashForce = 100f; //높아질수록 파워가 세짐 (더 멀리감)
    
    //현재 장착중인 무기가 무엇인지
    public Weapon_Type weaponType;
    public Weapon_Name weaponName;
    public int weaponHP = -1; //무기 내구도 상태
    
    //현재 공격력
    public int playerATK = 10;

    //플레이어 최대체력 (제한)
    public int playerMaxHP = 3;
    
    //현재 체력
    public int playerHP = 3;
    
    //현재 위치(맵상 위치)
}


public class PlayerDataManager : MonoBehaviour
{
    //public PlayerData playerData;
    public static PlayerDataManager instance;
    public string path; //경로
    public int nowSlot; // 현재 슬롯번호

    public  PlayerData nowPlayer = new PlayerData();
    
    private void Awake()
    {
        #region 싱글톤
        
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
        }
        
        #endregion
        
        path = Application.persistentDataPath + "/";
    }
    
    public void SaveData()
    {
        string data = JsonUtility.ToJson(nowPlayer);
        File.WriteAllText(path + nowSlot.ToString(), data);
    }

    public void LoadData()
    {
        string data = File.ReadAllText(path + nowSlot.ToString());
        nowPlayer = JsonUtility.FromJson<PlayerData>(data);
    }
    
    public void DataClear()
    {
        nowSlot = -1;
        nowPlayer = new PlayerData();
    }
}
