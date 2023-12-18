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
    public float attackRange = 4f;

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
    public bool pursuing = false;
    public bool searching = false;
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

    void FixedUpdate()
    {
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");

        FindTargetInView();

        float[] danger = steering.WallAvoidance(this, avoidanceRadius, maxAvoidanceDistance, obstacleLayer);
        float[] interest = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };


        if (pursuing)
        {
            interest = steering.Pursuit(this, target);
            maxSpeed = pursuitSpeed;

            if (!attacking && Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                StartCoroutine(ShadowAttack());
                attacking = true;
            }
        }
        else if (wander)
        {
            interest = steering.Wander(this);
            maxSpeed = wanderSpeed;
        }
        else if (searching)
        {
            interest = steering.Seek(this, lastTargetPosition);
            maxSpeed = pursuitSpeed;

            if (Vector3.Distance(transform.position, lastTargetPosition) < 1f && !targetInView)
            {
                wander = true;
                searching = false;
            }
        }

        resultDirection = FindMoveDirection(danger, interest);

        rb.velocity = resultDirection * maxSpeed;
        velocity = rb.velocity;

        if (resultDirection != Vector3.zero)
            SmoothlyAlignToDirection(resultDirection);
    }

    void FindTargetInView()
    {
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");
        LayerMask playerLayer = LayerMask.GetMask("Player");

        Vector3 playerPos = target.transform.position;
        playerPos.y = 1f;

        Vector3 actorPos = transform.position;
        actorPos.y = 1f;
       

        Vector3 direction = (playerPos - actorPos).normalized;

        ViewAngleGizmos(direction); 

        // exclui alvos atras de obstaculos
        RaycastHit hit;
        if (Physics.Raycast(actorPos, direction, out hit, viewRadius, obstacleLayer) && hit.collider.gameObject.layer != playerLayer)
        {
            Vector3 hitPoint = hit.point;
            if(Vector3.Distance(hitPoint, actorPos) > Vector3.Distance(playerPos, actorPos))
            {
                TargetOnView(direction);
                return;
            }

            TargetNotOnView();
        }
        else if (Physics.Raycast(actorPos, direction, viewRadius, playerLayer))
        {
            TargetOnView(direction);
        }
    }

    private void TargetOnView(Vector3 direction)
    {
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > viewAngle * 0.5f && wander)
        {
            return;
        }

        if (searching || wander)
        {
            searching = false;
            wander = false;
            pursuing = true;
        }

        targetInView = true;
        lastTargetPosition = target.transform.position;
    }

    private void TargetNotOnView()
    {
        targetInView = false;

        if (pursuing)
        {
            pursuing = false;
            searching = true;
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
        if (zeroInterest)
        {
            transform.Rotate(0f, 90f, 0f);
            pursuing = false;
            searching = false;
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

    void SmoothlyAlignToDirection(Vector3 targetDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    private IEnumerator ShadowAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        if(Vector3.Distance(transform.position, target.transform.position) < attackRange)
        {
            GameManager.Instance.LoadBattleScene(false, gameObject);
        }
        else
        {
            attacking = false;
            searching = true;
        }
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

    private void ViewAngleGizmos(Vector3 direction)
    {
        Vector3 actorPos = transform.position;
        if (showGizmos && showViewAngle)
        {
            Debug.DrawRay(actorPos, direction * viewRadius, Color.blue);

            // Get the starting and ending points of the view angle
            Vector3 viewAngleStart = Quaternion.AngleAxis(-viewAngle * 0.5f, transform.up) * transform.forward;
            Vector3 viewAngleEnd = Quaternion.AngleAxis(viewAngle * 0.5f, transform.up) * transform.forward;

            // Draw rays for visualization of the view angle
            Debug.DrawRay(actorPos, viewAngleStart * viewRadius, Color.magenta);
            Debug.DrawRay(actorPos, viewAngleEnd * viewRadius, Color.magenta);
        }
    }
}
