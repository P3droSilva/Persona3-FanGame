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
    float maxSpeed;
    Vector3 moveDir = Vector3.zero;

    public Vector3 velocity = Vector3.zero;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        maxSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        SpeedControl();

        bool isMoving = moveDir != Vector3.zero;
        anim.SetBool("Moving", isMoving);

        bool isRunning = maxSpeed == runSpeed && isMoving;
        anim.SetBool("Sprinting", isRunning);
        
        rb.drag = groundDrag;
        velocity = rb.velocity;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer() // moves the player
    {
        moveDir = LookAt.forward * verticalInput + LookAt.right * horizontalInput;
        rb.AddForce(moveDir.normalized * maxSpeed * 20f, ForceMode.Force);
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
            maxSpeed = runSpeed;
        else
            maxSpeed = moveSpeed;        
                
    }

    void SpeedControl() // limits the speed of the player
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
