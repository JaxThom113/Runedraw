using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using UnityEngine.SceneManagement;

public class CharacterMenuManager : MonoBehaviour
{
    [Header("Character Display References")]
    [SerializeField] private GameObject drawthurDisplay;
    [SerializeField] private GameObject decklanDisplay;
    [SerializeField] private GameObject shufflynnDisplay;

    [SerializeField] private AudioSource clickSound;

    /*
        0 = Drawthur
        1 = Decklan
        2 = Shufflynn
    */
    private int playerIndex = 0;

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLeftArrowClicked()
    {
        clickSound.Play();

        if (playerIndex == 0)
            return;

        playerIndex--;
        UpdateCharacterDisplay();
    }

    public void OnRightArrowClicked()
    {
        clickSound.Play();

        if (playerIndex == 2)
            return;

        playerIndex++;
        UpdateCharacterDisplay();
    }

    public void OnStartGameClicked()
    {
        clickSound.Play();

        GameData.SelectedPlayer = playerIndex;

        // load splash, the coroutine will handle loading the overworld after
        LoadingSplash.targetScene = "NewJaxOverworld";
        SceneManager.LoadScene("Splash");
    }

    public void OnTutorialClicked()  
    {
        clickSound.Play();

        GameData.SelectedPlayer = playerIndex;

        // start starting area type to 0 for the tutorial level
        GameData.SelectedAreaType = 0;

        // load into overworld same as if StartGame were clicked
        LoadingSplash.targetScene = "NewJaxOverworld";
        SceneManager.LoadScene("Splash");
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
