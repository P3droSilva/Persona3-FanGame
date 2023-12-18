using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] public float startTimeBtwAttack = 0.5f;

    private List<GameObject> EnemiesInRange;
    private bool mothmanInRange;
    private Animator anim;
    private float timeBtwAttack;
    private bool attacking;
    private GameObject enemyAttacked;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        timeBtwAttack = 0;
        attacking = false;

        //get public list of enemies in range from AttackingArea.cs that is located in a children of this object
        EnemiesInRange = GetComponentInChildren<AttackingArea>().EnemiesInRange;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if(attacking)
        {
            if(EnemiesInRange.Count > 0)
            {
                enemyAttacked = EnemiesInRange[0];
                Invoke("StartBattleWithDelay", 0.5f);
            }

            if(GetComponentInChildren<AttackingArea>().mothmanInRange)
            {
                GameManager.Instance.LoadBossBattle();
            }
        }
    }

    void GetInput()
    {
        if(timeBtwAttack <= 0)
        {
            if(Input.GetKey(KeyCode.Space))
            {
                anim.SetTrigger("Attack");
                timeBtwAttack = startTimeBtwAttack;

                attacking = true;
            }
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }
    }

    public void FinishAttack()
    {
        attacking = false;
    }

    void StartBattleWithDelay()
    {
        GameManager.Instance.LoadBattleScene(true, enemyAttacked);
    }

}
