using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad] // Unity가 로드될 때 이 클래스를 자동으로 초기화합니다.
public class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        // 플레이 모드로 전환되기 직전에 실행됩니다.
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 여기에 시작하고자 하는 씬의 경로를 설정하세요.
            string defaultScenePath = "Assets/Scenes/Official/MainTitle.unity";

            if (EditorSceneManager.GetActiveScene().path != defaultScenePath)
            {
                // 설정한 씬을 로드합니다.
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(defaultScenePath);
            }
        }
    }
}