using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemySpawnPoint1;
    [SerializeField] private GameObject enemySpawnPoint2;
    [SerializeField] private GameObject enemySpawnPoint3;
    [SerializeField] private GameObject enemySpawnPoint4;
    private int partySize = 4;
    private int enemiesSize;

    void Start()
    {
        //get encounter enemies properties
        var rand = new System.Random();
        enemiesSize = rand.Next(1, 5); // This will change depending on the future encounter table

        //Instantiate Enemies
        List<GameObject> enemySpawnPoints = new List<GameObject>();
        enemySpawnPoints.Add(enemySpawnPoint2);
        enemySpawnPoints.Add(enemySpawnPoint3);
        enemySpawnPoints.Add(enemySpawnPoint1);
        enemySpawnPoints.Add(enemySpawnPoint4);

        // instantiate the enemies as children of the enemySpawnPoints
        for(int i = 0; i < enemiesSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.transform.SetParent(enemySpawnPoints[i].transform);
            enemy.transform.localPosition = Vector3.zero;
        }
        
    }
}

