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

    public float[] Seek(Actor actor, Vector3 targetPos)
    {
        float[] interest = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        Vector3 directionToTarget = targetPos - actor.transform.position;
        directionToTarget.Normalize();

        for(int i=0; i < Directions.eightDirections.Count; i++)
        {
            float result = Vector3.Dot(directionToTarget, Directions.eightDirections[i]);

            if(result > 0)
            {
                if(result > interest[i])
                {
                    interest[i] = result;
                }
            }
        }
        return interest;
    }

    public float[] Pursuit(Actor pursuer, PlayerMovement evader)
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

    public float[] Wander(Actor actor)
    {
        // Calculate the displacement vector
        Vector3 wanderTarget = actor.resultDirection + new Vector3(Random.Range(-1f, 1f) * wanderJitter, 0, Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        // Adjust the target position based on wander distance
        Vector3 targetPos = actor.transform.position + actor.transform.forward * wanderDistance + wanderTarget;

        // Seek the calculated target position
        return Seek(actor, targetPos);
    }

    public float[] WallAvoidance (Actor actor, float avoidanceRadius, float actorColliderSize, LayerMask obstacleLayer)
    {
        float[] danger = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
        Collider[] colliders = Physics.OverlapSphere(actor.transform.position, avoidanceRadius, obstacleLayer);

        foreach (Collider collider in colliders)
        {
            // Get the closest point on the obstacle collider
            Vector3 directionToObstable = collider.ClosestPoint(actor.transform.position) - actor.transform.position;
            float distanceToObstacle = directionToObstable.magnitude;

            float weight = distanceToObstacle <= actorColliderSize ? 1f : (avoidanceRadius - distanceToObstacle)/avoidanceRadius;

            Vector3 directionToObstacleNormalized = directionToObstable.normalized;

            for(int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector3.Dot(directionToObstacleNormalized, Directions.eightDirections[i]);

                float dangerVal = result * weight;

                if(dangerVal > danger[i])
                {
                    danger[i] = dangerVal;
                }
            }
        }
        return danger;
    }

    public static class Directions
    {
        public static List<Vector3> eightDirections = new List<Vector3>()
        {
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(-1, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, -1)
        };
    }


}
