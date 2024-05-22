using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Particle : MonoBehaviour
{
    public ParticleSystem a;
    public ParticleSystem b;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            a.Play();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            b.Play();
        }

    }
}
