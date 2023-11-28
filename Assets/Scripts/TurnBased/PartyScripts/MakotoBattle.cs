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

        HP = 100 + (endurance * 10);
        SP = 100 + magic;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));
    }

    public override void Fire()
    {
        Debug.Log("Makoto used Fire!");
    }
}
