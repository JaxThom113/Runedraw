using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSystem: Singleton<LevelSystem>
{
    [Header("Areas")]
    public GameObject earthArea;
    public GameObject fireArea;
    public GameObject neutralArea;
    public GameObject waterArea;
    public GameObject windArea;

    [Header("UI References")]
    public TextMeshProUGUI areaTitle;
    public TextMeshProUGUI areaLevel;
    public GameObject transitionScreen;
    public GameObject LootView;

    [Header("Script References")]
    public PlayerMovement playerMovement;

    // current level and area
    private int currentLevel = 1;
    private int currentArea = 1;
    private int currentAreaType = 1; // 1 = neutral, 2 = fire, 3 = wind, 4 = water, 5 = earth

    // loot view variables
    private bool skipPressed = false;
    private GameObject currentInteractable = null;

    private CreateLevel createLevel;

    void Start()
    {
        int seed = 0;
        Random.InitState(seed);

        createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);

        UpdateUI();
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<LootCardGA>(LootBoxPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<LootCardGA>();
    }

    void UpdateUI()
    {
        if (areaTitle != null)
        {
            // names for different areas
            switch (currentAreaType)
            {
                case 1: areaTitle.text = $"Dungeons"; break; // neutral area
                case 2: areaTitle.text = $"Fire Zone"; break; // fire area
                case 3: areaTitle.text = $"Wind Zone"; break; // wind area
                case 4: areaTitle.text = $"Water Zone"; break; // water area
                case 5: areaTitle.text = $"Earth Zone"; break; // earth area
            }
        }

        if (areaLevel != null)
        {
            // (area) - (level) numbers
            areaLevel.text = $"{currentArea} - {currentLevel}";
        }
    }

    /*
        Level transition/coroutine
    */

    public void NextLevel()
    {
        if (currentLevel == 3)
        {
            currentArea++;
            currentLevel = 1;

            // randomly select area type for next area
            currentAreaType = Random.Range(1, 6); 

            StartCoroutine(StartTransition(true));
        }
        else
        {
            currentLevel++;

            StartCoroutine(StartTransition());
        }

        UpdateUI();
    }

    IEnumerator StartTransition(bool areaTransition = false)
    {
        // take control from player, have player continue moving upward
        playerMovement.enabled = false;

        // transition swipe effect
        transitionScreen.SetActive(true);
        transitionScreen.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);

        if (areaTransition)
        {
            // transition to level 1 of the next area
            switch (currentAreaType)
            {
                case 1: 
                    neutralArea.SetActive(true); 
                    fireArea.SetActive(false); 
                    windArea.SetActive(false); 
                    waterArea.SetActive(false);
                    earthArea.SetActive(false);
                    break;
                case 2: 
                    neutralArea.SetActive(false); 
                    fireArea.SetActive(true); 
                    windArea.SetActive(false); 
                    waterArea.SetActive(false);
                    earthArea.SetActive(false);
                    break;
                case 3: 
                    neutralArea.SetActive(false); 
                    fireArea.SetActive(false); 
                    windArea.SetActive(true); 
                    waterArea.SetActive(false);
                    earthArea.SetActive(false);
                    break;
                case 4: 
                    neutralArea.SetActive(false); 
                    fireArea.SetActive(false); 
                    windArea.SetActive(false); 
                    waterArea.SetActive(true);
                    earthArea.SetActive(false);
                    break;
                case 5: 
                    neutralArea.SetActive(false); 
                    fireArea.SetActive(false); 
                    windArea.SetActive(false); 
                    waterArea.SetActive(false);
                    earthArea.SetActive(true);
                    break;
            }
        
            createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);
        }

        // transition to next level in the current area
        ProcGen.GenerateLevel();
        createLevel.DrawLevel();
        playerMovement.ResetMovePoint();

        // transition swipe out and reset position
        transitionScreen.transform.DOMoveY(40, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);
        transitionScreen.SetActive(false);
        transitionScreen.transform.position = new Vector3(0, -40, 0);

        // return control to the player  
        playerMovement.enabled = true;

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

        yield return null;
    }

    /*
        Interactables
    */

    public IEnumerator LootBoxPerformer(LootCardGA lootCardGA)
    {
        // store the interactable instance for later deletion
        //currentInteractable = interactable;

        // display card pickup UI, start card pickup coroutine 
        LootView.SetActive(true);
        SoundEffectSystem.Instance.PlayVictoryTheme();

        yield return null;
    }

    public void OnSkipButtonClick()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        // skip button clicked, disable UI and delete the interactable
        LootView.SetActive(false);
        
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

        yield return new WaitUntil(() => skipPressed);
        LootView.SetActive(false);
    }
}