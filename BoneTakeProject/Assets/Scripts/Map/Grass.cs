using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    //잘림효과
    //public ParticleSystem leafParticle;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (transform.position.x - col.transform.position.x > 0) 
        {
            GetComponent<Animator>().Play("GrassMoving_L");
        }
        else 
        {
            GetComponent<Animator>().Play("GrassMoving_R");
        }
    }

    //잘림효과
    /*public void ApplyDamage(float damage)
    {
        Instantiate(leafParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }*/
}
