using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManeger : MonoBehaviour
{
    //カギを入れる為のList
    [SerializeField]
    private GameObject[] m_Keys;
    //ランダムで一つのカギを表示
    private GameObject m_ClearKey;

    [SerializeField]
    private EnemyFiver EnemyFiver;

    private void Start()
    {
        int rand = Random.Range(0, m_Keys.Length);
        //ランダムで一つだけ呼ぶ
        m_ClearKey = m_Keys[rand];
        
        EnemyFiver.SetIndex(rand);

        m_ClearKey.AddComponent<KeyScript>();
        m_ClearKey.AddComponent<BoxCollider>().isTrigger = true;
        //一旦カギを全て非表示
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