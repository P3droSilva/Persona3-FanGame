using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MothmanBattle : PlayerBattle
{

    public bool charging = false;
    public bool specialAttack = false;

    void Awake()
    {
        strength = 100;
        magic = 125;
        endurance = 130;
        agility = 80;
        luck = 50;
        isPlayer = false;

        HP = 100 + (endurance * 10);
        SP = 100000;
        initiative = agility + UnityEngine.Random.Range(1, (luck / 2));

        SetHpBar();
    }

    protected override void SetHpBar()
    {
        Transform parentTransform = this.gameObject.transform.parent;
        Transform hpCanvasTransform = parentTransform.Find("HealthCanvas");
        HpCanvas = hpCanvasTransform.GetComponent<Canvas>();
        Transform hpBarTransform = hpCanvasTransform.Find("HealthBar");
        HpBar = hpBarTransform.GetComponent<HealthBar>();

        HpBar.SetMaxHP(HP);
    }

    public override IEnumerator StartAction()
    {
        Debug.Log("Mothman's Turn");
        DecideAction();
        yield return new WaitForSeconds(1f);
    }

    private void DecideAction()
    {
        specialAttack = false;

        if(!charging)
        {
            int rand = UnityEngine.Random.Range(0, 100);
            if (rand > 30)
                PhysicalAttack();
            else
                SpecialAttack();
        }
        else
        {
            SpecialAttack();
        }
        
    }

    private void SpecialAttack()
    {
        if(!charging)
        {
            charging = true;
            return;
        }

        charging = false;

        rawDamage = magic * 3 + UnityEngine.Random.Range(1, luck);
        specialAttack = true;
    }

    public int GetTarget(List<PlayerBattle> party)
    {
        int target = 0;
        int lowestHP = 1000000;
        int rand = UnityEngine.Random.Range(0, 100);

        if(rand < 40)
        {
            target = UnityEngine.Random.Range(0, party.Count);
            return target;
        }
        for(int i = 0; i < party.Count; i++)
        {
            if (party[i].HP < lowestHP)
            {
                lowestHP = party[i].HP;
                target = i;
            }
        }

        return target;
    }

    public override void Fire()
    {
        Debug.Log("Shadow used Fire!");
    }

    public IEnumerator Spin()
    {
        Transform childMothman = gameObject.transform.GetChild(0);
        float totalRotationTime = 1.0f; // Total duration for the spin in seconds
        float elapsedTime = 0f;

        Quaternion startRotation = childMothman.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 360f, 0f); // 360 degrees around the Y-axis

        while (elapsedTime < totalRotationTime)
        {
            childMothman.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / totalRotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        // Ensure the final rotation is exactly the target rotation to avoid small errors.
        childMothman.rotation = startRotation;
    }
}
