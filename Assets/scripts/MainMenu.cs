using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        LevelManager.Instance.LoadScene("ClubScene_UI","CrossFade");
    }

    public void Settings()
    {
        LevelManager.Instance.LoadScene("Game", "CrossFade");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
