using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private AudioSource clickSound;

    public void OnStartGameClicked()
    {
        clickSound.Play();

        // load splash, the coroutine will handle loading the overworld after
        LoadingSplash.targetScene = "Overworld";
        SceneManager.LoadScene("Splash");
    }

    public void OnOptionsClicked()  
    {
        clickSound.Play();
        SceneManager.LoadScene("OptionsMenu");
    }

    public void OnCreditsClicked()
    {
        clickSound.Play();
        SceneManager.LoadScene("CreditsMenu");
    }

    public void OnQuitClicked()
    {
        clickSound.Play();

        // quit the game
        Application.Quit();
    }
}
