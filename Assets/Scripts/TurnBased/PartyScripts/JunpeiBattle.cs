using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class JunpeiBattle : PlayerBattle
{
    void Awake()
    {
        strength = 82;
        magic = 44;
        endurance = 69;
        agility = 56;
        luck = 53;
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
        Debug.Log("Junpei used Fire!");
    }
}
