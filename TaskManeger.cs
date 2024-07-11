using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskManeger : MonoBehaviour
{
    //タスクデータ
    [SerializeField]
    private TaskDataBase m_TaskDate;

    [SerializeField]
    private Text m_TaskText;

    [System.Serializable]
    private struct TaskEventData
    {
        [SerializeField]
        private UnityEvent m_TaskClearEvent;
        public UnityEvent TaskClearEvent => m_TaskClearEvent;
    }

    [SerializeField]
    private TaskEventData[] m_TaskEventDatas;

    private void Awake()
    {
        m_TaskDate.Initialize();
    }

    public bool CheckPrevTaskCleared(int index)
    {
        int prevIndex = index - 1;
        prevIndex = Mathf.Clamp(prevIndex,0,m_TaskDate.TaskDatas.Length-1);
        return m_TaskDate.TaskDatas[prevIndex].IsClearedTask;
    }

    public void TaskClear()
    {
        m_TaskDate.TaskClear();

        if (m_TaskEventDatas[m_TaskDate.CurrentTaskIndex()].TaskClearEvent != null)
            m_TaskEventDatas[m_TaskDate.CurrentTaskIndex()].TaskClearEvent.Invoke();
        
        DisplayTask();
    }

    public int CurentTask() => m_TaskDate.CurrentTaskIndex();

    public TaskManeger DisplayTask()
    {
        m_TaskText.text = m_TaskDate.DisplayCurrentTaskText();
        return this;
    }
}
