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
    [SerializeField] private TextMeshProUGUI runInfoSeed;
    [SerializeField] private TextMeshProUGUI startedFromTutorial;
    [SerializeField] private TextMeshProUGUI area1;
    [SerializeField] private TextMeshProUGUI area2;
    [SerializeField] private TextMeshProUGUI area3;
    [SerializeField] private TextMeshProUGUI winRun;
    [SerializeField] private TextMeshProUGUI enemiesFought;
    [SerializeField] private TextMeshProUGUI chestsLooted;
    [SerializeField] private TextMeshProUGUI timesRested;
    [SerializeField] private TextMeshProUGUI runesPlayed;

    void OnEnable()
    {
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPostReaction, ReactionTiming.POST);
    }

    private void StartRoundPostReaction(StartRoundGA startRoundGA)
    {
        // Resets any position drift accumulated from overlapping DOShakePosition
        // calls in TransformShake (e.g. ShaderSystem's per-hit shake on the
        // OverworldEnemy) so the enemy always starts a fresh round at origin.
        if (EnemySystem.Instance != null && EnemySystem.Instance.overworldEnemy != null)
        {
            EnemySystem.Instance.overworldEnemy.ResetPositionToBase();
        }
    }

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
        AudioSystem.Instance.PlaySFX("click");
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnResumeClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnBackToMenuClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1;

        // Wipe static ActionSystem bookkeeping so no performers/subscribers
        // from this (about-to-be-destroyed) scene survive into the next
        // gameplay session. Without this, stale callbacks targeting destroyed
        // MonoBehaviours accumulate and throw MissingReferenceException on
        // the second play-through.
        ActionSystem.ResetAll();

        LoadingSplash.targetScene = "MainMenu";
        SceneManager.LoadScene("Splash");
    }

    /*
        Run Info
    */

    public void OnRunInfoClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        runInfoCanvas.SetActive(!runInfoCanvas.activeSelf);

        if (runInfoCanvas.activeSelf)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;

        int hours = (int)(GameData.PlayTime / 3600);
        int minutes = (int)(GameData.PlayTime % 3600) / 60;
        int seconds = (int)(GameData.PlayTime % 60);
        runInfoTime.text = $"Play time: {hours:00}:{minutes:00}:{seconds:00}";
        
        if (GameData.SpecialSeed != null)
            runInfoSeed.text = $"Seed: {GameData.SpecialSeed}";
        else
            runInfoSeed.text = $"Seed: {GameData.SelectedSeed}";

        startedFromTutorial.text = GameData.StartedFromTutorial ? "Yes" : "No";
        area1.text = GameData.Area1.ToString();
        area2.text = GameData.Area2.ToString();
        area3.text = GameData.Area3.ToString();
        winRun.text = GameData.WinRun ? "Yes" : "No";

        enemiesFought.text = GameData.EnemiesFought.ToString();
        chestsLooted.text = GameData.ChestsLooted.ToString();
        timesRested.text = GameData.TimesRested.ToString();
        runesPlayed.text = GameData.RunesPlayed.ToString();
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
