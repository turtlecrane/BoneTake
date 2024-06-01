using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Component")]
    public GameObject testInteractionText;
    public DialoguePlayback dialoguePlayback;
    public ItemSelectUI itemSelectPanel;
    [HideInInspector] public EnemyAI enemyAIscript;
    [HideInInspector] public BossHitHandler bossHitHandler;
    public DumpedWeapon dumpedWeapon;
    
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
            //Debug.Log("NPC와 상호작용");
            npcCollision.gameObject.SendMessage("NpcInteraction",dialoguePlayback);
        }
    }

    private void ContinueBoneExtraction()
    {
        if (bossHitHandler == null && enemyAIscript != null && canExtractBones)
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
        else if (bossHitHandler != null && enemyAIscript == null && canExtractBones)
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
                //1. 현재 플레이어가 착용중인 무기가 basic이나 etc가 아니라면,
                if (PlayerDataManager.instance.nowPlayer.weaponType != Weapon_Type.Basic)
                {
                    //2. dumpedWeapon 프리팹 생성 (이 스크립트를 가진 오브젝트의 위치의 약간 위에서)
                    GameObject wp = Instantiate(dumpedWeapon.gameObject, transform.position + Vector3.up, Quaternion.identity);
                    DumpedWeapon wpscript = wp.GetComponent<DumpedWeapon>();
                    
                    //3. 프리팹에 현재 착용중인 무기의 정보를 저장
                    wpscript.weaponType = charCon2D.playerAttack.weapon_type;
                    wpscript.weaponName = charCon2D.playerAttack.weapon_name;
                    wpscript.weaponHP = charCon2D.playerAttack.weaponManager.weaponLife;
                    SetWeaponIcon(wpscript.spriteRenderer.sprite, charCon2D.playerAttack.weapon_name);
                    
                    //4. 프리팹을 윗방향으로 힘을 주기 (마치 분수처럼 랜덤한 윗방향으로)
                    Vector2 forceDirection = new Vector2(Random.Range(-1f, 1f), 1).normalized;
                    wpscript.rb.AddForce(forceDirection * Random.Range(1000f, 1000f)); // 힘의 정도를 조절하세요.
                }
                onComplete(); //5. 그다음 무기교체(or 무기선택 연출)
                StartCoroutine(CompleteBoneTake(boneTakeCompleteDuration));
            }
        }
    }

    private void SetWeaponIcon(Sprite _sprite, Weapon_Name _weaponName)
    {
        float hpPercentage = (float)charCon2D.playerAttack.weaponManager.weaponLife / weaponDataScript.GetName_WeaponLifeCount(_weaponName);
        Debug.Log(hpPercentage);
        /*if (hpPercentage > 0.35f)
        {
            _sprite = weaponDataScript.weaponGFXSource.freshIcon[weaponDataScript.GetName_WeaponID(_weaponName)];
        }
        else
        {
            _sprite = weaponDataScript.weaponGFXSource.rottenIcon[weaponDataScript.GetName_WeaponID(_weaponName)];
        }*/

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
