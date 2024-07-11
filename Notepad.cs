using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notepad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //�I�u�W�F�N�g�ASE�̕\����\����ݒ�
        if (other.tag == StringDate.Player)
        {
#if !UNITY_EDITOR
        Cursor.visible = true;
#endif
            Time.timeScale = 0;
            AudioSystem.instance.PlaySEFromObjectPosition(StringDate.Memo, gameObject);
            UiManeger.instance.ActiveMemo(true);
            UiManeger.instance.ActiveBackButtom(true);
            GameManeger.instance.SetPlayerStop(true);
            Destroy(gameObject);
        }
    }
}
