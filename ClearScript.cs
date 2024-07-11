using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == StringDate.Player)
        {
            bool check = other.GetComponent<Player>().ClearFrag();
            if (check)
            {
                SceneManagment.instance.ClearScene();
            }
        }
    }
}