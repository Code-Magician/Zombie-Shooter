using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FPController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float sprintSpeed = 20f;
    [SerializeField] float jumpForce = 300f;
    [Range(0, 10)][SerializeField] float sensitivity = 1f;


    [Header("References")]
    [SerializeField] Camera fpsCamera;
    [SerializeField] Animator anim;
    [SerializeField] GameObject aim;
    [SerializeField] GameObject fullBodyModel;
    [SerializeField] AudioSource[] footSteps;
    [SerializeField] AudioSource jump;
    [SerializeField] AudioSource land;
    [SerializeField] AudioSource ammokitAudio;
    [SerializeField] AudioSource medkitAudio;
    [SerializeField] AudioSource outOfAmmo;
    [SerializeField] AudioSource dealth;
    [SerializeField] AudioSource reload;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject bloodParticles;


    [Header("UI References")]
    [SerializeField] Slider healthBar;
    [SerializeField] Text totalBulletsText;
    [SerializeField] Text bulletsInGunText;
    [SerializeField] Slider compass;
    [SerializeField] GameObject bloodSplaterPrefab;
    [SerializeField] GameObject canvas;


    Rigidbody rb;
    CapsuleCollider fpsCollider;
    Quaternion camRotation, fpsRotation;
    bool cursorIsLocked = true;
    float x, z;
    bool isDead = false;
    bool playingWalking = false;
    bool previouslyGrounded = true;
    float canvasWidth, canvasHeight;


    // Inventory
    int ammo = 50;
    int maxAmmo = 50;
    int ammoClip = 10;
    int maxAmmoClip = 10;
    int health = 100;
    int maxHealth = 100;


    private void Awake()
    {
        LockCursor(true);
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fpsCollider = GetComponent<CapsuleCollider>();

        camRotation = fpsCamera.transform.localRotation;
        fpsRotation = this.transform.localRotation;

        health = maxHealth;
        ammoClip = maxAmmoClip;
        ammo = maxAmmo;

        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        RefreshHealthBar();
        RefreshBulletsInGun();
        RefreshTotalBulletText();
    }


    // Interval between 2 Fixed Update Functions is not constant...
    void Update()
    {
        SetCompass();

        // FPC Jump
        bool grounded = IsOnGround();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(0, jumpForce, 0);
            jump.Play();
            if (anim.GetBool("WalkWithWeapon"))
            {
                CancelInvoke("RandomFootsteps");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }
        previouslyGrounded = grounded;


        // Gets Weapon
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool("Weapon", !anim.GetBool("Weapon"));

        // Enable Aim
        if (anim.GetBool("Weapon"))
            aim.SetActive(true);
        else
            aim.SetActive(false);

        // Fire 
        if (Input.GetKeyDown(KeyCode.Mouse0) && anim.GetBool("Weapon") && GameStats.canShoot && !EventSystem.current.IsPointerOverGameObject())
        {
            if (ammoClip > 0)
            {
                Shoot();
            }
            else
            {
                outOfAmmo.Play();
            }
            // Debug.Log("AmmoClip: " + ammoClip);
            // Debug.Log("Ammo: " + ammo);
        }

        // Reload Gun
        if (Input.GetKeyDown(KeyCode.R) && anim.GetBool("Weapon"))
        {
            anim.SetTrigger("Reload");

            // Reloading ammoclip
            int ammoNeeded = maxAmmoClip - ammoClip;
            int ammoAvailable = Mathf.Min(ammoNeeded, ammo);
            ammoClip += ammoAvailable;
            ammo -= ammoAvailable;
            reload.Play();

            RefreshTotalBulletText();
            RefreshBulletsInGun();

            // Debug.Log("Ammo Clip: " + ammoClip);
            // Debug.Log("Ammo: " + ammo);
        }

        // Walking with Weapon
        if (x != 0 || z != 0)
        {
            if (!anim.GetBool("WalkWithWeapon"))
            {
                anim.SetBool("WalkWithWeapon", true);
                if (!playingWalking)
                    InvokeRepeating("RandomFootsteps", 0, 0.4f);
            }
        }
        else if (anim.GetBool("WalkWithWeapon"))
        {
            anim.SetBool("WalkWithWeapon", false);
            CancelInvoke("RandomFootsteps");
            playingWalking = false;
        }

        // // Check if dead
        // if (health <= 0 && !isDead)
        // {
        //     isDead = true;
        //     dealth.Play();
        // }
    }



    // Interval between 2 Fixed Update Functions is always constant...
    private void FixedUpdate()
    {
        // Moving FPC
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(x * sprintSpeed * Time.deltaTime, 0, z * sprintSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.Translate(x * movementSpeed * Time.deltaTime, 0, z * movementSpeed * Time.deltaTime, Space.Self);
        }


        // Rotating FPS with mouse
        // Mouse ko horizonal move kroge to FPS Local Y-Axis pr rotate hoga.
        float yAxisRot = Input.GetAxis("Mouse X");
        // Mouse ko Vertical move kroge to Camera Local X-Axis pr rotate hoga.
        float xAxisRot = Input.GetAxis("Mouse Y");

        camRotation *= Quaternion.Euler(-xAxisRot * sensitivity, 0, 0);
        camRotation = ClampRotationXAxis(camRotation);
        fpsRotation *= Quaternion.Euler(0, yAxisRot * sensitivity, 0);

        fpsCamera.transform.localRotation = camRotation;
        this.transform.localRotation = fpsRotation;


        // Toggle Cursor
        ToggleCursor();
    }



    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ammo" && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            ammokitAudio.Play();
            // Debug.Log("Ammo: " + ammo);
            Destroy(other.gameObject);

            RefreshTotalBulletText();
        }
        else if (other.gameObject.tag == "Med" && health < maxHealth)
        {
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            RefreshHealthBar();

            medkitAudio.Play();
            // Debug.Log("Health: " + health);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Danger")
        {
            InvokeRepeating("Danger", 0.5f, 1f);
            dealth.Play();

            RefreshHealthBar();
        }

        if (IsOnGround())
        {
            if (anim.GetBool("WalkWithWeapon") && !playingWalking)
                InvokeRepeating("RandomFootsteps", 0, 0.4f);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Danger")
        {
            CancelInvoke("Danger");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finishline")
        {
            GameoverWith("Dance");
        }
    }



    public void Danger()
    {
        health = Mathf.Clamp(health - 5, 0, maxHealth);
        // Check if dead
        if (health <= 0 && !isDead)
        {
            isDead = true;
            dealth.Play();
        }
        // Debug.Log("Health: " + health);
    }



    // Casting a Sphere from the position of FPS...
    // Sphere is of radius = FPS(Capsule Radius)
    // Sphere is casted downwards
    // hit will store all the information like objects that sphere hitted.
    // next is the distance from capsule position till which the sphere's center will go...
    // Physics.SphereCast returns true when it collides with some other collider otherwise it returns false...
    private bool IsOnGround()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, fpsCollider.radius, Vector3.down, out hit,
                                 fpsCollider.height / 2f - fpsCollider.radius + 0.05f);

    }


    // Jb mouse ko vertical move krenge to horizontal plane se 90 degree angle se jyada move ni kr paaenge camera.
    Quaternion ClampRotationXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -90, 90);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }



    private void ToggleCursor()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && cursorIsLocked)
        {
            LockCursor(false);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && !cursorIsLocked && !EventSystem.current.IsPointerOverGameObject())
        {
            LockCursor(true);
        }
    }
    public void LockCursor(bool action)
    {
        cursorIsLocked = action;
        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }



    // Play Random Walking Sound
    public void RandomFootsteps()
    {
        int idx = Random.Range(1, footSteps.Length);
        AudioSource temp = new AudioSource();

        temp = footSteps[idx];
        temp.Play();
        footSteps[idx] = footSteps[0];
        footSteps[0] = temp;
        playingWalking = true;
    }


    private void Shoot()
    {
        ammoClip--;
        RefreshBulletsInGun();

        anim.SetTrigger("Fire");
        GameStats.canShoot = false;


        RaycastHit hitInfo;
        if (Physics.Raycast(fpsCamera.gameObject.transform.position, fpsCamera.gameObject.transform.forward, out hitInfo, 200))
        {
            GameObject shotObj = hitInfo.collider.gameObject;
            if (shotObj.tag == "Zombie")
            {
                shotObj.GetComponent<ZombieController>().KillSelf();
                GameObject blood = Instantiate(bloodParticles, hitInfo.point, shotObj.transform.rotation);
                Destroy(blood, 0.5f);
            }
        }
    }


    public void TakeDamage(float damageAmount)
    {
        health = (int)Mathf.Clamp((float)health - damageAmount, 0, maxHealth);
        RefreshHealthBar();

        GameObject bloodSplatter = Instantiate(bloodSplaterPrefab, new Vector2(Random.Range(0, canvasWidth), Random.Range(0, canvasHeight)),
                    Quaternion.Euler(0, 0, Random.Range(0, 360)), canvas.transform);
        Destroy(bloodSplatter, 2.2f);

        if (health <= 0)
        {
            GameoverWith("Death");
        }
        // Debug.Log("Health: " + health);
    }


    private void GameoverWith(string action)
    {
        Vector3 pos = new Vector3(transform.position.x, Terrain.activeTerrain.SampleHeight(transform.position), transform.position.z);
        GameObject fullBody = Instantiate(fullBodyModel, pos, transform.rotation);
        fullBody.GetComponent<Animator>().SetTrigger(action);

        LockCursor(false);

        GameStats.gameOver = true;

        aim.SetActive(false);

        Destroy(gameObject);
    }


    private void RefreshHealthBar()
    {
        healthBar.value = health;
    }

    private void RefreshTotalBulletText()
    {
        totalBulletsText.text = ammo.ToString("00");
    }

    private void RefreshBulletsInGun()
    {
        bulletsInGunText.text = ammoClip.ToString("00");
    }


    private void SetCompass()
    {
        Vector3 toGoalVector = goal.transform.position - transform.position;
        float deg = Mathf.Clamp(Vector3.SignedAngle(transform.forward, toGoalVector, Vector3.up), -90, 90);
        compass.value = deg;
    }

}
