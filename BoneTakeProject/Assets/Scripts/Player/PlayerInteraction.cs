using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject testInteractionText;
    public EnemyAI enemyAIscript;
    
    [SerializeField] private bool canInteraction = false;
    [SerializeField] private bool canExtractBones = false;
    [SerializeField] private bool canTalkToNPC = false;
    
    [SerializeField] private bool isExtractingBones = false;
    
    [SerializeField] private float boneExtractCount = 0f;
    [SerializeField] private float m_boneExtractionTime;
    
    private CharacterController2D charCon2D;
    private PlayerFollowCameraController followCameraController;

    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
        followCameraController = GameManager.Instance.GetPlayerFollowCameraController();
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
                CancelBoneExtraction();
            }
        }
        else
        {
            //Debug.Log("상호작용 취소됨");
            ResetBoneExtraction();
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
            Debug.Log("발골중... ");
            isExtractingBones = true;
            boneExtractCount += Time.deltaTime;
            //카메라 줌
            followCameraController.virtualCamera.m_Lens.OrthographicSize -= 0.001f;
            
            if (boneExtractCount >= m_boneExtractionTime)
            {
                Debug.Log("발골완료!");
                m_boneExtractionTime = 0f;
                ResetBoneExtraction();
                charCon2D.playerAttack.weapon_type = enemyAIscript.weaponType;  //무기교체
                enemyAIscript.enemyHitHandler.isExtracted = true;
            }
        }
    }

    private void CancelBoneExtraction()
    {
        if (canExtractBones && isExtractingBones)
        {
            Debug.Log("발골취소!");
            ResetBoneExtraction();
        }
    }

    private void ResetBoneExtraction()
    {
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
}
