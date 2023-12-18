using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBattle : MonoBehaviour
{
    [SerializeField] protected HealthBar HpBar;
    [SerializeField] protected Canvas HpCanvas;

    public int strength;
    public int magic;
    public int endurance;
    public int agility;
    public int luck;
    public int SP;
    public int HP;
    public int initiative;
    public bool isPlayer;
    public bool isGuarding = false;
    private bool waitingForAction = false;

    protected int rawDamage = -1;

    void Awake()
    {
        strength = 50;
        magic = 50;
        endurance = 50;
        agility = 50;
        luck = 50;
        isPlayer = true;

        HP = 100 + (endurance * 10);
        SP = 100 + magic;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));

        SetHpBar();
    }

    protected virtual void SetHpBar()
    {
        int maxHP = 100 + (endurance * 10);
        HpBar.SetMaxHP(maxHP);
        HpBar.SetHP(HP);
    }

    public virtual void SaveStats()
    {
        return;
    }

    public virtual IEnumerator StartAction()
    {
        waitingForAction = true;

        if(isGuarding)
        {
            gameObject.GetComponent<Animator>().SetTrigger("Idle");
            isGuarding = false;
        }

        while (waitingForAction)
        {
            yield return null;
        }
    }

    public int getDamage()
    {
        int dmg = rawDamage;
        rawDamage = -1;
        return dmg;
    }

    public int TakeDamage(int rawDamage)
    {
        int damage = rawDamage - endurance - UnityEngine.Random.Range(1, luck);
        if (damage < 0)
        {
            damage = 0;
        }

        if(isGuarding)
        {
            damage = damage / 4;
            isGuarding = false;
            gameObject.gameObject.GetComponent<Animator>().SetTrigger("Idle");
        }

        HP -= damage;
        if (HP < 0)
            HP = 0;

        HpBar.SetHP(HP);

        Debug.Log("Took " + damage + " damage!");
        return HP;
    }

    public void PhysicalAttack()
    {
        rawDamage = strength * 3 + UnityEngine.Random.Range(1, luck);
        waitingForAction = false;
    }

    public void Guard()
    {
        isGuarding = true;
        waitingForAction = false;
    }

    virtual public void Fire()
    {

    }

    public void ToggleActiveCanvas()
    {
        if(HpCanvas.gameObject.activeSelf == true)
        {
            HpCanvas.gameObject.SetActive(false);
        }
        else
        {
            HpCanvas.gameObject.SetActive(true);
        }
    }
}
