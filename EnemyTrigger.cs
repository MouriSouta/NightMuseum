using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject[] m_Enemies;

    public void SpawnEnemy()
    {
        foreach (Transform spawnpoint in transform)
        {
            int index = Random.Range(0, m_Enemies.Length);
            Instantiate(m_Enemies[index], spawnpoint.position,Quaternion.identity);
            GameManeger.instance.EnemyDethCount ++;
        }
    }
}
