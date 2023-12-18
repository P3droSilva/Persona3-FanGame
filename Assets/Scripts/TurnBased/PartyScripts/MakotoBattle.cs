using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakotoBattle : PlayerBattle
{
    void Awake()
    {
        strength = 68;
        magic = 68;
        endurance = 68;
        agility = 68;
        luck = 68;
        isPlayer = true;


        if(GameManager.Instance == null)
        {
            HP = 100 + (endurance * 10);  
        }
        else if(GameManager.Instance.makotoHealth == -1)
        {
            HP = 100 + (endurance * 10);
            GameManager.Instance.makotoHealth = HP;
        }
        else
        {
            HP = GameManager.Instance.makotoHealth;
            if (HP == 0)
                HP = 1;
        }

        SP = 100 + magic;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));

        SetHpBar();
    }

    protected override void SetHpBar()
    {
        base.SetHpBar();
    }

    public override void SaveStats()
    {
        GameManager.Instance.makotoHealth = HP;
    }

    public override void Fire()
    {
        Debug.Log("Makoto used Fire!");
    }
}
