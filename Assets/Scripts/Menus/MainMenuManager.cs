using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnStartGameClicked()
    {
        // load splash, the coroutine will handle loading the overworld after
        SceneManager.LoadScene("Splash");
    }

    public void OnOptionsClicked()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void OnCreditsClicked()
    {
        SceneManager.LoadScene("CreditsMenu");
    }

    public void OnQuitClicked()
    {
        // quit the game
        Application.Quit();
    }
}
