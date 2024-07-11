using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    Player player;

    private LifeManeger LifeUI;

    private void Awake()
    {
        player = GetComponent<Player>();
        LifeUI = GetComponent<LifeManeger>();
    }

    public override void Damage(int Value)
    {
        LifeUI.BrokenHeart();

        if (Is_Death)
        {
            return;
        }

        base.Damage(Value);


        if (!Is_Death)
        {
            player.Damage();
        }
    }

    public override void Death()
    {
        player.Death();
    }

    public int GetHealth() => m_Health;
}
