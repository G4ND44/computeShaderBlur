using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDestroyer : MonoBehaviour
{
    public GameObject bombObject;
    public ParticleSystem bombParticleSystem;

    bool markParticlesForDestory = false;
    void Update()
    {
        if(markParticlesForDestory)
        {
            if(!bombParticleSystem.isPlaying)
            {
                Destroy(this.gameObject);
            }
        }
    }

     void OnCollisionEnter(Collision collision)
    {
        Destroy(bombObject);
        markParticlesForDestory = true;
        bombParticleSystem.Play();
    }
}
