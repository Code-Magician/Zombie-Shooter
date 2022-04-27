using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] float approachDistance = 20;

    Animator anim;
    NavMeshAgent agent;

    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Vector3.Distance(target.position, this.gameObject.transform.position) <= approachDistance)
        {
            agent.SetDestination(target.position);
            if (agent.remainingDistance > agent.stoppingDistance)
            {
                if (!anim.GetBool("Walk"))
                {
                    anim.SetBool("Walk", true);
                    anim.SetBool("Attack", false);
                }
            }
            else
            {
                if (anim.GetBool("Walk"))
                {
                    anim.SetBool("Walk", false);
                    anim.SetBool("Attack", true);
                }
            }
        }
        else
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Attack", false);
        }

        // if (Input.GetKey(KeyCode.A))
        //     anim.SetBool("Walk", true);
        // else
        //     anim.SetBool("Walk", false);

        // if (Input.GetKey(KeyCode.S))
        //     anim.SetBool("Run", true);
        // else
        //     anim.SetBool("Run", false);

        // if (Input.GetKey(KeyCode.D))
        //     anim.SetBool("Attack", true);
        // else
        //     anim.SetBool("Attack", false);

        // if (Input.GetKey(KeyCode.F))
        //     anim.SetBool("Death", true);
        // else
        //     anim.SetBool("Death", false);
    }
}
