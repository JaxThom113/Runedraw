using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        AudioSystem.Instance.PlayMusic("menu");

        // load saved audio settings
        AudioSystem.Instance.SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.5f) / 100);
        AudioSystem.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 0.5f) / 100);

        // reset data when main menu is loaded
        // Pause > Back to Menu and the Death both take you back here
        GameData.InitializeData();

        // Clear any ActionSystem performers/subscribers that were registered
        // in the previous gameplay scene. Their static dictionaries survive
        // scene loads, so without this the next playthrough inherits stale
        // callbacks pointing at destroyed MonoBehaviours.
        ActionSystem.ResetAll();
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
