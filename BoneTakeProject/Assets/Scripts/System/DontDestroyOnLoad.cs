using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoad : MonoBehaviour
{
    public string destorySceneName;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == destorySceneName)
        {
            Destroy(this.gameObject); // 지정한 씬으로 이동 시 오브젝트 파괴
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 오브젝트가 파괴될 때 이벤트에서 함수를 제거하여 메모리 누수 방지
    }
}
