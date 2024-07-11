using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFiver : MonoBehaviour
{
    [SerializeField]
    private GameObject[] m_Enemies;

    [SerializeField]
    private Transform[] m_SpawnPoints;

    private int m_SelectIndex;
    public void SetIndex(int index)
    {
        m_SelectIndex = index;
    }

    public void FiverEnemies()
    {
        Transform spawnPoint = m_SpawnPoints[m_SelectIndex];

        foreach(Transform s in spawnPoint)
        {
            int index = Random.Range(0, m_Enemies.Length);
            Instantiate(m_Enemies[index], s.position, Quaternion.identity);
            GameManeger.instance.EnemyDethCount++;
        }
    }
}
