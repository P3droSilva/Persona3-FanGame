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
    private Vector3 startPos = Vector3.zero;

    private bool firstUpdate = true;
    private bool movementDisabled = false;

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerPosition != Vector3.zero)
        {
            startPos = GameManager.Instance.playerPosition;
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        maxSpeed = moveSpeed;
    }

    void Update()
    {
        if(firstUpdate)
        {
            firstUpdate = false;
            // por algum motivo, se fizesse isso no Start, as vezes o player não carregava na posição certa
            if (startPos != Vector3.zero)
            {
                transform.position = startPos;
                Debug.Log("Player position loaded " + startPos);
            }
        }

        if (movementDisabled)
        {
            anim.SetBool("Moving", false);
            anim.SetBool("Sprinting", false);
            return;
        }

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
        if(movementDisabled)
        {
            rb.velocity = Vector3.zero;
            anim.SetBool("Moving", false);
            anim.SetBool("Sprinting", false);
            return;
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Stairs"))
        {
            GameManager.Instance.LoadSecondFloor();
            rb.velocity = Vector3.zero;
            StartCoroutine(DisableMovement(1f));
        }
        else if(other.CompareTag("StairsDown"))
        {
            GameManager.Instance.LoadFirstFloor();
            rb.velocity = Vector3.zero;
            StartCoroutine(DisableMovement(1f));
        }
        
        if(other.CompareTag("Heal"))
        {
            GameManager.Instance.Heal();
        }
    }

    private IEnumerator DisableMovement(float seconds)
    {
        movementDisabled = true;
        yield return new WaitForSeconds(seconds);
        movementDisabled = false;
    }
}
