using System;
using System.Collections;
using System.Collections.Generic;
using HeathenEngineering.PhysKit;
using Unity.Mathematics;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        transform.up = rb.velocity;
    }
    
    public void OnPathEnd(float2 velocity)
    {
        Invoke(nameof(TimeKill), 5);
    }
    
    void TimeKill()
    {
        Destroy(gameObject);
    }
}
