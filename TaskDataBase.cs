using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TaskDate", menuName = "Mydate/TaskDate")]
public class TaskDataBase : ScriptableObject
{
    private int m_CurrentTaskIndex;

    [System.Serializable]
    public struct TaskData
    {
        [SerializeField, Multiline(3)]
        private string m_TaskText;
        public string TaskText => m_TaskText;

       

        [HideInInspector]
        public bool IsClearedTask;
    }

    [SerializeField]
    private TaskData[] m_TaskDatas;

    public TaskData[] TaskDatas => m_TaskDatas;

    public void Initialize()
    {
        m_CurrentTaskIndex = 0;

        for(int i = 0; i < m_TaskDatas.Length;i++)
        {
            m_TaskDatas[i].IsClearedTask = false;
        }
    }

    public int CurrentTaskIndex()
        => m_CurrentTaskIndex;

    public string DisplayCurrentTaskText()
    {
        return m_TaskDatas[m_CurrentTaskIndex].TaskText;
    }

    public void TaskClear()
    {
        if (m_TaskDatas.Length <= m_CurrentTaskIndex) return;

        m_TaskDatas[m_CurrentTaskIndex].IsClearedTask = true;

        m_CurrentTaskIndex++;
    }

}
