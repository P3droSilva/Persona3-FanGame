using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YukariBattle : PlayerBattle
{
    void Awake()
    {
        strength = 50;
        magic = 91;
        endurance = 55;
        agility = 55;
        luck = 53;
        isPlayer = true;

        if(GameManager.Instance.yukariHealth == -1)
        {
            HP = 100 + (endurance * 10);
            GameManager.Instance.yukariHealth = HP;
        }
        else
        {
            HP = GameManager.Instance.yukariHealth;
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
        GameManager.Instance.yukariHealth = HP;
    }

    public override void Fire()
    {
        Debug.Log("Yukari used Fire!");
    }
}
