using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManeger : MonoBehaviour
{
    public static ItemManeger instance;
    //¡‚Ì•Ší
    private GameObject m_CurrentWepoan;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public GameObject NextWepoan { set => m_CurrentWepoan = value; }

    public void NewWepoan()
    => m_CurrentWepoan.SetActive(true);
}
