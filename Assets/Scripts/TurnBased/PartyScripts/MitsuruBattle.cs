using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MitsuruBattle : PlayerBattle
{
    void Awake()
    {
        strength = 55;
        magic = 85;
        endurance = 52;
        agility = 61;
        luck = 51;
        isPlayer = true;

        HP = 100 + (endurance * 10);
        SP = 100 + magic;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));

        SetHpBar();
    }

    protected override void SetHpBar()
    {
        base.SetHpBar();
    }

    public override void Fire()
    {
        Debug.Log("Mitsuru used Fire!");
    }
}
