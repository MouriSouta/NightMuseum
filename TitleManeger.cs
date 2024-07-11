using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManeger : MonoBehaviour
{
    [SerializeField]
    private GameObject TitlePanel;
    [SerializeField]
    private GameObject OptionPanel;

    private IEnumerator enumerator()
    {
        ResourceRequest[] requests = TextureLoad.LoadAllResourcesAsync<Texture2D>("Textures");
        Debug.Log(requests.Length);
        foreach (ResourceRequest request in requests)
        {
            while (request.progress < 0.9f)
            {
                yield return null;
                Debug.Log(request.asset.name + request.progress);
            }
            //Debug.Log(request.asset.name + request.p);
            //yield return request;
        }
        Debug.Log("I—¹");
    }

    private void Awake()
    {
        StartCoroutine(enumerator());
    }

    private void Start()
    {
        AudioSystem.instance.PlayBGM(StringDate.TitleBGM);
    }

    public void ChangePlayeScene()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.Decision, gameObject);
        AudioSystem.instance.FadeInBgm(SceneManagment.instance.FadeTime);
        SceneManagment.instance.PlayScene();
    }

    public void ChangeOption()
    {
        TitlePanel.SetActive(false);
        OptionPanel.SetActive(true);
    }

    public void ChangeTitle()
    {
        TitlePanel.SetActive(true);
        OptionPanel.SetActive(false);
    }
}