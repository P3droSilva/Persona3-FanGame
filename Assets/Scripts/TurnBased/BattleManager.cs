using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Camera Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera targetCamera;

    [Header("Battle UI")]
    [SerializeField] private Canvas battleCanvas;
    [SerializeField] private Button attackButton;
    [SerializeField] private GameObject targetArrowPrefab;
    private GameObject targetArrow;

    [Header("Enemy Properties")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemySpawnPoint1;
    [SerializeField] private GameObject enemySpawnPoint2;
    [SerializeField] private GameObject enemySpawnPoint3;
    [SerializeField] private GameObject enemySpawnPoint4;
    [SerializeField] private Transform  enemyParent;
    private int enemiesCount;

    [Header("Party Properties")]
    [SerializeField] private MakotoBattle makoto;
    [SerializeField] private MitsuruBattle mitsuru;
    [SerializeField] private JunpeiBattle junpei;
    [SerializeField] private YukariBattle yukari;
    private int partyCount = 4;

    // list that controls the turn order
    private List<PlayerBattle> characters = new List<PlayerBattle>();
    private List<ShadowBattle> enemies = new List<ShadowBattle>();

    private PlayerBattle activeCharacter;
    private int activeCharacterIndex;
    private int targetIdx = -1;
    private bool choosingTarget = false;

    private void Start()
    {
        battleCanvas.gameObject.SetActive(false);
        targetCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        // add party members to the list
        characters.Add(makoto);
        characters.Add(mitsuru);
        characters.Add(junpei);
        characters.Add(yukari);

        //get encounter enemies properties
        enemiesCount = UnityEngine.Random.Range(2, 4); // This will change depending on the future encounter table

        //Instantiate Enemies
        List<GameObject> enemySpawnPoints = new List<GameObject>
        {
            enemySpawnPoint2,
            enemySpawnPoint3,
            enemySpawnPoint1,
            enemySpawnPoint4
        };

        // instantiate the enemies as children of the enemySpawnPoints
        for (int i = 0; i < enemiesCount; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, enemySpawnPoints[i].transform);
            enemy.transform.localPosition = Vector3.zero;
            ShadowBattle shadowbt = enemy.GetComponent<ShadowBattle>();
            characters.Add(shadowbt); // add enemies to the list
            enemies.Add(shadowbt);
        }

        DecideTurnOrder();
        StartCoroutine(BattleLoop());
    }

    private void Update()
    {
        if (choosingTarget)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                targetIdx++;
                if (targetIdx >= enemies.Count)
                    targetIdx = 0;

                Destroy(targetArrow);
                targetArrow = Instantiate(targetArrowPrefab, enemies[targetIdx].gameObject.transform);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                targetIdx--;
                if (targetIdx < 0)
                    targetIdx = enemies.Count - 1;

                Destroy(targetArrow);
                targetArrow = Instantiate(targetArrowPrefab, enemies[targetIdx].gameObject.transform);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                choosingTarget = false;
                Destroy(targetArrow);
                SwapCamera();
                Debug.Log(activeCharacter.name + " attacked " + enemies[targetIdx].name);
            }
        }
    }

    IEnumerator BattleLoop()
    {
        activeCharacterIndex = 0;

        while(true)
        {
            activeCharacter = characters[activeCharacterIndex];

            if(activeCharacter.isPlayer)
            {
                Debug.Log("Active Char Index: " + activeCharacterIndex);
                Debug.Log(activeCharacter.name + " is a player");

                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(activeCharacter.PhysicalAttack);
                yield return StartCoroutine(PlayerAction());
            }
            else
            {
                Debug.Log("Active Char Index: " + activeCharacterIndex);
                Debug.Log(activeCharacter.name + " is an enemy");

                // enemy turn
                yield return new WaitForSeconds(1f);
            }
            
            if(enemiesCount == 0)
            {
                Debug.Log("You win!");
                break;
            }
            else if(partyCount == 0)
            {
                Debug.Log("You lose!");
                break;
            }
            
            activeCharacterIndex++;
            if(activeCharacterIndex >= characters.Count)
                activeCharacterIndex = 0;
            
        }
    }

    IEnumerator PlayerAction()
    {
        battleCanvas.gameObject.SetActive(true);

        yield return StartCoroutine(activeCharacter.StartAction());
        int rawDamage = activeCharacter.getDamage();

        battleCanvas.gameObject.SetActive(false);

        ChooseTarget();

        while (choosingTarget == true)
        {
            yield return null;
        }

        int hp = enemies[targetIdx].TakeDamage(rawDamage);
        if(hp <= 0)
        {
            enemies[targetIdx].gameObject.SetActive(false);

            Debug.Log("Character List Size: " + characters.Count);
            characters.Remove(enemies[targetIdx]);
            enemies.RemoveAt(targetIdx);
            enemiesCount--;
            Debug.Log("Character List Size: " + characters.Count);
        }

        targetIdx = -1;
        battleCanvas.gameObject.SetActive(false);
    }

    private void ChooseTarget()
    {
        targetIdx = 0;
        choosingTarget = true;
        SwapCamera();
        targetArrow = Instantiate(targetArrowPrefab, enemies[targetIdx].gameObject.transform);
    }

    private void SwapCamera()
    {
        if(mainCamera.gameObject.activeSelf == true)
        {
            mainCamera.gameObject.SetActive(false);
            targetCamera.gameObject.SetActive(true);
        }
        else
        {
            mainCamera.gameObject.SetActive(true);
            targetCamera.gameObject.SetActive(false);
        }

        Debug.Log("Camera Swapped");

    }

    private void DecideTurnOrder()
    {
        foreach(var chars in characters)
        {
            Debug.Log(chars.name + " has " + chars.initiative + " initiative and " + chars.agility + " agility"); 
        }

        characters.Sort((x, y) => y.initiative.CompareTo(x.initiative));
    }
}

