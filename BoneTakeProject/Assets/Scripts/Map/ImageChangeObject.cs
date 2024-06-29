using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageChangeObject : MonoBehaviour
{
    [Header("Component")]
    public SpriteRenderer spriteRenderer;
    
    public bool canColliderChange;
    [DrawIf("canColliderChange", true)] 
    public Collider2D collider2D;
    public bool isRemembered;
    public Sprite changedSprite;
    
    public bool isChanged;
    public string sceneName;
    
    private void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    void Start()
    {
        foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
        {
            if (sceneData.sceneName == sceneName)
            {
                foreach (var obj in sceneData.imageChangeObjects)
                {
                    if (obj.name == gameObject.name && obj.isChanged)
                    {
                        //데이터에 있는 스프라이트로 변경
                        isChanged = obj.isChanged;
                        if (canColliderChange)
                            collider2D.isTrigger = obj.isChanged;
                        spriteRenderer.sprite = obj.changeSprite;
                        break;
                    }
                }
                break;
            }
        }
    }

    public void ChangeImage()
    {
        if (canColliderChange)
        {
            collider2D.isTrigger = true;
        }
        spriteRenderer.sprite = changedSprite;
        isChanged = true;
        
        //맵 데이터에 저장 
        foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
        {
            if (sceneData.sceneName == sceneName)
            {
                foreach (var obj in sceneData.imageChangeObjects)
                {
                    if (obj.name == gameObject.name)
                    {
                        obj.isChanged = true;
                        obj.changeSprite = spriteRenderer.sprite;
                        break;
                    }
                }
            }
        }
    }
}
