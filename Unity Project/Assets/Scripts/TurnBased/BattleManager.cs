using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class BattleManager : MonoBehaviour
{
    [Header("Camera Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CinemachineVirtualCamera vcam;

    [Header("Battle UI")]
    [SerializeField] private Canvas battleCanvas;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button guardButton;

    [Header("Enemy Properties")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemySpawnPoint1;
    [SerializeField] private GameObject enemySpawnPoint2;
    [SerializeField] private GameObject enemySpawnPoint3;
    [SerializeField] private GameObject enemySpawnPoint4;
    private int enemiesCount;

    [Header("Party Properties")]
    [SerializeField] private MakotoBattle makoto;
    [SerializeField] private MitsuruBattle mitsuru;
    [SerializeField] private JunpeiBattle junpei;
    [SerializeField] private YukariBattle yukari;
    private int partyCount = 4;

    [Header("Battle Properties")]
    [SerializeField] private int playerAdvantage = -1;
    [SerializeField] private int advantageTurns = 0; 
    [SerializeField] private List<PlayerBattle> characters = new List<PlayerBattle>(); // list that controls the turn order
    private List<PlayerBattle> party = new List<PlayerBattle>();    
    private List<ShadowBattle> enemies = new List<ShadowBattle>();

    private PlayerBattle activeCharacter;
    private int activeCharacterIndex;
    private int targetIdx = -1;
    private bool choosingTarget = false;

    private void Start()
    {
        //free mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        battleCanvas.gameObject.SetActive(false);
        targetCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        // add party members to the Characters list
        characters.Add(makoto);
        characters.Add(junpei);
        characters.Add(mitsuru);
        characters.Add(yukari);

        // add party members to the Party list
        party.Add(makoto);
        party.Add(junpei);
        party.Add(mitsuru);
        party.Add(yukari);

        //get encounter enemies properties
        enemiesCount = UnityEngine.Random.Range(2, 5); // This will change depending on the future encounter table
   
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

        playerAdvantage = GameManager.Instance.playerAdvantage;

        DecideTurnOrder();
        StartCoroutine(BattleLoop());
    }

    private void Update()
    {
        if (choosingTarget)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                enemies[targetIdx].ToggleActiveCanvas();
                targetIdx++;
                if (targetIdx >= enemies.Count)
                    targetIdx = 0;

                enemies[targetIdx].ToggleActiveCanvas();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                enemies[targetIdx].ToggleActiveCanvas();
                targetIdx--;
                if (targetIdx < 0)
                    targetIdx = enemies.Count - 1;

                enemies[targetIdx].ToggleActiveCanvas();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                choosingTarget = false;
                SwapCamera();
            }
        }
    }

    IEnumerator BattleLoop()
    {
        activeCharacterIndex = 0;
        SwapCamera();

        advantageTurns = playerAdvantage == 1 ? enemiesCount : partyCount;

        while(true)
        {
            activeCharacter = characters[activeCharacterIndex];

            if(activeCharacter.isPlayer)
            {
                if (advantageTurns == 0 || playerAdvantage == 1)
                {
                    ChangeCameraFocus();

                    attackButton.onClick.RemoveAllListeners();
                    attackButton.onClick.AddListener(activeCharacter.PhysicalAttack);
                    guardButton.onClick.RemoveAllListeners();
                    guardButton.onClick.AddListener(activeCharacter.Guard);

                    yield return StartCoroutine(PlayerAction());
                }

                if(advantageTurns > 0 && playerAdvantage == 0)
                {
                    advantageTurns--;
                }
            }
            else
            {
                if (advantageTurns == 0 || playerAdvantage == 0)
                {
                    yield return StartCoroutine(EnemyAction());
                }

                if (advantageTurns > 0 && playerAdvantage == 1)
                {
                    advantageTurns--;
                }
            }
            
            if(enemiesCount == 0)
            {
                GameManager.Instance.LoadOverworldScene();
                foreach(PlayerBattle player in party)
                {
                    player.SaveStats();
                }
                break;
            }
            else if(partyCount == 0)
            {
                GameManager.Instance.LoadGameOver(false);
                foreach (PlayerBattle player in party)
                {
                    player.SaveStats();
                }
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

        battleCanvas.gameObject.SetActive(false);
        activeCharacter.ToggleActiveCanvas();

        if(activeCharacter.isGuarding)
        {
            yield return StartCoroutine(GuardAnimation());
        }
        else
        {
            int rawDamage = activeCharacter.getDamage();
            ChooseTarget();

            while (choosingTarget == true)
            {
                yield return null;
            }

            yield return StartCoroutine(AttackAnimation(activeCharacter.gameObject, enemies[targetIdx].gameObject));

            int hp = enemies[targetIdx].TakeDamage(rawDamage);

            yield return new WaitForSeconds(1f);
            enemies[targetIdx].ToggleActiveCanvas();

            if (hp <= 0)
            {
                enemies[targetIdx].gameObject.SetActive(false);
                characters.Remove(enemies[targetIdx]);
                enemies.RemoveAt(targetIdx);
                enemiesCount--;

                if (advantageTurns > 0 && playerAdvantage == 1)
                {
                    advantageTurns--;
                }
            }
            targetIdx = -1;
        }
        
        activeCharacter.ToggleActiveCanvas();
    }

    IEnumerator EnemyAction() 
    {
        yield return StartCoroutine(activeCharacter.StartAction());
        int rawDamage = activeCharacter.getDamage();

        int target = UnityEngine.Random.Range(0, party.Count);

        yield return StartCoroutine(AttackAnimation(activeCharacter.gameObject, party[target].gameObject));

        int hp = party[target].TakeDamage(rawDamage);
        yield return new WaitForSeconds(1f);

        if (hp <= 0)
        {
            party[target].gameObject.SetActive(false);
            characters.Remove(party[target]);
            party.RemoveAt(target);
            partyCount--;

            if (advantageTurns > 0 && playerAdvantage == 0)
            {
                advantageTurns--;
            }
        }

        battleCanvas.gameObject.SetActive(false);
    }
    

    private void ChooseTarget()
    {
        targetIdx = 0;
        choosingTarget = true;
        SwapCamera();
        enemies[targetIdx].ToggleActiveCanvas();
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
    }

    IEnumerator AttackAnimation(GameObject attacker, GameObject target)
    {
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();

        vcam.Follow = attacker.transform;
        vcam.LookAt = attacker.transform.parent.Find("LookAt").transform;
        transposer.m_FollowOffset = new Vector3(0, 2, 4.5f);

        activeCharacter.gameObject.GetComponent<Animator>().SetTrigger("Attack");
        yield return new WaitForSeconds(1f);

        vcam.Follow = target.transform;
        vcam.LookAt = target.transform.parent.Find("LookAt").transform;
        transposer.m_FollowOffset = new Vector3(0, 2, 4.5f);

        PlayerBattle targetScript = target.GetComponent<PlayerBattle>();
        if(party.Contains(targetScript))
        {
            target.GetComponent<Animator>().SetTrigger("Hit");
        }
    }

    IEnumerator GuardAnimation()
    {
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();

        vcam.Follow = activeCharacter.gameObject.transform;
        vcam.LookAt = activeCharacter.gameObject.transform.parent.Find("LookAt").transform;
        transposer.m_FollowOffset = new Vector3(0, 3, 4.5f);

        activeCharacter.gameObject.GetComponent<Animator>().SetTrigger("Guard");
        yield return new WaitForSeconds(1f);
    }

    private void ChangeCameraFocus()
    {
        GameObject partyMember = activeCharacter.gameObject;
        vcam.Follow = partyMember.transform;
        vcam.LookAt = partyMember.transform.parent.Find("LookAt").transform;
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();

        switch(partyMember.name)
        {
            case "Player(TurnBased)":
                transposer.m_FollowOffset = new Vector3(2.5f, 3, -4);
                break;
            case "Junpei(TurnBased)":
                transposer.m_FollowOffset = new Vector3(1f, 3, -4);
                break;
            case "Mitsuru(TurnBased)":
                transposer.m_FollowOffset = new Vector3(-0.5f, 3, -4);
                break;
            case "Yukari(TurnBased)":
                transposer.m_FollowOffset = new Vector3(-2f, 3, -4);
                break;
        }
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

