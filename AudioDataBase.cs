using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "CreateData/AudioData")]
public class AudioDataBase : ScriptableObject
{
    [Header("BGMデータ")]
    public AudioData[] m_BGMData;

    [Header("SEデータ")]
    public AudioData[] m_SEData;

    [System.Serializable]
    public struct AudioData
    {
        [SerializeField, Tooltip("名前")]
        string m_Name;
        [SerializeField, Tooltip("音源")]
        AudioClip m_AudioClip;

        public string Name => m_Name;
        public AudioClip AudioClip => m_AudioClip;
    }
}
