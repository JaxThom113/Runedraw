using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSystem: Singleton<LevelSystem>
{
    [Header("Debug")]
    public bool enemies = true;
    public bool interactables = true;

    [Header("Areas")]
    public GameObject earthArea;
    public GameObject fireArea;
    public GameObject neutralArea;
    public GameObject waterArea;
    public GameObject windArea;

    [Header("Custom Levels")]
    public GameObject tutorialLevel;    
    public GameObject finalBossLevel;

    [Header("UI References")]
    public TextMeshProUGUI areaTitle;
    public TextMeshProUGUI areaLevel;
    public GameObject transitionScreen;
    public GameObject LootView;

    [Header("Script References")]
    public PlayerMovement playerMovement;

    // current level and area
    private int currentLevel = 3;
    private int currentArea = 3;

    /*
        Current area type
            0 = Tutorial level
            1 = neutral
            2 = fire
            3 = wind
            4 = water
            5 = earth
            6 = FinalBoss level
    */
    public int currentAreaType;

    // loot view variables
    private bool skipPressed = false;
    private GameObject currentInteractable = null;

    private CreateLevel createLevel;

    void Start()
    {
        int seed = 0;
        Random.InitState(seed);

        // set the active area GameObject in --- Overworld ---
        currentAreaType = GameData.SelectedAreaType;
        SetActiveArea(currentAreaType);

        // get reference to the CreateLevel script in the currently active area GameObject (in Board)
        createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);

        // go to tutorial if it was selected on main menu
        if (currentAreaType == 0)
        {
            TextAsset lvlFile = Resources.Load<TextAsset>("Levels/Tutorial");
            createLevel.DrawLevel(lvlFile);
        }

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
                case 0: areaTitle.text = $"Tutorial"; break; // tutorial level
                case 1: areaTitle.text = $"Dungeons"; break; // neutral area
                case 2: areaTitle.text = $"Fire Zone"; break; // fire area
                case 3: areaTitle.text = $"Wind Zone"; break; // wind area
                case 4: areaTitle.text = $"Water Zone"; break; // water area
                case 5: areaTitle.text = $"Earth Zone"; break; // earth area
                case 6: areaTitle.text = $"Final Boss"; break; // final boss level
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
        if (currentAreaType == 0)
        {
            currentAreaType = 1;

            // start the actual game once completing tutorial level
            StartCoroutine(StartTransition(true));
        }
        else if (currentLevel == 3)
        {
            if (currentArea == 3)
            {
                // transition to the final boss level 
                currentAreaType = 6;

                TextAsset lvlFile = Resources.Load<TextAsset>("Levels/FinalBoss");
                StartCoroutine(StartTransition(true, lvlFile));
            }
            else
            {
                currentArea++;
                currentLevel = 1;

                // randomly select area type for next area
                currentAreaType = Random.Range(1, 6); 

                StartCoroutine(StartTransition(true));
            }

        }      
        else
        {
            // got to the next level of the current area
            currentLevel++;

            StartCoroutine(StartTransition());
        }

        UpdateUI();
    }

    IEnumerator StartTransition(bool areaTransition = false, TextAsset file = null)
    {
        // take control from player, have player continue moving upward
        playerMovement.enabled = false;

        // transition swipe effect
        transitionScreen.SetActive(true);
        transitionScreen.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);

        if (areaTransition)
        {
            SetActiveArea(currentAreaType);
        
            createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);
        }

        if (file != null)
        {
            // transition to a custome level from a file
            createLevel.DrawLevel(file);
        }
        else
        {
            // transition to next level in the current area
            createLevel.DrawLevel();
        }
        
        playerMovement.ResetMovePoint();

        // transition swipe out and reset position
        transitionScreen.transform.DOMoveY(50, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);
        transitionScreen.SetActive(false);
        transitionScreen.transform.position = new Vector3(0, -50, -5);

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

    /*
        Helper functions
    */

    private void SetActiveArea(int areaIndex)
    {
        tutorialLevel.SetActive(false);
        neutralArea.SetActive(false); 
        fireArea.SetActive(false); 
        windArea.SetActive(false); 
        waterArea.SetActive(false);
        earthArea.SetActive(false);
        finalBossLevel.SetActive(false);

        // transition to level 1 of the next area
        switch (areaIndex)
        {
            case 0: tutorialLevel.SetActive(true); break;
            case 1: neutralArea.SetActive(true); break;
            case 2: fireArea.SetActive(true); break;
            case 3: windArea.SetActive(true); break;
            case 4: waterArea.SetActive(true); break;
            case 5: earthArea.SetActive(true); break;
            case 6: finalBossLevel.SetActive(true); break;
        }

        
    }

    public void SetCurrentAreaType(int areaIndex)
    {
        currentAreaType = areaIndex;
    }
}