using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("UI Element References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Counter References")]
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    private float musicVolume;
    private float sfxVolume;

    void Start()
    {
        LoadSettings();
    }

    void LoadSettings()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        PlayerPrefs.Save();
    }

    /*
        UI element events
    */

    public void OnBackClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        SaveSettings();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnMusicVolumeValueChanged()
    {
        musicVolume =  musicSlider.value / 100;
        musicVolumeText.text = $"{musicVolume * 100}";
        AudioSystem.Instance.SetMusicVolume(musicVolume);
    }

    public void OnSfxVolumeValueChanged()
    {
        sfxVolume = sfxSlider.value / 100;
        sfxVolumeText.text = $"{sfxVolume * 100}";
        AudioSystem.Instance.SetSFXVolume(sfxVolume);
    }
}
