using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManeger : MonoBehaviour
{
    public static GameManeger instance;

    private bool is_Event;

    //メモを取ってる間機能停止
   // public bool m_Pause;

    private int m_Textnum;

    private int m_PrevTextnum;

    private TaskManeger m_TaskManeger;

    private GameObject m_Item;

    private GameObject m_ClearItem;

    private int m_EnmeyDethCount;

    private void Start()
    {
        FadeManager.instance.FadeIn(SceneManagment.instance.FadeTime);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        m_TaskManeger = GameObject.FindGameObjectWithTag(StringDate.TextManeger).GetComponent<TaskManeger>();
    }

    private void Update()
    {
        if(m_EnmeyDethCount <= 0 && m_TaskManeger.CurentTask() == 3)
        {
            GameManeger.instance.TaskClear();
        }
    }

    public void ItemDisplay()
    {
        m_Item.SetActive(true);
    }

    public bool CheckPrevTaskCleared(int index)
    {
       return m_TaskManeger.CheckPrevTaskCleared(index);
    }

    public void TaskDisplay()
    {
        m_TaskManeger.DisplayTask();
    }

    public GameManeger TaskClear()
    {
        m_TaskManeger.TaskClear();
        return this;
    }


    public void setEventItem(GameObject value)
        => m_Item = value;

    public void setClearKey(GameObject value)
        => m_ClearItem = value;

    /// <summary>
    /// 敵の数をカウント
    /// </summary>
    public int EnemyDethCount { get => m_EnmeyDethCount; set => m_EnmeyDethCount = value; }

    /// <summary>
    /// プレイやのストップ処理
    /// </summary>
    public bool GetPlayerStop()
        => is_Event;

    public bool SetPlayerStop(bool value)
        => is_Event = value;

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        Cursor.visible = true;
#endif
    }
}
