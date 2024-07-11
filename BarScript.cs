using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarScript : MonoBehaviour
{
    private void Start()
    {
        GameManeger.instance.setEventItem(gameObject);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == StringDate.Player)
        {
            AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ItemGet, gameObject);
            other.GetComponent<PlayerAnimation>().SetInt(StringDate.State, 1, true);

            GameManeger.instance.TaskClear();
        }
    }
}
