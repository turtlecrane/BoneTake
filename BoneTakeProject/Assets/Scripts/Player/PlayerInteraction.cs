using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject testInteractionText;
    [SerializeField] private bool canInteraction = false;
    [SerializeField] private bool canExtractBones = false;
    [SerializeField] private bool canTalkToNPC = false;
    
    [SerializeField] private bool isExtractingBones = false;
    
    [SerializeField] private float boneExtractCount = 0f;
    [SerializeField] private float m_boneExtractionTime;
    
    private CharacterController2D charCon2D;
    private Weapon_Type getWeaponType;

    private void Start()
    {
        charCon2D = GameManager.Instance.GetCharacterController2D();
    }

    private void Update()
    {
        testInteractionText.SetActive(canInteraction);
        HandleInteractionInput();
    }

    private void HandleInteractionInput()
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

    private void StartInteraction()
    {
        if (canTalkToNPC)
        {
            Debug.Log("NPC와 상호작용");
        }
    }

    private void ContinueBoneExtraction()
    {
        if (canExtractBones && m_boneExtractionTime != 0f)
        {
            Debug.Log("발골중... ");
            isExtractingBones = true;
            boneExtractCount += Time.deltaTime;
            
            if (boneExtractCount >= m_boneExtractionTime)
            {
                Debug.Log("발골완료!");
                m_boneExtractionTime = 0f;
                ResetBoneExtraction();
                charCon2D.playerAttack.weapon_type = getWeaponType;
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
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("Untagged")) return;
        
        canInteraction = true;

        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemyScript = collision.GetComponent<EnemyAI>();
            canExtractBones = enemyScript.enemyHitHandler.isCorpseState;
            m_boneExtractionTime = enemyScript.boneExtractionTime;
            getWeaponType = enemyScript.weaponType;
        }
        else if (collision.CompareTag("NPC"))
        {
            canTalkToNPC = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("Untagged")) return;

        canInteraction = false;

        if (collision.CompareTag("Enemy"))
        { 
            canExtractBones = false;
            m_boneExtractionTime = 0;
            getWeaponType = Weapon_Type.etc;
        }
        else if (collision.CompareTag("NPC"))
        {
            canTalkToNPC = false;
        }
    }
}
