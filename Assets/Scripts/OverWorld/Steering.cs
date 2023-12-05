using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static Autodesk.Fbx.FbxEuler;
using static UnityEditor.PlayerSettings;

public class Steering : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderDistance = 2f;
    [SerializeField] private float wanderJitter = 3f;
    private float wanderCircleAngle = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    
    public Vector3 Seek(Actor actor, Vector3 targetPos)
    {
        Vector3 desiredVelocity = targetPos - actor.transform.position;
        desiredVelocity.Normalize();
        desiredVelocity *= actor.maxSpeed;
        return (desiredVelocity - actor.velocity);
    }

    public Vector3 Pursuit(Actor pursuer, PlayerMovement evader)
    {
        //if the evader is ahead and facing the agent then we can just seek
        //for the evader's current position.
        Vector3 toEvader = evader.transform.position - pursuer.transform.position;
        float relativeDir = Vector3.Dot(pursuer.transform.forward.normalized, evader.transform.forward.normalized);

        if ((Vector3.Dot(toEvader.normalized, pursuer.transform.forward.normalized) > 0) && (relativeDir < -0.95)) //acos(0.95)=18 degs
        {
            return Seek(pursuer, evader.transform.position);
        }

        float lookAheadTime = toEvader.magnitude / (pursuer.maxSpeed + evader.velocity.magnitude);

        //now seek to the predicted future position of the evader
        return Seek(pursuer, evader.transform.position + evader.velocity * lookAheadTime);
    }

    public Vector3 Wander(Actor actor)
    {
        Vector3 circlePos = actor.transform.position + actor.transform.forward.normalized * wanderDistance;

        float angleChange = Random.Range(-wanderJitter, wanderJitter);
        float wanderAngle = Mathf.Deg2Rad * angleChange;

        // Adjust the wander angle each frame
        wanderCircleAngle += wanderAngle;

        Vector3 target = circlePos + new Vector3(Mathf.Cos(wanderCircleAngle), 0f, Mathf.Sin(wanderCircleAngle)) * wanderRadius;

        return Seek(actor, target);
    }


}
