using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameScene : MonoBehaviour
{
    public Narration narration;

    private void Update()
    {
        if (narration.isEnd)
        {
            SceneManager.LoadScene("Tutorial");
        }
    }
}
