using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoDisplay : MonoBehaviour
{
    public void BackStageScene()
    {
        //�J�[�\���\���A��\��
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
        Time.timeScale = 1;
        GameManeger.instance.SetPlayerStop(false);

        UiManeger.instance.ActiveMemo(false);
        UiManeger.instance.ActiveBackButtom(false);

        UiManeger.instance.ActiveTask(true);

        GameManeger.instance.TaskDisplay();
        GameManeger.instance.ItemDisplay();
    }
}
