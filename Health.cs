using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Health : MonoBehaviour, IDamagable
{
    [SerializeField]
    protected int m_Health;
    public int e_Health => m_Health;

    protected bool Is_Death;

    private void Awake()
    {
        Is_Death = false;
    }

    public virtual void Damage(int Value)
    {
        m_Health -= Value;

        if(m_Health <= 0)
        {
            Is_Death = true;
            Death();
        }
    }

    public abstract void Death();
}
