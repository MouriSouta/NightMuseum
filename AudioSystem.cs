using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using MS.ObjectPool;

    public class AudioSystem : MonoBehaviour
    {
        public static AudioSystem instance;

        [SerializeField]
        private AudioSource m_AudioBgm;

        [SerializeField]
        private AudioMixer m_Mixer;

        [SerializeField]
        private AudioDataBase m_AudioDataBase;

        [SerializeField]
        private AudioMixerGroup m_MixerGroupSe;

        [SerializeField]
        private GameObject[] m_SePrefabs;

        [SerializeField]
        private float m_Max = 5.0f;
        [SerializeField]
        private float m_Min = 0.0f;

        private AudioSource m_VolumeSe;

        [SerializeField]
        private int m_MaxSeCount = 30;

        [SerializeField]
        private int m_SeCount;

        private List<GameObject> m_AudioObjectList;
        private ObjectPool<AudioSource>[] m_Pools;

        private float m_FadeSpeed;

        private WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();

        private Dictionary<int, Coroutine> m_CheckCoroutineDic = new();

        private int m_CoroutineNumber = 0;

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                //������
                m_AudioObjectList = new List<GameObject>();
                m_CheckCoroutineDic = new();
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnLoaded;

                m_Pools = new ObjectPool<AudioSource>[m_SePrefabs.Length];
                for(int i = 0;i < m_SePrefabs.Length; i++)
                {
                    m_Pools[i] = new(m_SePrefabs[i], transform);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        ///<summary>
        ///BGM��炷
        ///</summary>
        ///<param name="name">���̖��O</param>
        ///<param name="loop">���[�v���邩�ǂ���</param>
        public AudioSystem PlayBGM(string name, bool loop = true)
        {
            //�����Ȃ��A�܂���None�Ɠ��͂����ꍇ�͏������Ȃ��悤�ɂ���
            if (name == "" || name == "None") return this;

            m_AudioBgm.loop = loop;
            for(int i = 0; i < m_AudioDataBase.m_BGMData.Length; i++)
            {
                //Type��BGM����Ȃ���Ύ��̃��[�v�ɂ���
                if(m_AudioDataBase.m_BGMData[i].Name == name)
                {
                    if(m_AudioBgm.isPlaying)
                    {
                        m_AudioBgm.Stop();
                    }
                    m_AudioBgm.clip = m_AudioDataBase.m_BGMData[i].AudioClip;
                    m_AudioBgm.Play();
                }
            }

            if (!m_AudioBgm.isPlaying)
                Debug.LogError("Clip��������܂���ł����@Name:" + name);

            return this;
        }

        ///<summary>
        ///�v�f�ԍ����w�肵��BGM��炷
        ///</summary>
        ///<param name="index">AudioData�̗v�f�ԍ�</param>
        ///<param name="loop">���[�v���邩�ǂ���</param>
        ///<returns></returns>
        public AudioSystem PlayBGM(int index, bool loop = true)
        {
            m_AudioBgm.loop = loop;

            if(m_AudioDataBase.m_BGMData.Length <= index)
            {
                Debug.LogError($"�z�񂪒����Ă��܂��@�v�f��:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //���łɉ������Ă���ꍇ�͈�x��~���Ă���Đ�
            if(m_AudioBgm.isPlaying)
            {
                m_AudioBgm.Stop();
            }

            //AudioData���ɂ���BGMData��Clip�������Ė炷
            //�Ȃ�Ȃ��ꍇ�̓G���[���o��
            m_AudioBgm.clip = m_AudioDataBase.m_BGMData[index].AudioClip;
            m_AudioBgm.Play();

            if (!m_AudioBgm.isPlaying)
                Debug.LogError("Clip��������܂���ł����@Name:" + name);

            return this;
        }

        ///<summary>
        ///SE�炷
        ///</summary>
        ///<param name="name">SE�̖��O</param>
        ///<param name="seObjectIndex">�ǂ̃v���n�u���g�p���邩</param>
        ///<param name="pitch">���̍����̒���</param>
        ///<param name="loop">���[�v���邩�ǂ���</param>
        public AudioSystem PlaySE(string name, int seObjectIndex = 0,Vector3 position = new Vector3(),float volume = 1f, float pitch = 1, bool loop = false)
        {
            //���������A�܂���None�Ɠ��͂����ꍇ�͏������Ȃ��悤�ɂ���
            if (name == "" || name == "None") return this;

            //����ȏ�AudioSource���������特��炳�Ȃ��悤�ɂ���
            if (m_MaxSeCount < m_SeCount) return this;
            //index�̒l��Length�̒l�𒴂��Ă�����G���[��\������
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index�̒l���I�[�o�[���Ă��܂��@index:" + seObjectIndex);
                return this;
            }

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if (m_Max < pitch) pitch = m_Max;
            else if (m_Min > pitch) pitch = m_Min;
            //�s�b�`��ύX����
            audioSourceSe.pitch = pitch;

            for(int i = 0; i < m_AudioDataBase.m_SEData.Length; i++)
            {
                //Type��BGM����Ȃ���Ύ��̃��[�v
                if(m_AudioDataBase.m_SEData[i].Name == name)
                {
                    //Loop�Ȃ�Play
                    if (loop)
                    {
                        audioSourceSe.clip = m_AudioDataBase.m_SEData[i].AudioClip;
                        audioSourceSe.Play();
                    }
                    else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[i].AudioClip);

                    m_SeCount++;
                    break;
                }
            }
            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip��������܂���@Name:" + name);

            //�������񂾂�A���g�ɂ��Ă��邱��AudioSource���폜����
            if(!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                 {
                     m_Pools[seObjectIndex].Release(audioSourceSe);
                     m_SeCount--;
                 }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }
            return this;
        }

        /// <summary>
        /// �v�f�ԍ����w�肵��SE��炷
        /// ��������̂�AudioManeger��
        /// </summary>
        /// <param name="index">SE�̗v�f�ԍ�</param>
        /// <param name="seObjectindex">�ǂ̃v���n�u���g�p���邩</param>
        /// <param name="pitch">���̍�������</param>
        /// <param name="loop">���[�v���邩�ǂ���</param>
        /// <returns></returns>
        public AudioSystem PlaySE(int index, int seObjectIndex = 0, Vector3 position = new Vector3(), float volume = 1f, float pitch = 1, bool loop = false)
        {
            //����ȏ�AudioSource���������特��炳�Ȃ��悤�ɂ���
            if (m_MaxSeCount < m_SeCount) return this;

            //index�̒l��Length�̒l�𒴂��Ă�����G���[��\��
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("Index�̒l���I�[�o�[���Ă��܂��@index:" + seObjectIndex);
                return this;
            }

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if (m_Max < pitch) pitch = m_Max;
            else if (m_Min > pitch) pitch = m_Min;
            //�s�b�`��ύX����
            audioSourceSe.pitch = pitch;

            //�z�񂪒������Ă�����G���[��\��
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($"�z�񂪒����Ă��܂��@�v�f��:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //���[�v����ꍇPlay�A���[�v���Ȃ��ꍇ��OneShot�ɂ���
            if (loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip��������܂���Name:" + name);

            //�������񂾂�A���g�ɂ��Ă��邱��AudiioSource���폜����
            //���񂾍ۂɃR�[���o�b�N���ݒ肳��Ă���ꍇ�͂��̃R�[���o�b�N������
            if (!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                {
                    m_Pools[seObjectIndex].Release(audioSourceSe);
                    m_SeCount--;
                }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }
            return this;
        }

        ///<summary>
        ///�w�肵���I�u�W�F�N�g����SE�炷
        /// ��������̂̓I�u�W�F�N�g��
        ///</summary>
        ///<param name="name">SE�̖��O</param>
        ///<param name="parentObj">�e�ƂȂ�I�u�W�F�N�g</param>
        ///<param name="offset">�I�u�W�F�N�g�𐶐�����ʒu�ɒ���</param>
        ///<param name="seObjectIndex">�ǂ̃v���n�u���g�p���邩</param>
        ///<param name="pitch">���̍����̒���</param>
        ///<param name="loop">���[�v���邩�ǂ���</param>
        public AudioSystem PlaySEAsChild(string name, Transform parent, Vector3? offset = null, int seObjectIndex = 0, float volume = 1f, float pitch = 9999, bool loop = false)
        {
            //���̖����A�܂���None�Ɠ��͂����ꍇ�͏������Ȃ��悤�ɂ���
            if (name == "" || name == "Name") return this;

            //����ȏ�AudioSource���������特��炳�Ȃ��悤�ɂ���
            if (m_MaxSeCount < m_SeCount) return this;
            //index�̒l��Length�̒l�𒴂��Ă�����G���[��\������
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index index:" + seObjectIndex);
                return this;
            }
            if (offset == null) offset = Vector3.zero;

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parent.position;
            audioSourceSe.transform.parent = parent;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //�s�b�`��ύX
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            bool is_Played = false;

            for(int i = 0; i < m_AudioDataBase.m_SEData.Length; i++)
            {
                if(m_AudioDataBase.m_SEData[i].Name == name && !is_Played)
                {
                    //Loop�Ȃ�Play
                    if (loop)
                    {
                        audioSourceSe.clip = m_AudioDataBase.m_SEData[i].AudioClip;
                        audioSourceSe.Play();
                    }
                    else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[i].AudioClip);
                    m_SeCount++;
                    is_Played = true;
                }
            }
            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip Nmae:" + name);

            //��t�����񂾂�A���g�ɂ��Ă��邱��AusdioSource���폜
            if(!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                {
                    m_Pools[seObjectIndex].Release(audioSourceSe);
                    m_SeCount--;
                }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }

            return this;
        }

        /// <summary>
        /// �v�f�ԍ���ݒ肵�A�w�肵���I�u�W�F�N�g����SE��炷
        /// ��������̂̓I�u�W�F�N�g��
        /// </summary>
        /// <param name="index">SE�̗v�f�ԍ�</param>
        /// <param name="parentObj">�e�ƂȂ�I�u�W�F�N�g</param>
        /// <param name="offset">�I�u�W�F�N�g�𐶐�����ʒu�̒���</param>
        /// <param name="seObjectIndex">�ǂ̃v���n�u���g�p���邩</param>
        /// <param name="pitch">���̍�������</param>
        /// <param name="loop">���[�v���邩�ǂ���</param>
        /// <returns></returns>
        public AudioSystem PlaySEAsChild(int index, Transform parent,Vector3?offset = null, int seObjectIndex = 0, float volume = 1, float pitch = 9999, bool loop = false)
        {
            //����ȏ�AudioSource���������特��炳�Ȃ�
            if (m_MaxSeCount < m_SeCount) return this;

            //index�̒l��Length�̒l�𒴂�������G���[��\��
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index�̒l���I�[�o�[���Ă��� index:" + seObjectIndex);
                return this;
            }

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parent.position;
            audioSourceSe.transform.parent = parent;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //�s�b�`��ύX
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            //�z�񂪒����Ă�����G���[��\��
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($"�z�񂪒����Ă��܂��@�v�f��:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //���[�v����ꍇ��Play�A���[�v���Ȃ��ꍇ��OneShot�ɂ���
            if(loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.Log("Clip��������܂��� Name:" + name);

            //������񂾂�A���g�ɂ��Ă��邱��AudioSource���폜
            //���񂾍ۂɃR�[���o�b�N���ݒ肳��Ă���ꍇ�͂��̃R�[���o�b�N������
            if (!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                {
                    m_Pools[seObjectIndex].Release(audioSourceSe);
                    m_SeCount--;
                }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }

            return this;
        }

        /// <summary>
        /// �w�肵���I�u�W�F�N�g����SE��炷
        /// ��������̂̓I�u�W�F�N�g�O
        /// </summary>
        /// <param name="name">SE�̖��O</param>
        /// <param name="parentObj">�w�肵���I�u�W�F�N�g</param>
        /// <param name="offset">�I�u�W�F�N�g�𐶐�����ʒu�̒���</param>
        /// <param name="seObjectIndex">�ǂ̃v���n�u���g�p���邩</param>
        /// <param name="pitch">���̍����̒���</param>
        /// <param name="loop">���[�v���邩</param>
        public AudioSystem PlaySEFromObjectPosition(string name, GameObject parentObj, Vector3? offset = null, int seObjectIndex = 0, float volume = 1f, float pitch = 9999, bool loop = false)
        {
            //�����Ȃ��A�܂���None�Ɠ��͂����ꍇ�͏������Ȃ��悤�ɂ���
            if (name == "" || name == "None") return this;

            //����ȏ�AudioSource���������特��炳�Ȃ��悤�ɂ���
            if (m_MaxSeCount < m_SeCount) return this;
            //index�̒l��Length�̒l�𒴂��Ă�����G���[��\��
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index index:" + seObjectIndex);
                return this;
            }
            if (offset == null) offset = Vector3.zero;

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parentObj.transform.position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //�s�b�`��ύX
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            bool is_Played = false;

            for(int i = 0;i < m_AudioDataBase.m_SEData.Length; i++)
            {
                if(m_AudioDataBase.m_SEData[i].Name == name && !is_Played)
                {
                    //Loop�Ȃ�Play
                    if (loop)
                    {
                        audioSourceSe.clip = m_AudioDataBase.m_SEData[i].AudioClip;
                        audioSourceSe.Play();
                    }
                    else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[i].AudioClip);
                    m_SeCount++;
                    is_Played = true;
                }
            }
            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip Name:" + name);

            //�������񂾂玩�g�ɂ��Ă��邱��AudioSource���폜����
            if (!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                {
                    m_Pools[seObjectIndex].Release(audioSourceSe);
                    m_SeCount--;
                }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }

            return this;
        }

        /// <summary>
        /// �v�f�ԍ���ݒ肵�A�w�肵���I�u�W�F�N�g����SE��炷
        /// ��������̂̓I�u�W�F�N�g�O
        /// </summary>
        /// <param name="index"></param>
        /// <param name="parentObj"></param>
        /// <param name="offset"></param>
        /// <param name="seObjectIndex"></param>
        /// <param name="pitch"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public AudioSystem PlaySEFromObjectPosition(int index, GameObject parentObj,Vector3? offset = null, int seObjectIndex = 0,float volume = 1f, float pitch = 9999, bool loop = false)
        {
            //����ȏ�AudioSource���������特��炳�Ȃ��悤�ɂ���
            if (m_MaxSeCount < m_SeCount) return this;

            //index�̒l��Length�̒l�𒴂����Ă�����G���[��\��
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index�̒l���I�[�o�[���Ă��܂� index:" + seObjectIndex);
                return this;
            }

            //AudioSource�̐����Ɛݒ�
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parentObj.transform.position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.volume = volume;
            audioSourceSe.loop = loop;

            if(pitch != 9999)
            {
                //�s�b�`��ύX
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            //�z�񂪒�������G���[��\��
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($":{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //���[�v����ꍇ��Play�A���[�v���Ȃ��ꍇ��OneShot�ɂ���
            if (loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip Name:" + name);

            //�������񂾂�A���g�ɂ��Ă��邱��AudioSource���폜
            //���񂾍ۂɃR�[���o�b�N���ݒ肳��Ă���ꍇ�͂��̃R�[���o�b�N������
            if(!loop)
            {
                m_CoroutineNumber++;
                Coroutine coroutine = StartCoroutine(Checking(audioSourceSe, m_CoroutineNumber, () =>
                {
                    m_Pools[seObjectIndex].Release(audioSourceSe);
                    m_SeCount--;
                }));
                m_CheckCoroutineDic.Add(m_CoroutineNumber, coroutine);
            }

            return this;
        }

        ///<summary>
        ///�������񂾎��ɃR�[���o�b�N
        /// </summary>
        /// <param name="audioi">���g��AudioSource�R���|�[�l���g</param>
        /// <param name="callback">�I�����ɌĂяo��</param>
        private IEnumerator Checking(AudioSource audio, int CoroutineNum, UnityAction callback)
        {
            yield return new WaitWhile(() => audio.isPlaying);

            callback();
            m_CheckCoroutineDic.Remove(CoroutineNum);
        }

        /// <summary>
        /// �V�[�����ǂݍ��܂ꂽ�Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_CheckCoroutineDic = new();
            m_CoroutineNumber = 0;

            m_AudioObjectList.Clear();
            m_SeCount = 0;
        }

        ///<summary>
        ///�V�[�����ǂݍ��܂��O�ɌĂ΂��
        /// </summary>
        /// <param name="scene"</param>
        private void OnSceneUnLoaded(Scene scene)
        {
            //�R���[�`�����쓮���Ă��Ȃ��ꍇError�ɂȂ�
            foreach(Coroutine coroutine in m_CheckCoroutineDic.Values)
            {
                StopCoroutine(coroutine);
            }

            Debugger.Log("�V�[���ǂݍ��݂܂�" + scene.name);
            foreach(GameObject obj in m_AudioObjectList)
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// �����ꎞ��~����
        /// </summary>
        public void PauseBgm()
            => m_AudioBgm.Pause();

        ///<summary>
        ///�����~����
        /// </summary>
        public void StopBgm()
            => m_AudioBgm.Pause();

        /// <summary>
        /// �����ĊJ����
        /// </summary>
        public void ReStartBgm()
            => m_AudioBgm.Play();

        /// <summary>
        /// �S�Ẳ��ʂ𒲐�
        /// </summary>
        /// <param name="volume">����</param>
        public void SetAllVolume(float volume)
            => m_Mixer.SetFloat("Master", volume);

        ///<summary>
        ///���ʒ���
        /// </summary>
        /// <param name="volume">����</param>
        public AudioSystem SetVolumeBgm(float volume)
        {
            m_AudioBgm.volume = volume;
            return this;
        }

        /// <summary>
        /// BGM�S�̂̉��ʂ�ύX
        /// </summary>
        /// <param name="volume">����</param>
        public void SetVolumeBgmMixer(float volume)
            => m_Mixer.SetFloat("BGM", volume);

        /// <summary>
        /// SE�S�̂̉��ʂ�ύX����
        /// </summary>
        /// <param name="volume">����</param>
        public void SetVolumeSeMixer(float volume)
            => m_Mixer.SetFloat("SE", volume);

        /// <summary>
        /// ���X�ɉ������������Ă���
        /// </summary>
        /// <param name="time">�������Ȃ鎞��</param>
        public AudioSystem FadeInBgm(float time, float volume = 1)
        {
            m_FadeSpeed = m_AudioBgm.volume / time;
            StartCoroutine(FadeInBgm(volume));
            return this;
        }

        /// <summary>
        /// ���X�ɉ���傫�����Ă���
        /// </summary>
        /// <param name="time">�傫�����鎞��</param>
        public AudioSystem FadeOutBgm(float time, float volume = 0)
        {
            m_FadeSpeed = m_AudioBgm.volume / time;
            StartCoroutine(FadeOutBgm(volume));
            return this;
        }

        private IEnumerator FadeInBgm(float volume)
        {
            float defaultVolume = 0;
            //
            if (volume == 1) defaultVolume = m_AudioBgm.volume;
            else defaultVolume = volume;

            while(m_AudioBgm.volume > 0)
            {
                yield return null;
                m_AudioBgm.volume -= m_FadeSpeed * Time.deltaTime;
            }
            StopBgm();
            m_AudioBgm.volume = defaultVolume;
        }

        private IEnumerator FadeOutBgm(float volume)
        {
            float defaultVolume = 0;
            //default�̉��ʂ��L�����Ă���
            if (volume == 0) defaultVolume = m_AudioBgm.volume;
            else defaultVolume = volume;

            m_AudioBgm.volume = 0;
            while(m_AudioBgm.volume < defaultVolume)
            {
                yield return null;
                m_AudioBgm.volume += m_FadeSpeed * Time.deltaTime;
            }
            m_AudioBgm.volume = defaultVolume;
        }
    }

