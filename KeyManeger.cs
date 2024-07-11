using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManeger : MonoBehaviour
{
    //�J�M������ׂ�List
    [SerializeField]
    private GameObject[] m_Keys;
    //�����_���ň�̃J�M��\��
    private GameObject m_ClearKey;

    [SerializeField]
    private EnemyFiver EnemyFiver;

    private void Start()
    {
        int rand = Random.Range(0, m_Keys.Length);
        //�����_���ň�����Ă�
        m_ClearKey = m_Keys[rand];
        
        EnemyFiver.SetIndex(rand);

        m_ClearKey.AddComponent<KeyScript>();
        m_ClearKey.AddComponent<BoxCollider>().isTrigger = true;
        //��U�J�M��S�Ĕ�\��
        foreach (GameObject Key in m_Keys)
        {
            Key.SetActive(false);
        }
    }

    public void KeyDisplay()
    {
        m_ClearKey.SetActive(true);
    }

    public void HaveKey()
    {
        m_ClearKey.SetActive(false);
    }
}