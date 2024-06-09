using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;


/// <summary>
/// 플레이어의 데이터 저장
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public float playTime;
    
    //저장된 위치(씬 이름 저장)
    public string mapName = "Interaction"; //새로시작할때 들어갈 기본 씬을 지정해야함. (ex 튜토리얼 씬)
    
    //플레이어 조작
    public float playerSpeed = 80f; //높아질수록 이동속도가 증가함
    //public float playerWaterSpeed = 40f; //물속에서의 움직임 속도
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

    //처치한 보스의 종류
    public List<string> killedTypeOfBosses = new List<string>();
    
    //맵데이터
    public List<SceneData> mapData = new List<SceneData>();
}

[System.Serializable]
public class SceneData
{
    public string sceneName;
    public List<BreakableObjectData> breakableObjects = new List<BreakableObjectData>();
}

[System.Serializable]
public class BreakableObjectData
{
    public string name;
    public bool isDestroy;
}



public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;
    public string path; //경로
    public int nowSlot; // 현재 슬롯번호

    public  PlayerData nowPlayer = new PlayerData();
    private string keyWord = "QmfkdnsE\ud83d\udc31\u200d\ud83d\udc53jtmxmdlqkfTh";
    
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
        File.WriteAllText(path + nowSlot.ToString(), Encrypt(data, keyWord));
    }

    public void LoadData()
    {
        string data = File.ReadAllText(path + nowSlot.ToString());
        nowPlayer = JsonUtility.FromJson<PlayerData>(Decrypt(data, keyWord));
    }

    public void DeleteData()
    {
        if (File.Exists(path + nowSlot.ToString())) // 파일이 실제로 존재하는지 확인
        {
            File.Delete(path + nowSlot.ToString()); // 파일이 존재하면 삭제
        }
    }
    
    public void DataClear(int _nowSlot = -1)
    {
        nowSlot = _nowSlot;
        nowPlayer = new PlayerData();
    }
    
    public static string Decrypt(string textToDecrypt, string key)
    {
        RijndaelManaged rijndaelCipher = new RijndaelManaged();
        rijndaelCipher.Mode = CipherMode.CBC;
        rijndaelCipher.Padding = PaddingMode.PKCS7;
        rijndaelCipher.KeySize = 128;
        rijndaelCipher.BlockSize = 128;
        
        byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyBytes = new byte[16];
        
        int len = pwdBytes.Length;
        if (len > keyBytes.Length) len = keyBytes.Length;

        Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        rijndaelCipher.IV = keyBytes;

        byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return Encoding.UTF8.GetString(plainText);
    }

 

        public static string Encrypt(string textToEncrypt, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;

            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
            
            int len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
            
            Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
}
