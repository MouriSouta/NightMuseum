using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private WeaponDeta Weapon;
    [SerializeField]
    private WeaponDeta.State m_ItemSerect;

    private Animator m_Animator;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(StringDate.Player);
        m_Animator = player.GetComponent<Animator>();
    }

    public GameObject passItem()
    {
        ItemManeger.instance.NextWepoan = gameObject;
        gameObject.SetActive(false);
        return Weapon.Weapons[(int)m_ItemSerect];
    }

    public void HitStop()
    {
        if(m_ItemSerect == WeaponDeta.State.Bar)
        {
            m_Animator.speed = 0;
            _ = Wait.WaitTime(0.1f, () => m_Animator.speed = 1);
        }
    }
}
