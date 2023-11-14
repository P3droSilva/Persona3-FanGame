using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float runSpeed = 40f;
    [SerializeField] private float groundDrag = 6f;
    public Transform LookAt;
    private Animator anim;
    private Rigidbody rb;

    float horizontalInput;
    float verticalInput;
    float speed;
    Vector3 moveDir = Vector3.zero;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        SpeedControl();

        bool isMoving = moveDir != Vector3.zero;
        anim.SetBool("Moving", isMoving);

        bool isRunning = speed == runSpeed;
        anim.SetBool("Sprinting", isRunning);

        rb.drag = groundDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        moveDir = LookAt.forward * verticalInput + LookAt.right * horizontalInput;
        rb.AddForce(moveDir.normalized * speed * 20f, ForceMode.Force);
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
            speed = runSpeed;
        else
            speed = moveSpeed;
        
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
