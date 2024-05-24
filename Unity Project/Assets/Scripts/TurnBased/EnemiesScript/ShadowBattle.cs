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
        Debug.Log("Shadow's Turn");
        DecideAction();
        yield return new WaitForSeconds(1f);
    }

    private void DecideAction()
    {
        PhysicalAttack();
    }

    public override void Fire()
    {
        Debug.Log("Shadow used Fire!");
    }
}
