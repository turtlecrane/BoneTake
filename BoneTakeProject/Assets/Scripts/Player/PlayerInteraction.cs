using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject testInteractionText;
    public bool canInteraction = false;
    public bool canExtractBones = false;
    public bool canTalkToNPC = false;

    private void Update()
    {
        testInteractionText.SetActive(canInteraction);
        if (Input.GetKey(KeyCode.F))
        {
            if (canExtractBones)
            {
                Debug.Log("발골중... ");
            }
            if (canTalkToNPC)
            {
                Debug.Log("NPC와 상호작용");
            }
            else
            {
                return;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("Untagged")) return;
        
        canInteraction = true;

        if (collision.CompareTag("Enemy"))
        {
            EnemyAI enemyScript = collision.GetComponent<EnemyAI>();
            canExtractBones = enemyScript.enemyHitHandler.isCorpseState;
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
        }
        else if (collision.CompareTag("NPC"))
        {
            canTalkToNPC = false;
        }
    }
}
