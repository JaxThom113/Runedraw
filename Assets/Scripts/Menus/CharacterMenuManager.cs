using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.SceneManagement;

public class CharacterMenuManager : MonoBehaviour
{
    [Header("Character Display References")]
    [SerializeField] private GameObject drawthurDisplay;
    [SerializeField] private GameObject decklanDisplay;
    [SerializeField] private GameObject shufflynnDisplay;

    [Header("Seed View")]
    [SerializeField] private GameObject seedInput;

    /*
        0 = Drawthur
        1 = Decklan
        2 = Shufflynn
    */
    private int playerIndex = 0;

    private bool validSeed = false;

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLeftArrowClicked()
    {
        AudioSystem.Instance.PlaySFX("click");

        if (playerIndex == 0)
            return;

        playerIndex--;
        UpdateCharacterDisplay();
    }

    public void OnRightArrowClicked()
    {
        AudioSystem.Instance.PlaySFX("click");

        if (playerIndex == 2)
            return;

        playerIndex++;
        UpdateCharacterDisplay();
    }

    public void OnStartGameClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.StopMusic();

        GameData.SelectedPlayer = playerIndex;

        if (GameData.IsSeededRun && !validSeed)
        {
            GameData.IsSeededRun = false;
            Debug.LogWarning("Invalid seed inputted, no longer seeded run & now generating new seed...");
        }

        // load splash, the coroutine will handle loading the overworld after
        LoadingSplash.targetScene = "Overworld";
        SceneManager.LoadScene("Splash");
    }

    public void OnTutorialClicked()  
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.StopMusic();

        GameData.StartedFromTutorial = true;
        GameData.SelectedPlayer = playerIndex;

        if (GameData.IsSeededRun && !validSeed)
        {
            GameData.IsSeededRun = false;
            Debug.LogWarning("Invalid seed inputted, no longer seeded run & now generating new seed...");
        }

        // load into overworld same as if StartGame were clicked
        LoadingSplash.targetScene = "Overworld";
        SceneManager.LoadScene("Splash");
    }

    public void OnSeededRunValueChanged()
    {
        AudioSystem.Instance.PlaySFX("click");
        seedInput.SetActive(!seedInput.activeSelf);
        GameData.IsSeededRun = !GameData.IsSeededRun;
    }

    public void OnSeedInputValueChanged()
    {
        if (int.TryParse(seedInput.GetComponent<TMP_InputField>().text, out int result))
        {
            validSeed = true;
            GameData.SelectedSeed = result;
        }
        else if (seedInput.GetComponent<TMP_InputField>().text == "Test")
        {
            // TODO: Make specific seed name load a game with custom level csvs
            validSeed = false;
        }
        else
        {
            validSeed = false;
        }
    }

    /*
        Helper functions
    */
    private void UpdateCharacterDisplay()
    {
        drawthurDisplay.SetActive(false);
        decklanDisplay.SetActive(false);
        shufflynnDisplay.SetActive(false);

        switch (playerIndex)
        {
            case 0:
                // show Drawthur
                drawthurDisplay.SetActive(true);
                break;
            case 1:
                // show Decklan
                decklanDisplay.SetActive(true);
                break;
            case 2:
                // show Shufflynn
                shufflynnDisplay.SetActive(true);
                break;
        }
    }
}
