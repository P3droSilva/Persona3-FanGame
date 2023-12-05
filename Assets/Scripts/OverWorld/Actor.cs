using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Actor : MonoBehaviour
{
    public float wanderSpeed = 30f;
    public float pursuitSpeed = 50f;
    public float maxSpeed;
    public Vector3 velocity = Vector3.zero;
    public Vector3 steeringForce;

    private Steering steering;
    private Rigidbody rb;
    private Animator anim;

    public PlayerMovement target;
    private bool pursuit = false;

    void Start()
    {
        steering = GetComponent<Steering>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        maxSpeed = wanderSpeed;
    }

    void ApplyForce()
    {
        rb.AddForce(steeringForce, ForceMode.Force);
        SpeedControl();
    }

    void Update()
    {
        float targetDistance = Vector3.Distance(transform.position, target.transform.position);
        if(targetDistance < 30f)
        {
            pursuit = true;
            maxSpeed = pursuitSpeed;
        }
        else
        {
            pursuit = false;
            maxSpeed = wanderSpeed;
        }

        Vector3 pursuitForce = steering.Pursuit(this, target);
        Vector3 wanderForce = steering.Wander(this);

        if (pursuit)
            steeringForce = pursuitForce;
        else
            steeringForce = wanderForce;

        ApplyForce();

        if(targetDistance < 3f)
        {
            StartCoroutine(ShadowAttack());
        }
    }

    private IEnumerator ShadowAttack()
    {
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Shadow Attack");
        SceneManager.LoadScene("TurnBasedBattle");
    }

    private void SpeedControl() // limits the speed of the player
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, 0f, limitedVel.z);
        }
        else
        {
            rb.velocity = flatVel;
        }

        velocity = rb.velocity;
        transform.LookAt(transform.position + velocity);
    }
}
