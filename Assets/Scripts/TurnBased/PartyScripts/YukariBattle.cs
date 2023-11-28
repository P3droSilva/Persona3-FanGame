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

        HP = 100 + (endurance * 10);
        SP = 100 + magic;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));
    }

    public override void Fire()
    {
        Debug.Log("Yukari used Fire!");
    }
}
