using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == StringDate.Player)
        {
            GameManeger.instance.TaskClear();
        }
    }
}
