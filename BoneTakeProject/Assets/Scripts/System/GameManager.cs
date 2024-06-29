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
        // 맵 데이터에 이미 있는 씬 데이터인지 확인
        SceneData existingSceneData = PlayerDataManager.instance.nowPlayer.mapData.Find(sceneData => sceneData.sceneName == sceneName);

        if (existingSceneData != null)
        {
            // SceneData가 이미 존재할 경우, BreakableObjectData 추가
            AddBreakableObjects(existingSceneData);
            AddImageChangeObjects(existingSceneData);
        }
        else
        {
            // SceneData가 존재하지 않을 경우, 새로운 SceneData 객체 생성 후 추가
            SceneData newSceneData = new SceneData { sceneName = sceneName };
            PlayerDataManager.instance.nowPlayer.mapData.Add(newSceneData);
            AddBreakableObjects(newSceneData);
            AddImageChangeObjects(newSceneData);
        }
    }

    private void AddBreakableObjects(SceneData sceneData)
    {
        BreakableObject[] breakableObjects = FindObjectsOfType<BreakableObject>();

        foreach (var obj in breakableObjects)
        {
            if (obj.isRemembered && !sceneData.breakableObjects
                    .Exists(data => data.name == obj.gameObject.name))
            {
                BreakableObjectData newData = new BreakableObjectData
                {
                    name = obj.gameObject.name,
                    isDestroy = obj.isDestroy
                };
                sceneData.breakableObjects.Add(newData);
            }
        }
    }
    
    private void AddImageChangeObjects(SceneData sceneData)
    {
        ImageChangeObject[] breakableObjects = FindObjectsOfType<ImageChangeObject>();

        foreach (var obj in breakableObjects)
        {
            if (obj.isRemembered && !sceneData.imageChangeObjects
                    .Exists(data => data.name == obj.gameObject.name))
            {
                ImageChangeObjectData newData = new ImageChangeObjectData
                {
                    name = obj.gameObject.name,
                    isChanged = obj.isChanged,
                    changeSprite = obj.changedSprite
                };
                sceneData.imageChangeObjects.Add(newData);
            }
        }
    }
}
