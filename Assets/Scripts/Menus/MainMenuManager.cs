using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        AudioSystem.Instance.PlayMusic("menu");
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
