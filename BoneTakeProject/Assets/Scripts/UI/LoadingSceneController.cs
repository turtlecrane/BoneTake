using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    private static string nextScene;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
        AudioManager.instance.BgmFadeOut(1f);
    }

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            
            if (op.progress < 0.9f)
            {
                Debug.Log(op.progress);
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= 1f)
                {
                    op.allowSceneActivation = true;
                    //AudioManager.instance.BgmFadeIn(3f);
                    yield break;
                }
            }
        }
    }
}
