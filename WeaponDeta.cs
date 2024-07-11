using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDeta", menuName = "Mydate/WeaponDeta")]
public class WeaponDeta : ScriptableObject
{
    [SerializeField]
    private GameObject[] m_Weapons;
    public GameObject[] Weapons => m_Weapons;

    public enum State
    {
        Bar,
        JapanSwoerd,
    }

    private State I_state = State.Bar;

    public State state { get => I_state; set => I_state = value; }
}
