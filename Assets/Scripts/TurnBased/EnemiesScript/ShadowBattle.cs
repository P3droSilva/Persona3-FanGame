using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBattle : PlayerBattle
{
    void Awake()
    {
        strength = 70;
        magic = 55;
        endurance = 40;
        agility = 60;
        luck = 40;
        isPlayer = false;

        HP = 100 + (endurance * 10);
        SP = 100000;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));
    }

    public override void Fire()
    {
        Debug.Log("Shadow used Fire!");
    }
}
