using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("UI Element References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle windowedToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Counter References")]
    [SerializeField] private TextMeshProUGUI windowedText;
    [SerializeField] private TextMeshProUGUI vsyncText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    private Resolution[] resolutions;
    private int resolutionIndex;
    private bool isWindowedOn;
    private bool isVsyncOn;
    private float musicVolume;
    private float sfxVolume;

    void Start()
    {
        SetupResolutions();
        LoadSettings();
    }

    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        resolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                resolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void LoadSettings()
    {
        windowedToggle.isOn = PlayerPrefs.GetInt("Windowed", 0) == 1;
        vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 0) == 1;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("Windowed", windowedToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);
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

    public void OnResolutionValueChanged(int index)
    {
        resolutionIndex = index;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void OnWindowedValueChanged()
    {
        AudioSystem.Instance.PlaySFX("click");
        isWindowedOn = !isWindowedOn;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
        windowedText.text = $"{(isWindowedOn ? "On" : "Off")}";
    }

    public void OnVSyncValueChanged()
    {
        AudioSystem.Instance.PlaySFX("click");
        isVsyncOn = !isVsyncOn;
        QualitySettings.vSyncCount = isVsyncOn ? 1 : 0;
        vsyncText.text = $"{(isVsyncOn ? "On" : "Off")}";
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
