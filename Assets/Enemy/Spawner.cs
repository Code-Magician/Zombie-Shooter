using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// IF THE PLAYER TRIGGERS THE COLLIDER ON GAMEOBJECT TO WHICH THIS SCRIPT IS ATTACHED...
// IT WILL SPAWN COUNT NUMBER OF ZOMBIES IN THE SPHERERADIUS CIRCLE ON THE TERRAIN...
public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject[] zombies;
    [SerializeField] int count;
    [SerializeField] int sphereRadius;

    private void Start()
    {

    }



    void Spawn()
    {
        for (int i = 1; i < count; i++)
        {
            GameObject zombie = zombies[Random.Range(0, zombies.Length)];
            Vector3 randomPoint = this.transform.position + Random.onUnitSphere * sphereRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                GameObject temp = Instantiate(zombie, hit.position, Quaternion.identity);
            }
            else
                i--;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Spawn();
        }
    }
}
