using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 프로젝트에서 공통적으로 사용되는 함수를 보관
/// 모든 스크립트에서 호출가능
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<GameManager>();
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = "GameManager";
                    _instance = container.AddComponent(typeof(GameManager)) as GameManager;
                }
            }

            return _instance;
        }
    }
    
    public PlayerFollowCameraController GetPlayerFollowCameraController() => GameObject.Find("PlayerFollowCameraController").GetComponent<PlayerFollowCameraController>();
    public PlayerGameData GetPlayerGameData() => GameObject.Find("DataManager").GetComponent<PlayerGameData>();
    public WeaponData GetWeaponData() => GameObject.Find("DataManager").GetComponent<WeaponData>();
    public DevSystemSetting GetDevSetting() => GameObject.Find("DevSetting").GetComponent<DevSystemSetting>();
    public CharacterController2D GetCharacterController2D() => GameObject.Find("Player").GetComponent<CharacterController2D>();
    
    
}
