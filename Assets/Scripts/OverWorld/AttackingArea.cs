using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingArea : MonoBehaviour
{
    public List<GameObject> EnemiesInRange;

    void Start()
    {
        EnemiesInRange = new List<GameObject>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Shadow") && !EnemiesInRange.Contains(other.gameObject))
        {
            EnemiesInRange.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Shadow") && EnemiesInRange.Contains(other.gameObject))
        {
            EnemiesInRange.Remove(other.gameObject);
        }
    }
}
