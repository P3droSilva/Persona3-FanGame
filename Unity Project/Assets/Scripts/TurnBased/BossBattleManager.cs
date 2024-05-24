using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class BossBattleManager : MonoBehaviour
{
    [Header("Camera Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Camera multiTargetCamera;
    [SerializeField] private CinemachineVirtualCamera vcam;

    [Header("Battle UI")]
    [SerializeField] private Canvas battleCanvas;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button guardButton;

    [Header("Enemy Properties")]
    [SerializeField] private MothmanBattle mothman;
    private int enemiesCount;

    [Header("Party Properties")]
    [SerializeField] private MakotoBattle makoto;
    [SerializeField] private MitsuruBattle mitsuru;
    [SerializeField] private JunpeiBattle junpei;
    [SerializeField] private YukariBattle yukari;
    private int partyCount = 4;

    [Header("Battle Properties")]
    [SerializeField] private List<PlayerBattle> characters = new List<PlayerBattle>(); // list that controls the turn order
    private List<PlayerBattle> party = new List<PlayerBattle>();    

    private PlayerBattle activeCharacter;
    private int activeCharacterIndex;
    private bool choosingTarget = false;

    private void Start()
    {
        //free mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        battleCanvas.gameObject.SetActive(false);
        targetCamera.gameObject.SetActive(true);
        multiTargetCamera.gameObject.SetActive(false);
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
        enemiesCount = 1;
   
        characters.Add(mothman);

        DecideTurnOrder();
        StartCoroutine(BattleLoop());
    }

    private void Update()
    {
        if (choosingTarget)
        {
            if (Input.GetKeyDown(KeyCode.Space))
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

        while(true)
        {
            activeCharacter = characters[activeCharacterIndex];

            if(activeCharacter.isPlayer)
            {

                ChangeCameraFocus();

                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(activeCharacter.PhysicalAttack);
                guardButton.onClick.RemoveAllListeners();
                guardButton.onClick.AddListener(activeCharacter.Guard);

                yield return StartCoroutine(PlayerAction());

            }
            else
            {

                yield return StartCoroutine(EnemyAction());
            }
            
            if(enemiesCount == 0)
            {
                GameManager.Instance.LoadGameOver(true);
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

            yield return StartCoroutine(AttackAnimation(activeCharacter.gameObject, mothman.gameObject));

            int hp = mothman.TakeDamage(rawDamage);

            yield return new WaitForSeconds(1f);
            mothman.ToggleActiveCanvas();

            if(hp <= 0)
            {
                mothman.gameObject.SetActive(false);
                characters.Remove(mothman);
                enemiesCount--;
            }
        }    

        activeCharacter.ToggleActiveCanvas();
    }

    IEnumerator EnemyAction() 
    {
        yield return StartCoroutine(activeCharacter.StartAction());
        int rawDamage = activeCharacter.getDamage();

        if (mothman.specialAttack)
        {
            yield return StartCoroutine(MultiAttackAnimation());

            foreach(PlayerBattle target in party)
            {
                target.TakeDamage(rawDamage);
            }

            yield return new WaitForSeconds(1f);

            multiTargetCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);

            foreach (PlayerBattle target in party)
            {
                if(target.HP <= 0)
                {
                    target.gameObject.SetActive(false);
                    characters.Remove(target);
                    partyCount--;
                }
            }
        }
        else if(!mothman.charging)
        {
            int target = mothman.GetTarget(party);

            yield return StartCoroutine(AttackAnimation(activeCharacter.gameObject, party[target].gameObject));

            int hp = party[target].TakeDamage(rawDamage);
            yield return new WaitForSeconds(1f);

            if (hp <= 0)
            {
                party[target].gameObject.SetActive(false);
                characters.Remove(party[target]);
                party.RemoveAt(target);
                partyCount--;
            }

            battleCanvas.gameObject.SetActive(false);
        }
        else
        {
            yield return StartCoroutine(ChargingAnimation());
        }
    }

    private void ChooseTarget()
    {
        choosingTarget = true;
        SwapCamera();
        mothman.ToggleActiveCanvas();
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
        if(attacker == mothman.gameObject)
        {
            transposer.m_FollowOffset = new Vector3(0, 7, -10f);
        }
        else
        {
            transposer.m_FollowOffset = new Vector3(0, 3, 4.5f);
        }

        activeCharacter.gameObject.GetComponent<Animator>().SetTrigger("Attack");
        yield return new WaitForSeconds(1f);

        vcam.Follow = target.transform;
        vcam.LookAt = target.transform.parent.Find("LookAt").transform;
        if(target == mothman.gameObject)
        {
            transposer.m_FollowOffset = new Vector3(0, 7, -10f);
        }
        else
        {
            transposer.m_FollowOffset = new Vector3(0, 3, 4.5f);
        }

        target.GetComponent<Animator>().SetTrigger("Hit");
    }

    IEnumerator MultiAttackAnimation()
    {
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();

        vcam.Follow = mothman.gameObject.transform;
        vcam.LookAt = mothman.gameObject.transform.parent.Find("LookAt").transform;
        transposer.m_FollowOffset = new Vector3(0, 7, -10f);

        mothman.gameObject.GetComponent<Animator>().SetTrigger("Attack");
        yield return new WaitForSeconds(1f);

        mainCamera.gameObject.SetActive(false);
        multiTargetCamera.gameObject.SetActive(true);

        foreach(PlayerBattle target in party)
        {
            GameObject targetObject = target.gameObject;
            targetObject.GetComponent<Animator>().SetTrigger("Hit");
        }
    }

    IEnumerator ChargingAnimation()
    {
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();

        vcam.Follow = mothman.gameObject.transform;
        vcam.LookAt = mothman.gameObject.transform.parent.Find("LookAt").transform;
        transposer.m_FollowOffset = new Vector3(0, 7, -10f);

        mothman.gameObject.GetComponent<Animator>().SetTrigger("Charging");
        StartCoroutine(mothman.Spin());
        yield return new WaitForSeconds(1f);
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
                transposer.m_FollowOffset = new Vector3(2.5f, 2, -4);
                break;
            case "Junpei(TurnBased)":
                transposer.m_FollowOffset = new Vector3(1f, 2, -4);
                break;
            case "Mitsuru(TurnBased)":
                transposer.m_FollowOffset = new Vector3(-0.5f, 2, -4);
                break;
            case "Yukari(TurnBased)":
                transposer.m_FollowOffset = new Vector3(-2f, 2, -4);
                break;
        }

        activeCharacter.GetComponent<Animator>().SetTrigger("Idle");
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

