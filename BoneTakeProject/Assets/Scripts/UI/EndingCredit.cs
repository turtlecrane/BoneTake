using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCredit : MonoBehaviour
{
    public string changeBGMName;
    public PopupManager popupManager;
    public Texture2D basicCursor;
    public bool isEnd;
    private Animator animator;

    private void Awake()
    {
        Cursor.SetCursor(basicCursor, Vector2.zero, CursorMode.Auto);
        animator = gameObject.GetComponent<Animator>();
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        AudioManager.instance.AllRemoveEnvironSound();
        StartCoroutine(ChangeBGM());
    }

    void Update()
    {
        if (isEnd)
        {
            isEnd = false;
            AudioManager.instance.BgmFadeOut(1f);
            SceneManager.LoadScene("MainTitle"); 
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
        {
            popupManager.SetPopup("엔딩 크레딧을 생략하시겠습니까?",false, () =>
            {
                popupManager.ClosePopup();
                AudioManager.instance.BgmFadeOut(1f);
                LoadingSceneController.LoadScene("MainTitle");
            },()=>{});
        }
    }

    public void CreditEnd()
    {
        isEnd = true;
    }
    
    private IEnumerator ChangeBGM()
    {
        if (AudioManager.instance.isBGMChanging)
        {
            AudioManager.instance.isBGMChanging = false;
            StartCoroutine(AudioManager.instance.FadeOut(1f)); 
            yield return new WaitUntil(()=>!AudioManager.instance.isBGMChanging);
        }
        else
        { 
            StartCoroutine(AudioManager.instance.FadeOut(1f)); 
            yield return new WaitUntil(()=>!AudioManager.instance.isBGMChanging);
        }
        
        InvokeRepeating("InvokePlayInGameBGM", 1f, 300f);
    }
    
    private void InvokePlayInGameBGM()
    {
        Debug.Log("엔딩 BGM 재생");
        StartCoroutine(AudioManager.instance.PlayBGM(changeBGMName));
    }
}
