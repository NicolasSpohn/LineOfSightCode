using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManagerScript : Singleton<GameStateManagerScript>
{
    public PlayerScript player;
    public StatueEnemyScript enemy;
    public GameObject pauseMenuObject;
    private InputControls input;
    public Slider staminaSlider;

    bool isPaused;
    Animator[] animators;

    private void Start()
    {
        animators = FindObjectsOfType<Animator>();
        input = new InputControls();
    }

    private void Update()
    {
        if(isPaused && input.Main.Pause.WasPressedThisFrame()) TogglePause();
        staminaSlider.value = player.playerStamina / player.maxStamina;
    }


    public void TogglePause()
    {
        isPaused = !isPaused;
        player.SetPause(isPaused);
        player.enabled = !isPaused;
        if (enemy.isActiveAndEnabled) enemy.SetPause(isPaused);
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].enabled = !isPaused;
        }
        pauseMenuObject.SetActive(isPaused);

        if(isPaused) input.Enable(); else input.Disable();

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene(SceneSwap.MainMenuScene);
    }

}
