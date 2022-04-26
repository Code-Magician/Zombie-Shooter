using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] AudioSource[] footSteps;
    [SerializeField] AudioSource jump;
    [SerializeField] AudioSource land;


    Rigidbody rb;
    CapsuleCollider fpsCollider;
    Quaternion camRotation, fpsRotation;
    bool cursorIsLocked = true;
    float x, z;



    private void Awake()
    {
        ChangeCursorState();
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fpsCollider = GetComponent<CapsuleCollider>();

        camRotation = fpsCamera.transform.localRotation;
        fpsRotation = this.transform.localRotation;
    }



    // Interval between 2 Fixed Update Functions is not constant...
    void Update()
    {
        // FPC Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround())
        {
            rb.AddForce(0, jumpForce, 0);
            jump.Play();
            if (anim.GetBool("WalkWithWeapon"))
                CancelInvoke("RandomFootsteps");
        }


        // Gets Weapon
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool("Weapon", !anim.GetBool("Weapon"));

        // Fire 
        if (Input.GetKeyDown(KeyCode.Mouse0))
            anim.SetTrigger("Fire");

        // Reload Gun
        if (Input.GetKeyDown(KeyCode.R))
            anim.SetTrigger("Reload");

        // Walking with Weapon
        if (x != 0 || z != 0)
        {
            if (!anim.GetBool("WalkWithWeapon"))
            {
                anim.SetBool("WalkWithWeapon", true);
                InvokeRepeating("RandomFootsteps", 0, 0.4f);
            }
        }
        else if (anim.GetBool("WalkWithWeapon"))
        {
            anim.SetBool("WalkWithWeapon", false);
            CancelInvoke("RandomFootsteps");
        }
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
        if (IsOnGround())
        {
            land.Play();
            if (anim.GetBool("WalkWithWeapon"))
                InvokeRepeating("RandomFootsteps", 0, 0.4f);
        }
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
            cursorIsLocked = false;
            ChangeCursorState();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && !cursorIsLocked)
        {
            cursorIsLocked = true;
            ChangeCursorState();
        }
    }
    public void ChangeCursorState()
    {
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
    }
}
