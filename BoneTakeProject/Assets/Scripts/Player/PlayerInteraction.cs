using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Component")]
    public GameObject testInteractionText;
    public DialoguePlayback dialoguePlayback;
    public ItemSelectUI itemSelectPanel;
    [HideInInspector] public EnemyAI enemyAIscript;
    [HideInInspector] public BossHitHandler bossHitHandler;
    
    [Header("State")]
    public bool isInteractiveCamera;
    [SerializeField] private bool canInteraction = false;
    [SerializeField] private bool canExtractBones = false;
    [SerializeField] private bool canTalkToNPC = false;
    [SerializeField] private bool isExtractingBones = false;
    [SerializeField] private bool isCompleteBones = false;
    [SerializeField] private float boneExtractCount = 0f;
    [SerializeField] private float m_boneExtractionTime;

    [Space(10f)]
    public float boneTakeCompleteDuration;
    private CharacterController2D charCon2D;
    private PlayerFollowCameraController followCameraController;
    private WeaponData weaponDataScript;
    private Collider2D npcCollision;

    private void Start()
    {
        charCon2D = CharacterController2D.instance;
        followCameraController = GameManager.Instance.GetPlayerFollowCameraController();
        weaponDataScript = WeaponData.instance;
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
            npcCollision.gameObject.SendMessage("NpcInteraction",dialoguePlayback);//DialoguePlayback
        }
    }

    private void ContinueBoneExtraction()
    {
        if (bossHitHandler == null && enemyAIscript != null)
        {
            PerformBoneExtraction(
                !enemyAIscript.enemyHitHandler.isExtracted,
                () =>
                {
                    charCon2D.playerAttack.weapon_type = enemyAIscript.weaponType;
                    charCon2D.playerAttack.weapon_name = enemyAIscript.weaponName;
                    charCon2D.playerAttack.weaponManager.weaponLife = weaponDataScript.GetName_WeaponLifeCount(enemyAIscript.weaponName);
                    enemyAIscript.enemyHitHandler.isExtracted = true;
                }
            );
        }
        else if (bossHitHandler != null && enemyAIscript == null)
        {
            PerformBoneExtraction(
                !bossHitHandler.isExtracted,
                () =>
                {
                    foreach (var weapon in bossHitHandler.weaponName)
                    {
                        itemSelectPanel.CreateItemSelection(weapon, () =>
                        {
                            StartCoroutine(CompleteBoneTake(boneTakeCompleteDuration));
                        });
                        itemSelectPanel.gameObject.SetActive(true);
                        itemSelectPanel.gameObject.GetComponent<Image>()
                            .DOFade(0.8f, 1f)
                            .SetUpdate(UpdateType.Normal, true)
                            .OnComplete(() =>
                            {
                                itemSelectPanel.SortSelectedItems();
                            });
                    }
                    bossHitHandler.isExtracted = true;
                }
            );
        }
    }

    private void PerformBoneExtraction(bool condition, Action onComplete)
    {
        if (!canTalkToNPC && canExtractBones && m_boneExtractionTime != 0f && condition)
        {
            charCon2D.animator.SetBool("IsBoneTaking", true);
            charCon2D.animator.SetBool("IsBoneTakeIntro", true);
            isExtractingBones = true;
            boneExtractCount += Time.deltaTime;
            followCameraController.virtualCamera.m_Lens.OrthographicSize -= 0.01f;

            if (boneExtractCount >= m_boneExtractionTime)
            {
                onComplete();
                StartCoroutine(CompleteBoneTake(boneTakeCompleteDuration));
            }
        }
    }

    private void CancelBoneTake()
    {
        if (!canTalkToNPC && canExtractBones && isExtractingBones)
        {
            ResetBoneTake();
        }
    }

    public void SendBloodParticlePlay()
    {
        if(enemyAIscript != null) enemyAIscript.SendMessage("PlayBloodParticle");
        if(bossHitHandler != null) bossHitHandler.SendMessage("PlayBloodParticle");
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
        if (collision.CompareTag("Map") || collision.CompareTag("MapSection") || collision.CompareTag("Untagged")) return;
        
        if (collision.CompareTag("Enemy"))
        {
            enemyAIscript = collision.GetComponent<EnemyAI>();
            if (enemyAIscript.enemyHitHandler.isExtracted || !enemyAIscript.enemyHitHandler.isCorpseState)
            {
                canInteraction = false;
                return;
            }
            canExtractBones = enemyAIscript.enemyHitHandler.isCorpseState;
            m_boneExtractionTime = enemyAIscript.boneExtractionTime;
        }
        else if (collision.CompareTag("Boss"))
        {
            bossHitHandler = collision.GetComponentInParent<BossHitHandler>();
            if (bossHitHandler.isExtracted || !bossHitHandler.isCorpseState)
            {
                canInteraction = false;
                return;
            }
            canExtractBones = bossHitHandler.isCorpseState;
            m_boneExtractionTime = bossHitHandler.boneExtractionTime;
        }
        else if (collision.CompareTag("NPC"))
        {
            npcCollision = collision;
            canTalkToNPC = true;
        }
        canInteraction = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("MapSection") || collision.CompareTag("Untagged")) return;

        canInteraction = false;
        enemyAIscript = null;
        npcCollision = null;
        bossHitHandler = null;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
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
        StartCoroutine(charCon2D.playerAttack.weaponManager.WeaponGetEffect(charCon2D.playerAttack.weapon_name));
    }
}
