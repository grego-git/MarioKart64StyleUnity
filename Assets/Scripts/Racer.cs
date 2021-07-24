using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racer : MonoBehaviour
{
    private Sprite[][] sprites = new Sprite[3][];
    private SpriteRenderer sr;

    private Rigidbody rb;

    [SerializeField]
    private float topSpeed;
    [SerializeField]
    private float accel;
    [SerializeField]
    private float turnSpeed;
    Vector3 dir;

    // Sprite Index vars
    [Header("Sprite")]
    [SerializeField]
    private float spriteIndexDur;
    private float spriteIndexTimer;
    private int spriteIndex;

    // Camera vars
    [Header("Camera")]
    [SerializeField]
    private Transform cam;
    private Vector3 camForward;
    private float initCamRot;

    [Header("Rumble")]
    // Rumble vars
    [SerializeField]
    private float rumbleSpeed;
    [SerializeField]
    private float rumbleHeight;
    private float rumbleTimer;

    // Start is called before the first frame update
    void Start()
    {
        // Load sprites
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Resources.LoadAll<Sprite>(string.Format("Sprites/{0}/GregioKartSpriteSheet", i + 1));
            Debug.Log(sprites[i].Length);
        }

        // Get components
        sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();

        // Get camera X Rotation
        initCamRot = cam.eulerAngles.x;
    }

    private void LateUpdate()
    {
        float angle = Vector3.SignedAngle(camForward, transform.forward, Vector3.up) / 180.0f * 31.0f;

        if (angle > 0.0f)
        {
            sr.flipX = false;
        }
        else if (angle < 0.0f)
        {
            sr.flipX = true;
        }

        sr.sprite = sprites[spriteIndex][(int)Mathf.Floor(Mathf.Abs(angle))];
        sr.transform.LookAt(transform.position + camForward);

        rumbleTimer += rumbleSpeed * rb.velocity.magnitude / topSpeed * Time.deltaTime;

        sr.transform.localPosition = new Vector3(0.0f, Mathf.Sin(rumbleTimer) * rumbleHeight * rb.velocity.magnitude / topSpeed, 0.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        spriteIndexTimer += rb.velocity.magnitude / topSpeed;

        if (spriteIndexTimer >= spriteIndexDur)
        {
            spriteIndex++;
            spriteIndexTimer = 0.0f;
        }

        if (spriteIndex == sprites.Length)
        {
            spriteIndex = 0;
        }

        // Input
        if (Input.GetAxisRaw("Horizontal") < 0.0f)
        {
            dir = -new Vector3(cam.right.x, 0.0f, cam.right.z);
        }
        else if (Input.GetAxisRaw("Horizontal") > 0.0f)
        {
            dir = new Vector3(cam.right.x, 0.0f, cam.right.z);
        }
        else
        {
            dir = new Vector3(transform.forward.x, 0.0f, transform.forward.z);
        }

        AdjustCamera();

        camForward = Vector3.Cross(cam.right, transform.up).normalized;

        transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).normalized);

        if (Input.GetButton("Jump"))
        {
            if (rb.velocity.magnitude <= topSpeed)
            {
                rb.velocity += camForward * accel * Time.deltaTime;
            }
        }
        else 
        {
            transform.LookAt(transform.position + Vector3.Slerp(camForward, transform.forward, rb.velocity.magnitude / topSpeed));
        }        
    }

    void AdjustCamera()
    {
        camForward = Vector3.Cross(cam.right, transform.up).normalized;

        float angle = Mathf.Clamp(Vector3.SignedAngle(camForward, dir, Vector3.up), -75.0f, 75.0f);

        cam.position = transform.position;
        cam.rotation = Quaternion.Euler(0.0f, cam.eulerAngles.y, cam.eulerAngles.z);

        cam.Rotate(new Vector3(0.0f, angle * turnSpeed * rb.velocity.magnitude / topSpeed * Time.deltaTime, 0.0f));

        cam.rotation = Quaternion.Euler(initCamRot, cam.eulerAngles.y, cam.eulerAngles.z);
        cam.position -= (cam.transform.forward * 1.25f);
    }
}
