using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UISystem : Singleton<UISystem>
{
    [Header("HUD References")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject runInfoCanvas;
    [SerializeField] private TextMeshProUGUI time;

    [Header("Run Info Panel References")]
    [SerializeField] private TextMeshProUGUI runInfoTime;
    [SerializeField] private TextMeshProUGUI startedFromTutorial;
    [SerializeField] private TextMeshProUGUI area1;
    [SerializeField] private TextMeshProUGUI area2;
    [SerializeField] private TextMeshProUGUI area3;
    [SerializeField] private TextMeshProUGUI winRun;
    [SerializeField] private TextMeshProUGUI enemiesFought;
    [SerializeField] private TextMeshProUGUI chestsLooted;
    [SerializeField] private TextMeshProUGUI timesRested;
    [SerializeField] private TextMeshProUGUI cardsBurned;
    [SerializeField] private TextMeshProUGUI runesPlayed;
    [SerializeField] private TextMeshProUGUI mostPlayedCard;

    void Update()
    {
        // start tracking time
        GameData.PlayTime += Time.deltaTime;

        int hours = (int)(GameData.PlayTime / 3600);
        int minutes = (int)(GameData.PlayTime % 3600) / 60;
        int seconds = (int)(GameData.PlayTime % 60);

        time.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    /*
        Pause Menu
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
        Run Info
    */

    public void OnRunInfoClicked()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        runInfoCanvas.SetActive(!runInfoCanvas.activeSelf);

        if (runInfoCanvas.activeSelf)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;

        int hours = (int)(GameData.PlayTime / 3600);
        int minutes = (int)(GameData.PlayTime % 3600) / 60;
        int seconds = (int)(GameData.PlayTime % 60);
        runInfoTime.text = $"Play time: {hours:00}:{minutes:00}:{seconds:00}";

        startedFromTutorial.text = GameData.StartedFromTutorial ? "Yes" : "No";
        area1.text = GameData.Area1.ToString();
        area2.text = GameData.Area2.ToString();
        area3.text = GameData.Area3.ToString();
        winRun.text = GameData.WinRun ? "Yes" : "No";
    } 
   
    public void TransformShake(Transform objectTransform)
    {
        if (objectTransform is RectTransform rt)
        {
            rt.DOShakeAnchorPos(0.5f, new Vector2(24f, 24f), 14, 90f, false, true);
        }
        else
        {
            objectTransform.DOShakePosition(0.5f, new Vector3(0.25f, 0f, 0.25f), 10, 90f, false, true);
        }
    } 
    public void UpdateStatusEffects(StatusEffect statusEffect, int stacks, bool afflictedUnitIsPlayer)
    {
        
    }

    /*
        Inventory buttons 
        (to be implemented from Inventory.cs)
    */
}
