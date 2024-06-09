using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public DevSystemSetting GetDevSetting() => GameObject.Find("DevSetting").GetComponent<DevSystemSetting>();
    //public CharacterController2D GetCharacterController2D() => GameObject.Find("Player").GetComponent<CharacterController2D>();
    public PopupManager GetPopupManager() => GameObject.Find("Canvas").GetComponent<Transform>().Find("Popup").GetComponent<PopupManager>();
    public InGameUiManager GetInGameUiManager() => GameObject.Find("Canvas").GetComponent<Transform>().Find("InGameUI").GetComponent<InGameUiManager>();
    public PauseMenu GetPauseMenu() => GameObject.Find("Canvas").GetComponent<Transform>().Find("PauseMenu").GetComponent<PauseMenu>();
    
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 자동으로 호출
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnPlayerEnterScene(scene.name);
    }
    
    public void OnPlayerEnterScene(string sceneName)
    {
        foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
        {
            if(sceneData.sceneName == sceneName) return;
        }
        
        // SceneData 객체 생성 후, 맵 데이터 리스트에 추가
        SceneData newSceneData = new SceneData();
        newSceneData.sceneName = sceneName;
        PlayerDataManager.instance.nowPlayer.mapData.Add(newSceneData);

        // 현재 씬의 모든 오브젝트를 탐색하여 BreakableObject 스크립트를 가진 오브젝트를 찾음
        BreakableObject[] breakableObjects = FindObjectsOfType<BreakableObject>();

        foreach (var obj in breakableObjects)
        {
            if (obj.isRemembered)
            {
                // BreakableObjectData 객체 생성 후, SceneData에 추가
                BreakableObjectData data = new BreakableObjectData();
                data.name = obj.gameObject.name;
                data.isDestroy = obj.isDestroy;
                newSceneData.breakableObjects.Add(data);
            }
        }
    }
}
