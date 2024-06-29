using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakableObject : MonoBehaviour
{
    public float hp;
    public Transform objGFX;
    //public Weapon_Type breakableWeapon; //부서질수 있는 무기의 종류 -> "무기"면 모두 부서지도록 변경 (24.06.19)
    
    public ParticleSystem damageParticles; //파편 파티클
    public ParticleSystem brokenParticles; //부서지는 파티클
    public ParticleSystem dustParticles;
    
    public string sceneName;
    public bool isRemembered;
    public bool isShakable;
    public bool dontDestroy;
    
    public bool isShortCut;
    [DrawIf("isShortCut", true)]
    public string shortCutSceneName;
    
    [HideInInspector]public bool isDestroy = false;
    private float shakeValue;
    private Collider2D collider;
    
    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        sceneName = SceneManager.GetActiveScene().name;
    }

    private void Start()
    {
        foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
        {
            if (sceneData.sceneName == sceneName)
            {
                foreach (var obj in sceneData.breakableObjects)
                {
                    if (obj.name == gameObject.name)
                    {
                        //obj.isDestroy = true;
                        gameObject.SetActive(!obj.isDestroy);
                        break;
                    }
                }
                break;
            }
        }
    }

    public void Obj_ApplyDamage(float damage)
    {
        if(isDestroy || dontDestroy) return;
        
        PlayerAttack charCon2D = CharacterController2D.instance.playerAttack;
        
        shakeValue = 0.25f;
        if (charCon2D.weapon_type != Weapon_Type.Basic)
        {
            shakeValue = 0.6f;

            if (hp > 0)
            {
                //파편 파티클 재생
                damageParticles.Play();
            
                //데미지 입기
                hp -= damage;
            
                if (charCon2D.weapon_type != Weapon_Type.Basic && charCon2D.weapon_type != Weapon_Type.etc)
                {
                    charCon2D.weaponManager.weaponLife -= 1;
                }
            }
            //부서지기
            if (hp <= 0)
            {
                StartCoroutine(BreakingEffect());
            }
        }

        if (isShakable)
        {
            objGFX.DOShakePosition(0.25f, shakeValue, 100);
        }
    }

    public IEnumerator BreakingEffect()
    {
        isDestroy = true;
        if (isShortCut)
        {
            bool sceneFound = false;

            foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
            {
                if (sceneData.sceneName == shortCutSceneName)
                {
                    Debug.Log("숏컷과 연결된 씬을 데이터상에서 발견");
                    sceneFound = true;

                    foreach (var obj in sceneData.breakableObjects)
                    {
                        if (obj.name == gameObject.name)
                        {
                            obj.isDestroy = true;
                        }
                    }
                    break;
                }
            }

            if (!sceneFound)
            {
                Debug.Log("숏컷과 연결된 이름의 씬이 데이터상에 없음");
                SceneData newSceneData = new SceneData();
                newSceneData.sceneName = shortCutSceneName;
                PlayerDataManager.instance.nowPlayer.mapData.Add(newSceneData);
             
                BreakableObjectData data = new BreakableObjectData();
                data.name = gameObject.name;
                data.isDestroy = true;
                newSceneData.breakableObjects.Add(data);
            }
        }
        
        //맵 데이터에 저장 (이 스크립트를 가지고있는 오브젝트 정보)
        foreach (var sceneData in PlayerDataManager.instance.nowPlayer.mapData)
        {
            if (sceneData.sceneName == sceneName)
            {
                foreach (var obj in sceneData.breakableObjects)
                {
                    if (obj.name == gameObject.name)
                    {
                        obj.isDestroy = true;
                        break;
                    }
                }
                //break;
            }
        }

        
        
        //통과 가능하게 전환
        collider.isTrigger = true;
        objGFX.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f)
            .OnComplete(() => { objGFX.gameObject.SetActive(false);});
        
        //부서지는 파티클 재생
        brokenParticles.Play();
        dustParticles.Play();
        yield return new WaitForSeconds(2.5f);
        gameObject.SetActive(false);
    }
}
