using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    [SerializeField]
    private Image m_FadeImg;//フェードさせるための

    private float m_FadeInTime;
    private float m_FadeOutTime;
    private float m_FadeTime;

    private float m_SetAlphaValue;
    private float m_EndAlphaValue;

    [SerializeField]
    GameObject m_FadeCanvas;

    GameObject m_CurrentFadeObj;

    private bool IsUnscaled;
    private bool IsDefaultScaled = false;

    private float m_deltaTime;

    //処理が終わった時にコールバックするのに使う
    public delegate void OnCompleteDelegate();
    private OnCompleteDelegate OnComplete;

    /// <summary>
    /// 処理が終わった時に呼び出す
    /// </summary>
    /// <param name="onCompleteDelegate">呼びたい処理</param>
    public void OnCompleted(OnCompleteDelegate onCompleteDelegate) => OnComplete = onCompleteDelegate;

    public delegate void OnFadeInFadeOutComplete();
    private OnFadeInFadeOutComplete OnFadeInOutComplete;

    /// <summary>
    /// FadeOutし終わったタイミングで呼びだす
    /// </summary>
    /// <param name="onFadeInFadeOutComplete">呼びたい処理</param>
    /// <returns></returns>
    public FadeManager OnFadeInOutCompleted(OnFadeInFadeOutComplete onFadeInFadeOutComplete)
    {
        OnFadeInOutComplete = onFadeInFadeOutComplete;
        return this;
    }
    /// <summary>
    /// Fadeの状態を管理するState
    /// </summary>
    private enum State
    {
        Normal,
        Fade,
        FadeIn,
        FadeOut,
        FadeInOut,
    }
    State m_State = State.Normal;

    private void Awake()
    {
        //シングルトン　シーン遷移しても破棄されないようにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            GameObject fadeCanvas = Instantiate(m_FadeCanvas);
            DontDestroyOnLoad(fadeCanvas);
            m_FadeImg = fadeCanvas.GetComponentInChildren<Image>();
            IsDefaultScaled = false;
            IsUnscaled = IsDefaultScaled;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンが読み込まれたときに呼ばれる
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="loadSceneMode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (m_FadeImg == null)
        {
            m_FadeImg = GameObject.FindGameObjectWithTag("FadeImage").GetComponent<Image>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsUnscaled) m_deltaTime = Time.unscaledDeltaTime;
        else m_deltaTime = Time.deltaTime;

        switch (m_State)
        {
            case State.Fade: FadeUpdate(); break;
            case State.FadeIn: FadeInUpdate(); break;
            case State.FadeOut: FadeOutUpdate(); break;
            case State.FadeInOut: FadeInOutUpdate(); break;
            default: break;
        }
    }

    public FadeManager ChangeColor(Color color)
    {
        //透明度はキープ
        color = new Color(color.r, color.g, color.b, m_FadeImg.color.a);
        m_FadeImg.color = color;
        return this;
    }
    private float m_FadeAlphaValue;

    /// <summary>
    /// 一定の値から一定の値まで変化させる
    /// </summary>
    /// <param name="time"></param>
    /// <param name="startAlpha"></param>
    /// <param name="endAlpha"></param>
    /// <returns></returns>
    public FadeManager Fade(float time, float startAlpha, float endAlpha, bool isUnscaled = false)
    {
        m_FadeTime = time;
        m_State = State.Fade;
        m_EndAlphaValue = endAlpha;
        IsUnscaled = isUnscaled;

        //値を増やすのか減らすのかを判定
        m_FadeAlphaValue = startAlpha - endAlpha;
        //透明度の初期値を設定
        FadeObjAlphaSetUp(startAlpha);
        return this;
    }

    private void FadeUpdate()
    {
        if (m_FadeImg.color.a <= m_EndAlphaValue)
        {
            if (m_FadeAlphaValue < 0)
                m_FadeImg.color -= new Color(0, 0, 0, m_FadeAlphaValue * m_deltaTime / m_FadeTime);
        }
        else if (m_FadeImg.color.a >= m_EndAlphaValue)
        {
            if (m_FadeAlphaValue > 0)
                m_FadeImg.color -= new Color(0, 0, 0, m_FadeAlphaValue * m_deltaTime / m_FadeTime);
        }
        else
        {
            m_State = State.Normal;
            m_FadeImg.color = new Color(m_FadeImg.color.r, m_FadeImg.color.g, m_FadeImg.color.b, m_EndAlphaValue);
            //コールバックを呼ぶ
            if (OnComplete != null)
            {
                OnComplete.Invoke();
                OnComplete = null;
                IsUnscaled = IsDefaultScaled;
            }
        }
    }

    /// <summary>
    /// 徐々に画面を明るくする
    /// </summary>
    /// <param name="time">かかる時間</param>
    public FadeManager FadeIn(float time, bool isUnscaled = false)
    {
        m_FadeInTime = time;
        IsUnscaled = isUnscaled;
        FadeObjAlphaSetUp(1.0f);
        m_State = State.FadeIn;
        return this;
    }

    /// <summary>
    /// フェードインするUpdateで呼ばれる関数
    /// </summary>
    private void FadeInUpdate()
    {
        if (m_FadeImg.color.a >= 0)
            m_FadeImg.color -= new Color(0, 0, 0, m_deltaTime / m_FadeInTime);
        else
        {
            m_State = State.Normal;
            m_FadeImg.color = new Color(m_FadeImg.color.r, m_FadeImg.color.g, m_FadeImg.color.b, 0);
            //コールバックを呼ぶ
            if (OnComplete != null)
            {
                OnComplete.Invoke();
                OnComplete = null;
                IsUnscaled = IsDefaultScaled;
            }
        }
    }

    /// <summary>
    /// 徐々に画面を暗くする
    /// </summary>
    /// <param name="time">かかる時間</param>
    public FadeManager FadeOut(float time, bool isUnscaled = false)
    {
        m_FadeOutTime = time;
        IsUnscaled = isUnscaled;
        FadeObjAlphaSetUp(0.0f);
        m_State = State.FadeOut;
        return this;
    }

    /// <summary>
    /// フェードアウトするUpdateで呼ばれる関数
    /// </summary>
    private void FadeOutUpdate()
    {
        if (m_FadeImg.color.a <= 1)
            m_FadeImg.color += new Color(0, 0, 0, m_deltaTime / m_FadeOutTime);
        else
        {
            m_FadeImg.color = new Color(m_FadeImg.color.r, m_FadeImg.color.g, m_FadeImg.color.b, 1);
            m_State = State.Normal;
            //コールバックを呼ぶ
            if (OnComplete != null)
            {
                OnComplete.Invoke();
                OnComplete = null;
                IsUnscaled = IsDefaultScaled;
            }
        }
    }

    /// <summary>
    /// 暗くなった後にまた明るくなる
    /// </summary>
    /// <param name="fadeOutTime">フェードアウトの時間</param>
    /// <param name="fadeInTime">フェードインの時間</param>
    public FadeManager FadeInOut(float fadeOutTime, float fadeInTime, bool isUnscaled = false)
    {
        m_FadeOutTime = fadeOutTime;
        IsUnscaled = isUnscaled;
        m_FadeInTime = fadeInTime;
        FadeObjAlphaSetUp(0.0f);
        m_State = State.FadeInOut;
        return this;
    }

    /// <summary>
    /// フェードアウトした後にフェードインするUpdateで呼ばれる関数
    /// </summary>
    private void FadeInOutUpdate()
    {
        if (m_FadeImg.color.a <= 1)
            m_FadeImg.color += new Color(0, 0, 0, m_deltaTime / m_FadeOutTime);
        else
        {
            //フェードアウトし終わったら、ステートをフェードインにする
            m_FadeImg.color = new Color(m_FadeImg.color.r, m_FadeImg.color.g, m_FadeImg.color.b, 1);
            m_State = State.FadeIn;

            //フェードインフェードアウトの中間でこれを呼び出す
            if (OnFadeInOutComplete != null)
            {
                OnFadeInOutComplete.Invoke();
                OnFadeInOutComplete = null;
            }
        }
    }
    /// <summary>
    /// フェードオブジェクトのアルファの設定
    /// </summary>
    /// <param name="alpha">生成時のアルファの値</param>
    public void FadeObjAlphaSetUp(float alpha)
    {
        //値が1より大きく、0未満だったら最大、最小値に直す
        if (alpha > 1) alpha = 1;
        else if (alpha < 0) alpha = 0;

        m_SetAlphaValue = alpha;

        //オブジェクトの生成
        //透明度をFadeに応じて変更する
        m_FadeImg.color = new Color(m_FadeImg.color.r, m_FadeImg.color.g, m_FadeImg.color.b, m_SetAlphaValue);
    }

    public FadeManager SetUnScaledDeltaTime(bool isbool)
    {
        IsDefaultScaled = isbool;
        return this;
    }
}

