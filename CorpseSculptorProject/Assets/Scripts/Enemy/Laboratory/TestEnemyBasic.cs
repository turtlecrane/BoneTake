using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestEnemyBasic : MonoBehaviour
{
    public float life;
    public bool isCorpseState;
    
    
    private Rigidbody2D rb;
    private bool isInvincible = false;
    //private bool isHitted = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (life <= 0)
        {
            StartCoroutine(EnemyKnockdown());
        }

        //테스트용------
        if (isCorpseState)
        {
            var b = this.GetComponentInChildren<TextMeshPro>();
            b.text = "[시체 상태]";
        }
    }
    
    public void ApplyDamage(float damage) {
        if (!isInvincible) 
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            //transform.GetComponent<Animator>().SetBool("Hit", true);
            life -= damage;//라이프 차감
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(direction * 3500f, 0));//넉백
            StartCoroutine(HitTime());
        }
    }
    
    IEnumerator HitTime()
    {
        //isHitted = true;
        isInvincible = true;
        yield return new WaitForSeconds(0.1f);
        //isHitted = false;
        isInvincible = false;
    }
    
    IEnumerator EnemyKnockdown()
    {
        isInvincible = true;
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        /*capsule.size = new Vector2(1f, 1f);
        capsule.offset = new Vector2(0f, -0.5f);
        capsule.direction = CapsuleDirection2D.Horizontal;*/
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(0.25f);
        capsule.isTrigger = true;
        yield return new WaitForSeconds(1f);
        isCorpseState = true;
        
        /*
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        yield return new WaitForSeconds(0.25f);
        rb.velocity = new Vector2(0, rb.velocity.y);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
        */

    }
    
}
