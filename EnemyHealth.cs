using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    EnemyBase enemy;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    public override void Damage(int value)
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieDamageSE, gameObject);

        if (Is_Death)
        {
            return;
        }
        base.Damage(value);

        if(!Is_Death)
        {
            enemy.DamageAnim();
        }

    }

    public override void Death()
    {
        enemy.Death();
    }
}
