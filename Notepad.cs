using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notepad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //オブジェクト、SEの表示非表示を設定
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
