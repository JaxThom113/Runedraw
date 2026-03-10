using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class UISystem : Singleton<UISystem>
{
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject runInfoCanvas;

    /*
        Pause Menu buttons
    */

    public void OnPauseClicked()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnResumeClicked()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnBackToMenuClicked()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1;

        LoadingSplash.targetScene = "MainMenu";
        SceneManager.LoadScene("Splash");
    }

    /*
        Run Info buttons
    */

    public void OnRunInfoClicked()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        runInfoCanvas.SetActive(!runInfoCanvas.activeSelf);

        if (runInfoCanvas.activeSelf)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    /*
        Inventory buttons 
        (to be implemented from Inventory.cs)
    */
}
