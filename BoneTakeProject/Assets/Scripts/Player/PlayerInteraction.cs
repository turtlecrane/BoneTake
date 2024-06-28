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
    public DialoguePlayback dialoguePlayback;
    public ItemSelectUI itemSelectPanel;
    [HideInInspector] public EnemyAI enemyAIscript;
    [HideInInspector] public EnemyAI_Flight enemyAIscript_F;
    [HideInInspector] public BossHitHandler bossHitHandler;
    public DumpedWeapon dumpedWeapon_Prefabs;
    
    //public Transform particleTransform;
    public ParticleSystem bloodParticlePrefabs;
    private ParticleSystem bloodParticle;
    
    [Header("State")]
    public bool isInteractiveCamera;
    [SerializeField] private bool canInteraction = false;
    [SerializeField] private bool canExtractBones = false;
    [SerializeField] private bool canTalkToNPC = false;
    [SerializeField] private bool canGetWeapon = false;
    [SerializeField] private bool isExtractingBones = false;
    [SerializeField] private bool isCompleteBones = false;
    public bool isDialoguing = false;
    [SerializeField] private float boneExtractCount = 0f;
    [SerializeField] private float m_boneExtractionTime;

    [Space(10f)]
    public float boneTakeCompleteDuration;
    private CharacterController2D charCon2D;
    private PlayerFollowCameraController followCameraController;
    private WeaponData weaponDataScript;
    private Collider2D npcCollision;
    private Collider2D dumpedWeapon_Item;

    private void Start()
    {
        charCon2D = CharacterController2D.instance;
        followCameraController = GameManager.Instance.GetPlayerFollowCameraController();
        weaponDataScript = WeaponData.instance;
    }

    private void Update()
    {
        HandleInteractionInput();
        if (Input.GetKeyDown(KeyCode.G))
        {
            TakeOffWeapon();
            charCon2D.playerAttack.weapon_type = Weapon_Type.Basic;
            charCon2D.playerAttack.weapon_name = Weapon_Name.Basic;
            charCon2D.playerAttack.weaponManager.weaponLife = -1;
        }
    }

    private void HandleInteractionInput()
    {
        if (canInteraction && !isDialoguing)
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

        if (canGetWeapon)
        {
            TakeOffWeapon();

            DumpedWeapon wpscript = dumpedWeapon_Item.gameObject.GetComponent<DumpedWeapon>();
            charCon2D.playerAttack.weapon_type = wpscript.weaponType;
            charCon2D.playerAttack.weapon_name = wpscript.weaponName;
            charCon2D.playerAttack.weaponManager.weaponLife = wpscript.weaponHP;
            
            //최종적으로 떨어진 아이템 삭제함
            Destroy(dumpedWeapon_Item.gameObject);
        }
    }

    private void ContinueBoneExtraction()
    {
        if (canExtractBones)
        {
            if (enemyAIscript != null)
            {
                ExtractBones(enemyAIscript.enemyHitHandler, enemyAIscript.weaponType, enemyAIscript.weaponName);
            }
            else if (enemyAIscript_F != null)
            {
                ExtractBones(enemyAIscript_F.enemyHitHandler, enemyAIscript_F.weaponType, enemyAIscript_F.weaponName);
            }
            else if (bossHitHandler != null)
            {
                ExtractBossBones();
            }
        }
    }
    
    private void ExtractBones(EnemyHitHandler hitHandler, Weapon_Type weaponType, Weapon_Name weaponName)
    {
        PerformBoneExtraction(
            !hitHandler.isExtracted,
            () =>
            {
                AudioManager.instance.StopAndRemoveEnvironSound("BoneTaking");
                charCon2D.playerAttack.weapon_type = weaponType;
                charCon2D.playerAttack.weapon_name = weaponName;
                charCon2D.playerAttack.weaponManager.weaponLife = weaponDataScript.GetName_WeaponLifeCount(weaponName);
                hitHandler.isExtracted = true;
            }
        );
    }

    private void ExtractBossBones()
    {
        PerformBoneExtraction(
            !bossHitHandler.isExtracted,
            () =>
            {
                AudioManager.instance.StopAndRemoveEnvironSound("BoneTaking");
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
                TakeOffWeapon();
                onComplete();
                StartCoroutine(CompleteBoneTake(boneTakeCompleteDuration));
            }
        }
    }

    public void TakeOffWeapon()
    {
        //현재 플레이어가 착용중인 무기가 basic이나 etc가 아니라면
        if (PlayerDataManager.instance.nowPlayer.weaponType != Weapon_Type.Basic && PlayerDataManager.instance.nowPlayer.weaponType != Weapon_Type.etc)
        {
            AudioManager.instance.PlaySFX("GetDumpedWeapon");
            //dumpedWeapon 프리팹 생성
            GameObject wp = Instantiate(dumpedWeapon_Prefabs.gameObject, transform.position + new Vector3(0,2,0), Quaternion.identity);
            DumpedWeapon wpscript = wp.GetComponent<DumpedWeapon>();
                    
            //프리팹에 현재 착용중인 무기의 정보를 저장
            wpscript.weaponType = charCon2D.playerAttack.weapon_type;
            wpscript.weaponName = charCon2D.playerAttack.weapon_name;
            wpscript.weaponHP = charCon2D.playerAttack.weaponManager.weaponLife;
                    
            float hpPercentage = (float)charCon2D.playerAttack.weaponManager.weaponLife / weaponDataScript.GetName_WeaponLifeCount(charCon2D.playerAttack.weapon_name);
            if (hpPercentage > 0.35f)
            {
                wpscript.spriteRenderer.sprite = weaponDataScript.weaponGFXSource
                    .freshItem[weaponDataScript.GetName_WeaponID(charCon2D.playerAttack.weapon_name)];
            }
            else
            {
                wpscript.spriteRenderer.sprite = weaponDataScript.weaponGFXSource
                    .rottenItem[weaponDataScript.GetName_WeaponID(charCon2D.playerAttack.weapon_name)];
            }
                    
            Vector2 forceDirection = new Vector2(Random.Range(-1f, 1f), 1).normalized;
            wpscript.rb.AddForce(forceDirection * Random.Range(500f, 1000f));//힘의 정도
            float randomTorque = Random.Range(-500f, 500f); // 랜덤한 회전값 추가
            wpscript.rb.AddTorque(randomTorque);
        }
    }

    private void CancelBoneTake()
    {
        if (!canTalkToNPC && canExtractBones && isExtractingBones)
        {
            AudioManager.instance.StopAndRemoveEnvironSound("BoneTaking");
            ResetBoneTake();
        }
    }

    public void BloodParticleCreate()
    {
        // x 축을 -90도로 회전하는 회전값을 생성
        Quaternion spawnRotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject particle = Instantiate(bloodParticlePrefabs.gameObject, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z), spawnRotation);
        bloodParticle = particle.GetComponent<ParticleSystem>();
    }

    public void BloodParticlePlay()
    {
        if (bloodParticle != null) bloodParticle.Play();
    }

    public void BloodParticleDestroy()
    {
        if (bloodParticle != null) Destroy(bloodParticle.gameObject);
    }

    private void ResetBoneTake()
    {
        BloodParticleDestroy();
        charCon2D.animator.SetBool("IsBoneTakeIntro", false);
        charCon2D.animator.SetBool("IsBoneTaking", false);
        AudioManager.instance.StopAndRemoveEnvironSound("BoneTaking");
        boneExtractCount = 0f;
        isExtractingBones = false;
        //카메라 복구
        if (!(charCon2D.m_Rigidbody2D.velocity.y < -31.0f) && !charCon2D.playerHitHandler.isDead)
        {
            followCameraController.virtualCamera.m_Lens.OrthographicSize = followCameraController.lensOrtho_InitSize;
        }
    }

    IEnumerator CompleteBoneTake(float time)
    {
        isCompleteBones = true;
        m_boneExtractionTime = 0f;
        charCon2D.animator.SetBool("IsBoneTakeComplete", true);
        //StartCoroutine(followCameraController.CameraTargetTimeZoomIn(0.5f, 5.5f));
        yield return new WaitForSeconds(time);
        charCon2D.animator.SetBool("IsBoneTakeComplete", false);
        isCompleteBones = false;
    }
   
    ///////////////////////////////////////////////////////////////////////////////////////////////
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || 
            collision.CompareTag("MapSection") || 
            collision.CompareTag("Untagged") || 
            collision.CompareTag("BreakableObj") ||
            isDialoguing) return;
        
        if (collision.CompareTag("Enemy"))
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy_Standing"))
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
            else if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy_Flight"))
            {
                enemyAIscript_F = collision.GetComponentInParent<EnemyAI_Flight>();
                if (enemyAIscript_F.enemyHitHandler.isExtracted || !enemyAIscript_F.enemyHitHandler.isCorpseState)
                {
                    canInteraction = false;
                    return;
                }
                canExtractBones = enemyAIscript_F.enemyHitHandler.isCorpseState;
                m_boneExtractionTime = enemyAIscript_F.boneExtractionTime;
            }
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
        else if (collision.CompareTag("NPC") || collision.CompareTag("SaveNPC"))
        {
            npcCollision = collision;
            canTalkToNPC = true;
        }
        else if (collision.CompareTag("DumpedWeapon"))
        {
            dumpedWeapon_Item = collision;
            canGetWeapon = true;
        }
        canInteraction = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Map") || collision.CompareTag("MapSection") || collision.CompareTag("Untagged") || collision.CompareTag("BreakableObj")) return;

        canInteraction = false;
        enemyAIscript = null;
        enemyAIscript_F = null;
        npcCollision = null;
        bossHitHandler = null;
        dumpedWeapon_Item = null;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        { 
            canExtractBones = false;
            m_boneExtractionTime = 0;
        }
        else if (collision.CompareTag("NPC") || collision.CompareTag("SaveNPC"))
        {
            canTalkToNPC = false;
        }
        else if (collision.CompareTag("DumpedWeapon"))
        {
            canGetWeapon = false;
        }
    }
    
    /// <summary>
    /// 무기획득 모션을 실행시키기 </br>
    /// (획득한 무기의 이미지는 발골시 얻은 무기로 동적 할당)
    /// </summary>
    public void PlayWeaponGetEffect()
    {
        AudioManager.instance.PlaySFX("GetNewWeapon");
        StartCoroutine(charCon2D.playerAttack.weaponManager.WeaponGetEffect(charCon2D.playerAttack.weapon_name));
    }
}
