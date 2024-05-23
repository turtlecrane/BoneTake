using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Particle : MonoBehaviour
{
    public ParticleSystem a;
    public ParticleSystem b;
    public ParticleSystem c;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            a.Play();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            b.Play();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            c.Play();
        }

    }
}
