using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 프로젝트에서 공통적으로 사용되는 함수를 보관
/// 모든 스크립트에서 호출가능
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    /*public static GameManager Instance
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
    }*/

    private void Awake()
    {
        #region 싱글톤
        
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        
        #endregion
    }

    public PlayerFollowCameraController GetPlayerFollowCameraController() => GameObject.Find("PlayerFollowCameraController").GetComponent<PlayerFollowCameraController>();
    public DevSystemSetting GetDevSetting() => GameObject.Find("DevSetting").GetComponent<DevSystemSetting>();
    //public CharacterController2D GetCharacterController2D() => GameObject.Find("Player").GetComponent<CharacterController2D>();
    public PopupManager GetPopupManager() => GameObject.Find("Canvas").GetComponent<Transform>().Find("Popup").GetComponent<PopupManager>();
    public InGameUiManager GetInGameUiManager() => GameObject.Find("Canvas").GetComponent<Transform>().Find("InGameUI").GetComponent<InGameUiManager>();
    public PauseMenu GetPauseMenu() => GameObject.Find("Canvas").GetComponent<Transform>().Find("PauseMenu").GetComponent<PauseMenu>();
    
}
