using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornTrap : MonoBehaviour
{
    private Transform player;
    
    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHitHandler hitHandler = other.gameObject.GetComponent<PlayerHitHandler>();
            hitHandler.Player_ApplyDamage(1, false, (player.position - transform.position).x >= 0);
        }
    }
}
