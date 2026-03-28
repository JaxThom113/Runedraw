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
        SceneManager.LoadScene("CharacterMenu");
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
