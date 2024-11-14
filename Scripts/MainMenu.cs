using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene(SceneSwap.GameplayScene);
    }

    public void BackToMain()
    {
        SceneManager.LoadScene(SceneSwap.MainMenuScene);
    }

    public void CreditScene()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}