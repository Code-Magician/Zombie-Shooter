using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// CONTROLS THE STATE MACHINE, SOUNDS AND HEALTH OF EACH ZOMBIE...
public class ZombieController : MonoBehaviour
{
    // ALL THE STATES OF THE ZOMBIES...
    public enum STATE
    {
        IDLE,
        WANDER,
        CHASE,
        ATTTACK,
        DEATH
    }
    // THE STATE IN WHICH  THE ZOMBIE IS CURRENTLY IN...
    public STATE state = STATE.IDLE;


    [Header("References")]
    public Transform target;

    // THE DAMAGE IT WILL DO TO PLAYER...
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float walkingSpeed = 1f;
    [SerializeField] float runningSpeed = 5f;

    // THE MAX DISTANCE AT WHICH ZOMBIE CAN SEE PLAYER...
    [SerializeField] float approachDistance = 20;

    // THE MIN DISTANCE AT WHICH THE ZOMBIE CAN'T SEE THE PLAYER...
    [SerializeField] float forgetPlayerDistance = 30f;

    // THE MAX DISTANCE AT WHICH THE PLAYER CAN START ATTACKING THE PLAYER...
    [SerializeField] float attackDistace = 2f;
    public GameObject ragDoll;

    // AMOUNT OF HEATH THE RIFLE BULLET OF PLAYER WILL DO TO THIS ZOMBIE...
    [SerializeField] int bulletDamage;

    [SerializeField] AudioSource AttackAudioSource;
    [SerializeField] AudioClip[] AttackClips;

    // ZOMBIE ROAR SOUND AUDIOSOURCE AND THE MAX AND MIN DELAY BETWEEN EACH TIME THE SOUND PLAYS...
    [SerializeField] AudioSource zombieSoundAudioSource;
    [SerializeField] float minWait;
    [SerializeField] float maxWait;


    Animator anim;
    NavMeshAgent agent;
    bool isAlive = true;
    int health = 100;



    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating("PlayZombieSound", 0, Random.Range(minWait, maxWait));
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

                AudioSource[] audioS = gameObject.GetComponents<AudioSource>();
                foreach (AudioSource x in audioS)
                    x.volume = 0;


                isAlive = false;
                break;
        }
    }

    // TOGGLES ALL THE TRIGGERS OF THE ZOMBIE ANIMATOR TO FALSE...
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


    // EVERY TIME THIS FUNCTION IS CALLED THE BULLETEDAMAGE AMOUNT OF HEALTH WILL BE REDUCED FROM THE ZOMBIE HEALTH
    // AND WHEN THE HEALTH OF ZOMBIE REACHES ZERO IT WILL RUN DIE ANIMAITON OR WILL TURN TO RAGDOLL...
    public void KillSelf()
    {
        health -= bulletDamage;

        if (health <= 0)
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


    public void PlayZombieSound()
    {
        zombieSoundAudioSource.Play();
    }
}
