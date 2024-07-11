using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManeger : MonoBehaviour
{
    [SerializeField]
    private GameObject m_HpUI;
    [SerializeField]
    private Image m_BloodImage;

    private int m_MaxHp;

    private List<Transform> HP;

    private void Awake()
    {
        m_BloodImage.color = new Color(1.0f, 1.0f, 1.0f, 0f);

        HP = new List<Transform>();
        for (int i = 0; i < m_HpUI.transform.childCount; i++)
        {
            HP.Add(m_HpUI.transform.GetChild(i));
        }
        m_MaxHp = HP.Count;
    }


    public void BrokenHeart()
    {
        HP[HP.Count-1].GetComponent<Image>().enabled = false;
        HP.RemoveAt(HP.Count - 1);

        Color color = m_BloodImage.color;
        color.a = 1.0f / m_MaxHp * (m_MaxHp - HP.Count);
        m_BloodImage.color = color;
    }
}
