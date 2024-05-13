using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject testInteractionText;
    public EnemyAI enemyAIscript;
    public float boneTakeCompleteDuration;

    public bool isInteractiveCamera;
    [SerializeField] private bool canInteraction = false;
    [SerializeField] private bool canExtractBones = false;
    [SerializeField] private bool canTalkToNPC = false;
    
    [SerializeField] private bool isExtractingBones = false;
    [SerializeField] private bool isCompleteBones = false;
    
    [SerializeField] private float boneExtractCount = 0f;
    [SerializeField] private float m_boneExtractionTime;
    
    private CharacterController2D charCon2D;
    private PlayerFollowCameraController followCameraController;
    private WeaponData weaponDataScript;

    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        followCameraController = GameManager.Instance.GetPlayerFollowCameraController();
        weaponDataScript = GameManager.Instance.GetWeaponData();
    }

    private void Update()
    {
        testInteractionText.SetActive(canInteraction);
        HandleInteractionInput();
    }

    private void HandleInteractionInput()
    {
        if (canInteraction)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartInteraction();
            }
    
            if (Input.GetKey(KeyCode.F))
            {
                ContinueBoneExtraction();
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                CancelBoneTake();
            }
        }
        else
        {
            //Debug.Log("상호작용 취소됨");
            if (!isCompleteBones)
            {
                ResetBoneTake();
            }
        }
    }

    private void StartInteraction()
    {
        if (canTalkToNPC)
        {
            Debug.Log("NPC와 상호작용");
        }
    }

    private void ContinueBoneExtraction()
    {
        if (!enemyAIscript.enemyHitHandler.isExtracted&& canExtractBones && m_boneExtractionTime != 0f)
        {
            charCon2D.animator.SetBool("IsBoneTaking", true);
            charCon2D.animator.SetBool("IsBoneTakeIntro", true);
            isExtractingBones = true;
            boneExtractCount += Time.deltaTime;
            //카메라 줌
            followCameraController.virtualCamera.m_Lens.OrthographicSize -= 0.001f;
            
            if (boneExtractCount >= m_boneExtractionTime)
            {
                
                charCon2D.playerAttack.weapon_type = enemyAIscript.weaponType;  //무기타입 교체
                charCon2D.playerAttack.weapon_name = enemyAIscript.weaponName;  //무기이름 교체
                charCon2D.playerAttack.weaponManager.weaponLife = weaponDataScript.GetName_WeaponLifeCount(enemyAIscript.weaponName); //무기 이름에따른 무기 HP 부여
                StartCoroutine(CompleteBoneTake(boneTakeCompleteDuration));
                enemyAIscript.enemyHitHandler.isExtracted = true; //발골된 시체임을 구분
            }
        }
    }

    private void CancelBoneTake()
    {
        if (canExtractBones && isExtractingBones)
        {
            ResetBoneTake();
        }
    }

    private void ResetBoneTake()
    {
        charCon2D.animator.SetBool("IsBoneTakeIntro", false);
        charCon2D.animator.SetBool("IsBoneTaking", false);
        boneExtractCount = 0f;
        isExtractingBones = false;
        //카메라 복구
        if (!(charCon2D.m_Rigidbody2D.velocity.y < -31.0f) &&
            !charCon2D.playerHitHandler.isDead
            )
        {
            followCameraController.virtualCamera.m_Lens.OrthographicSize = followCameraController.lensOrtho_InitSize;
        }
    }

    IEnumerator CompleteBoneTake(float time)
    {
        isCompleteBones = true;
        m_boneExtractionTime = 0f;
        charCon2D.animator.SetBool("IsBoneTakeComplete", true);
        StartCoroutine(followCameraController.CameraTargetTimeZoomIn(0.5f, 5.5f));
        yield return new WaitForSeconds(time);
        charCon2D.animator.SetBool("IsBoneTakeComplete", false);
        isCompleteBones = false;
    }
   
    ///////////////////////////////////////////////////////////////////////////////////////////////
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("Untagged")) return;
        
        if (collision.CompareTag("Enemy"))
        {
            enemyAIscript = collision.GetComponent<EnemyAI>();
            if (enemyAIscript.enemyHitHandler.isExtracted)
            {
                canInteraction = false;
                return;
            }
            canExtractBones = enemyAIscript.enemyHitHandler.isCorpseState;
            m_boneExtractionTime = enemyAIscript.boneExtractionTime;
        }
        else if (collision.CompareTag("NPC"))
        {
            canTalkToNPC = true;
        }
        
        canInteraction = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("Untagged")) return;

        canInteraction = false;
        enemyAIscript = null;

        if (collision.CompareTag("Enemy"))
        { 
            canExtractBones = false;
            m_boneExtractionTime = 0;
        }
        else if (collision.CompareTag("NPC"))
        {
            canTalkToNPC = false;
        }
    }
    
    /// <summary>
    /// 무기획득 모션을 실행시키기 </br>
    /// (획득한 무기의 이미지는 발골시 얻은 무기로 동적 할당)
    /// </summary>
    public void PlayWeaponGetEffect()
    {
        charCon2D.playerAttack.weaponManager.WeaponGetEffect(charCon2D.playerAttack.weapon_name);
    }
}
