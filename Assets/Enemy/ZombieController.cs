using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public enum STATE
    {
        IDLE,
        WANDER,
        CHASE,
        ATTTACK,
        DEATH
    }
    public STATE state = STATE.IDLE;


    [Header("References")]
    public Transform target;
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float walkingSpeed = 1f;
    [SerializeField] float runningSpeed = 5f;
    [SerializeField] float approachDistance = 20;
    [SerializeField] float forgetPlayerDistance = 30f;
    [SerializeField] float attackDistace = 2f;
    public GameObject ragDoll;

    [SerializeField] AudioSource AttackAudioSource;
    [SerializeField] AudioClip[] AttackClips;


    Animator anim;
    NavMeshAgent agent;
    bool isAlive = true;


    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Mouse2))
        // {
        //     if (Random.Range(0, 100) < 50)
        //     {
        //         GameObject temp = Instantiate(ragDoll, transform.position, transform.rotation);
        //         temp.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.gameObject.transform.forward * 500, ForceMode.Impulse);
        //         Destroy(gameObject);
        //         return;
        //     }
        //     else
        //     {
        //         state = STATE.DEATH;
        //     }
        // }

        if (!GameStats.gameOver && target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
            return;
        }

        switch (state)
        {
            case STATE.IDLE:
                if (!GameStats.gameOver && CanSeePlayer())
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

                if (!GameStats.gameOver && CanSeePlayer())
                    state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 10)
                {
                    state = STATE.IDLE;
                    ToggleAnimationTriggers();
                    agent.ResetPath();
                }
                break;

            case STATE.CHASE:
                if (!GameStats.gameOver)
                {
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
                }
                else
                {
                    GameOver();
                }
                break;

            case STATE.ATTTACK:
                if (!GameStats.gameOver)
                {
                    if (!anim.GetBool("Attack"))
                    {
                        ToggleAnimationTriggers();
                        anim.SetBool("Attack", true);
                    }

                    Vector3 lookAtPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
                    this.transform.LookAt(lookAtPos);

                    if (OutOfAttackRange())
                        state = STATE.CHASE;
                }
                else
                {
                    GameOver();
                }
                break;

            case STATE.DEATH:
                Destroy(agent);
                ToggleAnimationTriggers();
                anim.SetBool("Death", true);

                if (isAlive)
                {
                    Sink sink = GetComponent<Sink>();
                    if (sink != null)
                        sink.StartSink();
                }
                isAlive = false;
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


    public void KillSelf()
    {
        if (CanSeePlayer())
        {
            GameObject temp = Instantiate(ragDoll, transform.position, transform.rotation);
            temp.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.gameObject.transform.forward * 100, ForceMode.Impulse);
            Destroy(gameObject);
            return;
        }
        else
        {
            state = STATE.DEATH;
        }
    }


    private void DamagePlayer()
    {
        if (target != null)
        {
            target.GetComponent<FPController>().TakeDamage(damageAmount);
            RandomAttackSound();
        }
        else
        {
            Invoke("GameOver", 5f);
        }
    }


    private void GameOver()
    {
        state = STATE.WANDER;
        agent.ResetPath();
    }


    public void RandomAttackSound()
    {
        int idx = Random.Range(1, AttackClips.Length);
        AudioClip clip = AttackClips[idx];

        AttackAudioSource.Stop();
        AttackAudioSource.clip = clip;
        AttackAudioSource.Play();


        AudioClip temp = clip;
        AttackClips[idx] = AttackClips[0];
        AttackClips[0] = temp;
    }
}
