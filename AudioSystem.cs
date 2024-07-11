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
                //初期化
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
        ///BGMを鳴らす
        ///</summary>
        ///<param name="name">音の名前</param>
        ///<param name="loop">ループするかどうか</param>
        public AudioSystem PlayBGM(string name, bool loop = true)
        {
            //何もなし、またはNoneと入力した場合は処理しないようにする
            if (name == "" || name == "None") return this;

            m_AudioBgm.loop = loop;
            for(int i = 0; i < m_AudioDataBase.m_BGMData.Length; i++)
            {
                //TypeがBGMじゃなければ次のループにする
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
                Debug.LogError("Clipが見つかりませんでした　Name:" + name);

            return this;
        }

        ///<summary>
        ///要素番号を指定してBGMを鳴らす
        ///</summary>
        ///<param name="index">AudioDataの要素番号</param>
        ///<param name="loop">ループするかどうか</param>
        ///<returns></returns>
        public AudioSystem PlayBGM(int index, bool loop = true)
        {
            m_AudioBgm.loop = loop;

            if(m_AudioDataBase.m_BGMData.Length <= index)
            {
                Debug.LogError($"配列が超えています　要素数:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //すでに音が鳴っている場合は一度停止してから再生
            if(m_AudioBgm.isPlaying)
            {
                m_AudioBgm.Stop();
            }

            //AudioData内にあるBGMDataのClipを代入して鳴らす
            //ならない場合はエラーを出す
            m_AudioBgm.clip = m_AudioDataBase.m_BGMData[index].AudioClip;
            m_AudioBgm.Play();

            if (!m_AudioBgm.isPlaying)
                Debug.LogError("Clipが見つかりませんでした　Name:" + name);

            return this;
        }

        ///<summary>
        ///SE鳴らす
        ///</summary>
        ///<param name="name">SEの名前</param>
        ///<param name="seObjectIndex">どのプレハブを使用するか</param>
        ///<param name="pitch">音の高さの調節</param>
        ///<param name="loop">ループするかどうか</param>
        public AudioSystem PlaySE(string name, int seObjectIndex = 0,Vector3 position = new Vector3(),float volume = 1f, float pitch = 1, bool loop = false)
        {
            //何も無し、またはNoneと入力した場合は処理しないようにする
            if (name == "" || name == "None") return this;

            //これ以上AudioSourceが増えたら音を鳴らさないようにする
            if (m_MaxSeCount < m_SeCount) return this;
            //indexの値がLengthの値を超えていたらエラーを表示する
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("indexの値がオーバーしています　index:" + seObjectIndex);
                return this;
            }

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if (m_Max < pitch) pitch = m_Max;
            else if (m_Min > pitch) pitch = m_Min;
            //ピッチを変更する
            audioSourceSe.pitch = pitch;

            for(int i = 0; i < m_AudioDataBase.m_SEData.Length; i++)
            {
                //TypeがBGMじゃなければ次のループ
                if(m_AudioDataBase.m_SEData[i].Name == name)
                {
                    //LoopならPlay
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
                Debug.LogError("Clipが見つかりません　Name:" + name);

            //音が鳴りやんだら、自身についているこのAudioSourceを削除する
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
        /// 要素番号を指定してSEを鳴らす
        /// 生成すれのはAudioManeger内
        /// </summary>
        /// <param name="index">SEの要素番号</param>
        /// <param name="seObjectindex">どのプレハブを使用するか</param>
        /// <param name="pitch">音の高さ調節</param>
        /// <param name="loop">ループするかどうか</param>
        /// <returns></returns>
        public AudioSystem PlaySE(int index, int seObjectIndex = 0, Vector3 position = new Vector3(), float volume = 1f, float pitch = 1, bool loop = false)
        {
            //これ以上AudioSourceが増えたら音を鳴らさないようにする
            if (m_MaxSeCount < m_SeCount) return this;

            //indexの値がLengthの値を超えていたらエラーを表示
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("Indexの値がオーバーしています　index:" + seObjectIndex);
                return this;
            }

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if (m_Max < pitch) pitch = m_Max;
            else if (m_Min > pitch) pitch = m_Min;
            //ピッチを変更する
            audioSourceSe.pitch = pitch;

            //配列が超えたていたらエラーを表示
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($"配列が超えています　要素数:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //ループする場合Play、ループしない場合はOneShotにする
            if (loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clipが見つかりませんName:" + name);

            //音が鳴りやんだら、自身についているこのAudiioSourceを削除する
            //鳴りやんだ際にコールバックが設定されている場合はそのコールバックを処理
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
        ///指定したオブジェクトからSE鳴らす
        /// 生成するのはオブジェクト内
        ///</summary>
        ///<param name="name">SEの名前</param>
        ///<param name="parentObj">親となるオブジェクト</param>
        ///<param name="offset">オブジェクトを生成する位置に調節</param>
        ///<param name="seObjectIndex">どのプレハブを使用するか</param>
        ///<param name="pitch">音の高さの調節</param>
        ///<param name="loop">ループするかどうか</param>
        public AudioSystem PlaySEAsChild(string name, Transform parent, Vector3? offset = null, int seObjectIndex = 0, float volume = 1f, float pitch = 9999, bool loop = false)
        {
            //何の無い、またはNoneと入力した場合は処理しないようにする
            if (name == "" || name == "Name") return this;

            //これ以上AudioSourceが増えたら音を鳴らさないようにする
            if (m_MaxSeCount < m_SeCount) return this;
            //indexの値がLengthの値を超えていたらエラーを表示する
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index index:" + seObjectIndex);
                return this;
            }
            if (offset == null) offset = Vector3.zero;

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parent.position;
            audioSourceSe.transform.parent = parent;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //ピッチを変更
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            bool is_Played = false;

            for(int i = 0; i < m_AudioDataBase.m_SEData.Length; i++)
            {
                if(m_AudioDataBase.m_SEData[i].Name == name && !is_Played)
                {
                    //LoopならPlay
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

            //おtが鳴りやんだら、自身についているこのAusdioSourceを削除
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
        /// 要素番号を設定し、指定したオブジェクトからSEを鳴らす
        /// 生成するのはオブジェクト内
        /// </summary>
        /// <param name="index">SEの要素番号</param>
        /// <param name="parentObj">親となるオブジェクト</param>
        /// <param name="offset">オブジェクトを生成する位置の調整</param>
        /// <param name="seObjectIndex">どのプレハブを使用するか</param>
        /// <param name="pitch">音の高さ調整</param>
        /// <param name="loop">ループするかどうか</param>
        /// <returns></returns>
        public AudioSystem PlaySEAsChild(int index, Transform parent,Vector3?offset = null, int seObjectIndex = 0, float volume = 1, float pitch = 9999, bool loop = false)
        {
            //これ以上AudioSourceが増えたら音を鳴らさない
            if (m_MaxSeCount < m_SeCount) return this;

            //indexの値がLengthの値を超えたたらエラーを表示
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("indexの値がオーバーしている index:" + seObjectIndex);
                return this;
            }

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parent.position;
            audioSourceSe.transform.parent = parent;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //ピッチを変更
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            //配列が超えていたらエラーを表示
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($"配列が超えています　要素数:{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //ループする場合はPlay、ループしない場合はOneShotにする
            if(loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.Log("Clipが見つかりません Name:" + name);

            //音がやんだら、自身についているこのAudioSourceを削除
            //鳴りやんだ際にコールバックが設定されている場合はそのコールバックを処理
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
        /// 指定したオブジェクトからSEを鳴らす
        /// 生成するのはオブジェクト外
        /// </summary>
        /// <param name="name">SEの名前</param>
        /// <param name="parentObj">指定したオブジェクト</param>
        /// <param name="offset">オブジェクトを生成する位置の調整</param>
        /// <param name="seObjectIndex">どのプレハブを使用するか</param>
        /// <param name="pitch">音の高さの調整</param>
        /// <param name="loop">ループするか</param>
        public AudioSystem PlaySEFromObjectPosition(string name, GameObject parentObj, Vector3? offset = null, int seObjectIndex = 0, float volume = 1f, float pitch = 9999, bool loop = false)
        {
            //何もなし、またはNoneと入力した場合は処理しないようにする
            if (name == "" || name == "None") return this;

            //これ以上AudioSourceが増えたら音を鳴らさないようにする
            if (m_MaxSeCount < m_SeCount) return this;
            //indexの値がLengthの値を超えていたらエラーを表示
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("index index:" + seObjectIndex);
                return this;
            }
            if (offset == null) offset = Vector3.zero;

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parentObj.transform.position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.loop = loop;
            audioSourceSe.volume = volume;

            if(pitch != 9999)
            {
                //ピッチを変更
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            bool is_Played = false;

            for(int i = 0;i < m_AudioDataBase.m_SEData.Length; i++)
            {
                if(m_AudioDataBase.m_SEData[i].Name == name && !is_Played)
                {
                    //LoopならPlay
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

            //音が鳴りやんだら自身についているこのAudioSourceを削除する
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
        /// 要素番号を設定し、指定したオブジェクトからSEを鳴らす
        /// 生成するのはオブジェクト外
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
            //これ以上AudioSourceが増えたら音を鳴らさないようにする
            if (m_MaxSeCount < m_SeCount) return this;

            //indexの値がLengthの値を超えたていたらエラーを表示
            if(m_SePrefabs.Length < seObjectIndex)
            {
                Debug.LogError("indexの値がオーバーしています index:" + seObjectIndex);
                return this;
            }

            //AudioSourceの生成と設定
            AudioSource audioSourceSe = m_Pools[seObjectIndex].Get();
            audioSourceSe.transform.position = (Vector3)offset + parentObj.transform.position;
            audioSourceSe.outputAudioMixerGroup = m_MixerGroupSe;
            audioSourceSe.volume = volume;
            audioSourceSe.loop = loop;

            if(pitch != 9999)
            {
                //ピッチを変更
                if (m_Max < pitch) pitch = m_Max;
                else if (m_Min > pitch) pitch = m_Min;
                audioSourceSe.pitch = pitch;
            }

            //配列が超えたらエラーを表示
            if(m_AudioDataBase.m_SEData.Length <= index)
            {
                Debug.LogError($":{m_AudioDataBase.m_SEData.Length} index:{index}");
                return this;
            }

            //ループする場合はPlay、ループしない場合はOneShotにする
            if (loop)
            {
                audioSourceSe.clip = m_AudioDataBase.m_SEData[index].AudioClip;
                audioSourceSe.Play();
            }
            else audioSourceSe.PlayOneShot(m_AudioDataBase.m_SEData[index].AudioClip);

            if (!audioSourceSe.isPlaying)
                Debug.LogError("Clip Name:" + name);

            //音が鳴りやんだら、自身についているこのAudioSourceを削除
            //鳴りやんだ際にコールバックが設定されている場合はそのコールバックを処理
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
        ///音が鳴りやんだ時にコールバック
        /// </summary>
        /// <param name="audioi">自身のAudioSourceコンポーネント</param>
        /// <param name="callback">終了時に呼び出す</param>
        private IEnumerator Checking(AudioSource audio, int CoroutineNum, UnityAction callback)
        {
            yield return new WaitWhile(() => audio.isPlaying);

            callback();
            m_CheckCoroutineDic.Remove(CoroutineNum);
        }

        /// <summary>
        /// シーンが読み込まれたときに呼ばれる
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
        ///シーンが読み込まれる前に呼ばれる
        /// </summary>
        /// <param name="scene"</param>
        private void OnSceneUnLoaded(Scene scene)
        {
            //コルーチンが作動していない場合Errorになる
            foreach(Coroutine coroutine in m_CheckCoroutineDic.Values)
            {
                StopCoroutine(coroutine);
            }

            Debugger.Log("シーン読み込みます" + scene.name);
            foreach(GameObject obj in m_AudioObjectList)
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// 音を一時停止する
        /// </summary>
        public void PauseBgm()
            => m_AudioBgm.Pause();

        ///<summary>
        ///音を停止する
        /// </summary>
        public void StopBgm()
            => m_AudioBgm.Pause();

        /// <summary>
        /// 音を再開する
        /// </summary>
        public void ReStartBgm()
            => m_AudioBgm.Play();

        /// <summary>
        /// 全ての音量を調節
        /// </summary>
        /// <param name="volume">音量</param>
        public void SetAllVolume(float volume)
            => m_Mixer.SetFloat("Master", volume);

        ///<summary>
        ///音量調節
        /// </summary>
        /// <param name="volume">音量</param>
        public AudioSystem SetVolumeBgm(float volume)
        {
            m_AudioBgm.volume = volume;
            return this;
        }

        /// <summary>
        /// BGM全体の音量を変更
        /// </summary>
        /// <param name="volume">音量</param>
        public void SetVolumeBgmMixer(float volume)
            => m_Mixer.SetFloat("BGM", volume);

        /// <summary>
        /// SE全体の音量を変更する
        /// </summary>
        /// <param name="volume">音量</param>
        public void SetVolumeSeMixer(float volume)
            => m_Mixer.SetFloat("SE", volume);

        /// <summary>
        /// 徐々に音を小さくしていく
        /// </summary>
        /// <param name="time">小さくなる時間</param>
        public AudioSystem FadeInBgm(float time, float volume = 1)
        {
            m_FadeSpeed = m_AudioBgm.volume / time;
            StartCoroutine(FadeInBgm(volume));
            return this;
        }

        /// <summary>
        /// 徐々に音を大きくしていく
        /// </summary>
        /// <param name="time">大きくする時間</param>
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
            //defaultの音量を記憶しておく
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

