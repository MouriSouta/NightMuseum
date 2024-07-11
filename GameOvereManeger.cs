using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOvereManeger : MonoBehaviour
{
    private void Start()
    {
        AudioSystem.instance.PlayBGM(StringDate.GameOverBGM);
    }

    public void Rispown()
    {
        AudioSystem.instance.FadeInBgm(SceneManagment.instance.FadeTime);
        SceneManagment.instance.PlayScene();
    }

    public void End()
    {
        SceneManagment.instance.TitleScene();
    }
}
