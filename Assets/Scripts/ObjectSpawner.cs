using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public float sphereRadius;
    public GameObject objectToSpawn;
    public float minDelay = 0.0f;
    public float maxDelay = 5.0f;
    public bool spawn = true;

    void Start()
    {
        StartCoroutine(Spawner());
    }


    IEnumerator Spawner()
    {
        while(spawn)
        {
             yield return new WaitForSeconds(Random.Range(minDelay,maxDelay));
            Vector3 SpawnerPosition = this.transform.position;
            Vector3 newRandomPos = new Vector3(SpawnerPosition.x+ Random.Range(sphereRadius * (-1.0f), sphereRadius), SpawnerPosition.y ,SpawnerPosition.z + Random.Range(sphereRadius * (-1.0f), sphereRadius));
            Instantiate(objectToSpawn,newRandomPos,Quaternion.identity);
        }
       
       yield return null;
    }
}
