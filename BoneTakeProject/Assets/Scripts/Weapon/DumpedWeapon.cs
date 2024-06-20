using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumpedWeapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public Weapon_Type weaponType;
    public Weapon_Name weaponName;
    public int weaponHP;
    
    [Header("Component")]
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Collider2D collider;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ChangeState();
        }
    }

    private void ChangeState()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        collider.isTrigger = true;
    }
}
