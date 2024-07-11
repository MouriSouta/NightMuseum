using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagment : MonoBehaviour
{
    public static SceneManagment instance;

    private AsyncOperation m_AsyncOperation;

    [SerializeField]
    private GameObject Loading;

    [SerializeField]
    private float m_FadeTime = 0.5f;
    public float FadeTime => m_FadeTime;

    private void Awake()
    {
        if (instance == null)
        {
            FadeManager.instance.FadeIn(m_FadeTime);
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


    }

    private void Update()
    {
        //強制終了ボタン
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKey(KeyCode.JoystickButton6) && Input.GetKey(KeyCode.JoystickButton7))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }

    public void TitleScene()
    {
#if !UNITY_EDITOR
        Cursor.visible = true;
#endif
        SceneManager.LoadScene("Title");
    }

    public void PlayScene()
    {
        //カーソル表示、非表示
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif

        //Loading.SetActive(true);
        FadeManager.instance.FadeOut(m_FadeTime).OnCompleted(() =>
        {
            Loading.SetActive(true);
            m_AsyncOperation = SceneManager.LoadSceneAsync("Stage");
            m_AsyncOperation.allowSceneActivation = false;
            //非同期表示
            StartCoroutine(WaitOperation());
        });

    }

    //Loadingを軽くするための処理
    private IEnumerator WaitOperation()
    {
        while (m_AsyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        m_AsyncOperation.allowSceneActivation = true;
        Loading.SetActive(false);
    }

    public void ClearScene()
    {
#if !UNITY_EDITOR
        Cursor.visible = true;
#endif
        SceneManager.LoadScene("ClearScene");
    }

    public void GameOverScene()
    {
#if !UNITY_EDITOR
        Cursor.visible = true;
#endif
        SceneManager.LoadScene("GameOver");
    }

    public void GameEnd()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
