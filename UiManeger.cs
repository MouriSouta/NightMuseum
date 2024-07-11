using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManeger : MonoBehaviour
{
    public static UiManeger instance;

    [SerializeField]
    private GameObject m_activeMemo;
    [SerializeField]
    private GameObject m_activeBackButtom;
    [SerializeField]
    private GameObject m_activeTask;


    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void ActiveMemo(bool activeMemo)
    {
        m_activeMemo.SetActive(activeMemo);
    }
    public bool GetActiveMemo() => m_activeMemo;

    public void ActiveBackButtom(bool activeBackButtom)
    {
        m_activeBackButtom.SetActive(activeBackButtom);
    }
    public bool GetActiveBackButtom() => m_activeBackButtom;

    public void ActiveTask(bool activeTask)
    {
        m_activeTask.SetActive(activeTask);
    }
    public bool GetActiveTask() => m_activeTask;
}
