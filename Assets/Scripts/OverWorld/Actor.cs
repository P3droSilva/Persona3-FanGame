using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Steering;

public class Actor : MonoBehaviour
{
    [Header("Movement and Attack")]
    public Vector3 velocity = Vector3.zero;
    public float wanderSpeed = 3f;
    public float pursuitSpeed = 5f;
    public float maxSpeed;
    public float rotationSpeed = 10f;
    public float attackRange = 2f;

    [Header("Steering Arguments")]
    public float avoidanceRadius = 7f;
    public float maxAvoidanceDistance = 2.5f;
    public float viewRadius = 15f;
    public float viewAngle = 120f;
    public PlayerMovement target;
    private Vector3 lastTargetPosition = Vector3.zero;

    [Header("Steering Control")]
    public Vector3 resultDirection;
    public bool targetInView = false;
    public bool wander = true;
    public bool attacking = false;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public bool showDanger = true;
    public bool showInterest = true;
    public bool showViewAngle = true;
    private float[] interestTemp;
    private float[] dangerTemp;

    // Components
    private Steering steering;
    private Rigidbody rb;
    private Animator anim;

    void Start()
    {
        steering = GetComponent<Steering>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        maxSpeed = wanderSpeed;
    }

    void FindTargetInView()
    {
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");
        LayerMask playerLayer = LayerMask.GetMask("Player");

        Collider playerCollider = target.GetComponent<Collider>();

        Vector3 direction = (playerCollider.transform.position - transform.position).normalized;

        if (showGizmos && showViewAngle)
        {
            Debug.DrawRay(transform.position, direction * viewRadius, Color.blue);

            // Get the starting and ending points of the view angle
            Vector3 viewAngleStart = Quaternion.AngleAxis(-viewAngle * 0.5f, transform.up) * transform.forward;
            Vector3 viewAngleEnd = Quaternion.AngleAxis(viewAngle * 0.5f, transform.up) * transform.forward;

            // Draw rays for visualization of the view angle
            Debug.DrawRay(transform.position, viewAngleStart * viewRadius, Color.magenta);
            Debug.DrawRay(transform.position, viewAngleEnd * viewRadius, Color.magenta);
        }

        // exclui alvos fora do angulo de visao
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > viewAngle * 0.5f && wander)
        {
            return;
        }
            
        // exclui alvos atras de obstaculos
        //RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, viewRadius, obstacleLayer))
        {
            Debug.Log("Target behind obstacle");
            targetInView = false;
        }
        else if (Physics.Raycast(transform.position, direction, viewRadius, playerLayer))
        {
            targetInView = true;
            wander = false;
            lastTargetPosition = playerCollider.transform.position;
        }
    }

    private Vector3 FindMoveDirection(float[] danger, float[] interest)
    {
        for (int i = 0; i < interest.Length; i++)
        {
            // se o valor for maior que 1, seta pra 1
            danger[i] = Mathf.Clamp01(danger[i]);
            interest[i] = Mathf.Clamp01(interest[i]);

            // diminui o valor de interesse baseado no perigo
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }

        // gizmos
        interestTemp = interest;
        dangerTemp = danger;
            
        // se todas as direcoes de interesse contem um obstaculo, rotaciona 180 graus
        bool zeroInterest = interest.All(x => x < 0.05f);
        if(zeroInterest)
        {
            transform.Rotate(0f, 45f, 0f);
            wander = true;
            return Vector3.zero;
        }
        
        Vector3 outputDirection = Vector3.zero;
        for (int i = 0; i < interest.Length; i++)
        {
            outputDirection += interest[i] * Steering.Directions.eightDirections[i];
        }
        outputDirection.y = 0f;
        outputDirection.Normalize();

        return outputDirection;
    }

    void FixedUpdate()
    {
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");
        
        FindTargetInView();

        float[] danger = steering.WallAvoidance(this, avoidanceRadius, maxAvoidanceDistance, obstacleLayer);
        float[] interest;


        if(attacking)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        else if (targetInView) // se esta vendo, persegue
        {
            interest = steering.Pursuit(this, target);
            maxSpeed = pursuitSpeed;

            if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                StartCoroutine(ShadowAttack());
                attacking = true;
            }
        }
        else if (wander)  // nao viu, wander
        {
            interest = steering.Wander(this);
            maxSpeed = wanderSpeed;
        }
        else             // ja viu, mas perdeu de vista
        {
            interest = steering.Seek(this, lastTargetPosition);
            maxSpeed = pursuitSpeed;
        }

        // se chegou na ultima posicao e nao ve, wander
        if (Vector3.Distance(transform.position, lastTargetPosition) < 2f && !targetInView)
            wander = true;

        resultDirection = FindMoveDirection(danger, interest);

        rb.velocity = resultDirection * maxSpeed;
        velocity = rb.velocity;

        if(resultDirection != Vector3.zero)
            SmoothlyAlignToDirection(resultDirection);
    }
    
    void SmoothlyAlignToDirection(Vector3 targetDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    private IEnumerator ShadowAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Shadow Attack");
        SceneManager.LoadScene("TurnBasedBattle");
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying && showGizmos)
        {
            if (interestTemp != null && showInterest)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < interestTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestTemp[i] * 5f);
                }
            }

            if (dangerTemp != null && showDanger)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < dangerTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * dangerTemp[i] * 5f);
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, resultDirection * 5f);
        }
    }
}
