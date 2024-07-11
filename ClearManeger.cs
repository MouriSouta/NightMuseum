using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearManeger : MonoBehaviour
{
    [SerializeField]
    private GameObject ClearPanel;

    private void Start()
    {
        _ = Wait.WaitTime(3.5f, () => {
            AudioSystem.instance.PlayBGM(StringDate.ClearBGM);
            AudioSystem.instance.SetVolumeBgm(0);
        });
        _ = Wait.WaitTime(7.5f, () => {
            ClearPanel.SetActive(true);
            AudioSystem.instance.SetVolumeBgm(1);
        });
    }

    public void ChangeTitleScene()
    {
        SceneManagment.instance.TitleScene();
    }

    public void ChangegameEnd()
    {
        SceneManagment.instance.GameEnd();
    }
}
