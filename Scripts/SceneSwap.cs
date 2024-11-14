using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwap : MonoBehaviour
{
    public static int MainMenuScene = 0;
    public static int GameplayScene = 1;
    public static int WinScene = 2;
    public static int LoseScene = 3;
    public static int CreditsScene = 4;

    public void PlayButton()
    {

        SceneManager.LoadScene(GameplayScene);
    }

    public void BackToMain()
    {
        ActivateMouse();
        SceneManager.LoadScene(MainMenuScene);
    }

    public void WinScreen()
    {
        ActivateMouse();
        SceneManager.LoadScene(WinScene);
    }

    public void LoseScreen()
    {
        ActivateMouse();
        SceneManager.LoadScene(LoseScene);
    }

    public void CreditScene()
    {
        ActivateMouse();
        SceneManager.LoadScene(CreditsScene);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    void ActivateMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

