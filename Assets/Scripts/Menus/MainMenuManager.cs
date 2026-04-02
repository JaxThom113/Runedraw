using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        AudioSystem.Instance.PlayMusic("menu");

        // reset data when main menu is loaded
        // Pause > Back to Menu and the Death both take you back here
        GameData.InitializeData();
    }

    public void OnStartGameClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        SceneManager.LoadScene("CharacterMenu");
    }

    public void OnOptionsClicked()  
    {
        AudioSystem.Instance.PlaySFX("click");
        SceneManager.LoadScene("OptionsMenu");
    }

    public void OnCreditsClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        SceneManager.LoadScene("CreditsMenu");
    }

    public void OnQuitClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        Application.Quit();
    }
}
