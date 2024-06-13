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
    public Weapon_Type breakableWeapon; //부서질수 있는 무기의 종류
    
    public ParticleSystem damageParticles; //파편 파티클
    public ParticleSystem brokenParticles; //부서지는 파티클
    public ParticleSystem dustParticles;

    public string sceneName;
    
    private float shakeValue;
    private Collider2D collider;

    public bool isRemembered;
    public bool isShakable;
    public bool isDestroy = false;
    
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
        if(isDestroy) return;
        
        PlayerAttack charCon2D = CharacterController2D.instance.playerAttack;
        
        shakeValue = 0.25f;
        if (charCon2D.weapon_type == breakableWeapon)
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
                Debug.Log("부서졌습니다.");
                StartCoroutine(BreakingEffect());
            }
        }

        if (isShakable)
        {
            //흔들리기
            objGFX.DOShakePosition(0.25f, shakeValue, 100);
        }
    }

    public IEnumerator BreakingEffect()
    {
        isDestroy = true;
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
                break;
            }
        }
        //PlayerDataManager.instance.nowPlayer.mapData
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
