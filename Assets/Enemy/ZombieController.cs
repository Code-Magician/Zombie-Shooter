using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    enum STATE
    {
        IDLE,
        WANDER,
        CHASE,
        ATTTACK,
        DEATH
    }
    STATE state = STATE.IDLE;


    [Header("References")]
    public Transform target;
    [SerializeField] float walkingSpeed = 1f;
    [SerializeField] float runningSpeed = 5f;
    [SerializeField] float approachDistance = 20;
    [SerializeField] float forgetPlayerDistance = 30f;
    [SerializeField] float attackDistace = 2f;
    [SerializeField] GameObject ragDoll;


    Animator anim;
    NavMeshAgent agent;


    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (Random.Range(0, 100) < 50)
            {
                GameObject temp = Instantiate(ragDoll, transform.position, transform.rotation);
                temp.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.gameObject.transform.forward * 500, ForceMode.Impulse);
                Destroy(gameObject);
                return;
            }
            else
            {
                state = STATE.DEATH;
            }

        }

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
            return;
        }

        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                    state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 10)
                    state = STATE.WANDER;
                break;

            case STATE.WANDER:
                // Move zombie to random target...
                if (!agent.hasPath)
                {
                    float x = transform.position.x + Random.Range(-5f, 5f);
                    float z = transform.position.z + Random.Range(-5f, 5f);
                    float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
                    Vector3 tempTarget = new Vector3(x, y, z);

                    agent.SetDestination(tempTarget);
                    agent.stoppingDistance = 0;
                    agent.speed = walkingSpeed;

                    ToggleAnimationTriggers();
                    anim.SetBool("Walk", true);
                }
                if (CanSeePlayer())
                    state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 10)
                {
                    state = STATE.IDLE;
                    ToggleAnimationTriggers();
                    agent.ResetPath();
                }
                break;

            case STATE.CHASE:
                agent.SetDestination(target.position);
                agent.stoppingDistance = attackDistace;
                agent.speed = runningSpeed;

                ToggleAnimationTriggers();
                anim.SetBool("Run", true);

                if (InAttackRange() && !agent.pathPending)
                {
                    state = STATE.ATTTACK;
                }
                if (ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }
                break;

            case STATE.ATTTACK:
                if (!anim.GetBool("Attack"))
                {
                    ToggleAnimationTriggers();
                    anim.SetBool("Attack", true);
                }
                this.transform.LookAt(target.position);

                if (OutOfAttackRange())
                    state = STATE.CHASE;
                break;

            case STATE.DEATH:
                ToggleAnimationTriggers();
                anim.SetBool("Death", true);
                break;
        }
    }

    private void ToggleAnimationTriggers()
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Run", false);
        anim.SetBool("Attack", false);
        anim.SetBool("Death", false);
    }


    bool CanSeePlayer()
    {
        if (Vector3.Distance(this.transform.position, target.position) <= approachDistance)
            return true;

        return false;
    }

    bool ForgetPlayer()
    {
        if (Vector3.Distance(this.transform.position, target.position) > forgetPlayerDistance)
            return true;

        return false;
    }

    bool OutOfAttackRange()
    {
        return Vector3.Distance(target.position, this.transform.position) > agent.stoppingDistance + 2f;
    }

    bool InAttackRange()
    {
        return (agent.remainingDistance <= agent.stoppingDistance);
    }
}
