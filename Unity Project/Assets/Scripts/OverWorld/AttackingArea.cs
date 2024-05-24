using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingArea : MonoBehaviour
{
    public List<GameObject> EnemiesInRange;
    public bool mothmanInRange;

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

        if (other.gameObject.CompareTag("Mothman"))
        {
            mothmanInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Shadow") && EnemiesInRange.Contains(other.gameObject))
        {
            EnemiesInRange.Remove(other.gameObject);
        }

        if (other.gameObject.CompareTag("Mothman"))
        {
            mothmanInRange = false;
        }
    }
}
