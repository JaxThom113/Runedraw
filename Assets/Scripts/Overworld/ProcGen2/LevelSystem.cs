using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LevelSystem: Singleton<LevelSystem>
{
    [Header("Transition Screen")]
    public GameObject transitionScreen;

    [Header("HUD UI References")]
    public TextMeshProUGUI areaTitle;
    public TextMeshProUGUI areaLevel;

    [Header("Card Pickup UI References")]
    public GameObject cardPickupUI;

    [Header("Script References")]
    public ProcGen2 procGen;
    public PlayerMovement2 playerMovement;

    // current level and area
    private int currentLevel;
    private int currentArea;
    private bool skipPressed = false;
    private GameObject currentInteractable = null;

    void Start()
    {
        currentLevel = 1;
        currentArea = 1;

        UpdateUI();
    }

    /*
        Level/Area Transitions
    */

    public void NextLevel()
    {
        if (currentLevel == 5)
        {
            NextArea();

            //placeholder area transition
            StartCoroutine(LevelTransition());
            UpdateUI();
            
            return;
        }

        currentLevel++;
        Debug.Log($"Moving to level {currentLevel}");

        StartCoroutine(LevelTransition());

        UpdateUI();
    }
    
    public void NextArea()
    {
        currentArea++;
        currentLevel = 1;
        Debug.Log($"Moving to area {currentArea}");

        // load next area scene
        // SceneManager.LoadScene($"Level{currentLevel}");

        StartCoroutine(AreaTransition());

        UpdateUI();
    }

    void UpdateUI()
    {
        if (areaTitle != null)
        {
            // names for different areas
            switch (currentArea)
            {
                case 1: areaTitle.text = $"Dungeons"; break;
                case 2: areaTitle.text = $"Forest"; break;
                case 3: areaTitle.text = $"Tundra"; break;
            }
        }

        if (areaLevel != null)
        {
            // (area) - (level) numbers
            areaLevel.text = $"{currentArea} - {currentLevel}";
        }
    }

    IEnumerator LevelTransition()
    {
        // take control from player, have player continue moving upward
        playerMovement.enabled = false;
        playerMovement.ContinueUp();
        //SceneTransitionSystem.Instance.enemyData = OverworldSystem.Instance.GetCurrentEnemy();

        // transition swipe effect
        transitionScreen.SetActive(true);
        transitionScreen.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);

        // generate new level
        procGen.GenerateLevel();
        playerMovement.ResetMovePoint();

        // transition swipe out and reset position
        transitionScreen.transform.DOMoveY(40, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);
        transitionScreen.SetActive(false);
        transitionScreen.transform.position = new Vector3(0, -40, 0);

        // tp player to bottom of the screen, have player move upward to starting cell
        playerMovement.TeleportToBottom();
        playerMovement.ContinueUp();

        // return control to the player  
        playerMovement.enabled = true;
        Debug.Log("Level transition complete");

        yield return null;
    }

    IEnumerator AreaTransition()
    {
        Debug.Log($"Will work on this coroutine later!");

        yield return null;
    }

    /*
        Enemies
    */

    public void GoToBattleScreen()
    {
        // transition to battle screen
        StartCoroutine(BattleScreenTransition());
    }

    IEnumerator BattleScreenTransition()
    {
        // take control from player, have player continue moving upward
        playerMovement.enabled = false;

        // have a camera zoom in on the enemy/player collision

        // transition swipe effect
        transitionScreen.SetActive(true);
        transitionScreen.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);

        // generate new level
        SceneManager.LoadScene("BattleScene");

        // transition swipe out and reset position
        transitionScreen.transform.DOMoveY(40, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);
        transitionScreen.SetActive(false);
        transitionScreen.transform.position = new Vector3(0, -40, 0);

        // return control to the player  
        playerMovement.enabled = true;

        Debug.Log("Battle scene transition complete");

        yield return null;
    }

    /*
        Interactables
    */

    public void LootBox(GameObject interactable)
    {
        // store the interactable instance for later deletion
        currentInteractable = interactable;

        // display card pickup UI, start card pickup coroutine
        cardPickupUI.SetActive(true);

        StartCoroutine(LootBoxActivate());
    }

    public void OnSkipButtonClick()
    {
        // skip button clicked, disable UI and delete the interactable
        cardPickupUI.SetActive(false);
        
        if (currentInteractable != null)
        {
            Destroy(currentInteractable);
            currentInteractable = null;
        }

        skipPressed = true;
    }

    IEnumerator LootBoxActivate()
    {
        // given the current level and area, allow player to pick a card
        // weaker cards appear at earlier levels/areas, stronger cards appear later
        skipPressed = false;
        
        Debug.Log("Loot Box Activated!");

        yield return new WaitUntil(() => skipPressed);
        cardPickupUI.SetActive(false);
    }
}