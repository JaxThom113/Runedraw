using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using TMPro;

public class LevelSystem: Singleton<LevelSystem>
{
    [Header("Debug")]
    public bool enemies = true;
    public bool interactables = true; 
    public bool chooseLevel = true; 
    public int level = 0; 
    /// <summary>If true, clearing one floor in an area advances to the next area (same pacing as clearing floor 3). If false, play three floors per area.</summary>
    public bool isConvergence = true;

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
    public GameObject campfireView;

    [Header("Area Intros")]
    public GameObject areaIntros;
    public CanvasGroup areaIntrosCanvasGroup;
    public GameObject tutorialAreaIntro;
    public GameObject finalBossAreaIntro;
    public GameObject earthAreaIntro;
    public GameObject fireAreaIntro;
    public GameObject neutralAreaIntro;
    public GameObject waterAreaIntro;
    public GameObject windAreaIntro;
    public TextMeshProUGUI earthAreaNumber;
    public TextMeshProUGUI fireAreaNumber;
    public TextMeshProUGUI neutralAreaNumber;
    public TextMeshProUGUI waterAreaNumber;
    public TextMeshProUGUI windAreaNumber;

    [Header("Script References")]
    public PlayerMovement playerMovement; 



    // current level and area
    private int currentLevel = 1;
    public int CurrentLevel => currentLevel;
    public int currentArea = 1;

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
    private int currentAreaType;
    public int CurrentAreaType => currentAreaType;


    [SerializeField] private List<int> overworldMap = new List<int>();

    // interactable view variables
    public bool LootSelectionCompleted { get; private set; } = false;
    public bool CampfireInteractCompleted { get; private set; } = false;

    private CreateLevel createLevel;

    void Start()
    { 
        if(chooseLevel) { 
            currentAreaType = level;   
            currentArea = level;
        }
        if (GameData.IsSeededRun)
        {
            // get the seed that was selected on the menu
            UnityEngine.Random.InitState(GameData.SelectedSeed);
        }
        else
        {
            // generate a random seed from current TickCount and save it
            GameData.SelectedSeed = Environment.TickCount;
            UnityEngine.Random.InitState(GameData.SelectedSeed);
        }

        // set the active area GameObject in ---- Overworld ----
        // this could be tutorial or a random level, depending on what button was clicked in CharacterMenu 
        
        if (!GameData.StartedFromTutorial)
        { 
            if(chooseLevel) { 
                currentAreaType = level;   
                currentArea = level; 
                GameData.SelectedAreaType = level; 
                GameData.Area1 = GameData.SelectedAreaType; 
            } 
            else{ 
                GameData.SelectedAreaType = UnityEngine.Random.Range(1, 6);
                GameData.Area1 = GameData.SelectedAreaType; 
            }
            
        }
        currentAreaType = GameData.SelectedAreaType;

        if (chooseLevel && level == 6)
        {
            GameData.SelectedAreaType = 6;
            GameData.Area1 = 6;
            currentAreaType = 6;
            currentArea = 3;
            currentLevel = 3;
        }

        overworldMap.Clear();
        overworldMap.AddRange(new[] { 1, 2, 3, 4, 5 });
        for (int i = 4; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (overworldMap[i], overworldMap[j]) = (overworldMap[j], overworldMap[i]);
        }
        if (currentAreaType >= 1 && currentAreaType <= 5)
            overworldMap.Remove(currentAreaType);

        SetActiveArea();
        SetActiveEnemyBank();
        // get reference to the CreateLevel script in the currently active area GameObject (in Board)
        createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);

        // go to tutorial if it was selected on main menu
        if (currentAreaType == 0)
        {
            TextAsset lvlFile = Resources.Load<TextAsset>("Levels/Tutorial");
            createLevel.DrawLevel(lvlFile);
        }
        else if (chooseLevel && level == 6)
        {
            TextAsset finalBossFile = Resources.Load<TextAsset>("Levels/FinalBoss");
            
        }


        StartCoroutine(ShowAreaIntro(currentAreaType));

        UpdateUI();

        // Emit one startup area event so reactive systems (fog/shader) sync the initial area.
        if (ActionSystem.Instance != null)
            ActionSystem.Instance.Perform(new NextAreaGA(currentLevel, applyLevelTransition: false));
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<LootCardGA>(LootBoxPerformer);
        ActionSystem.AttachPerformer<CampfireGA>(CampfirePerformer);
        ActionSystem.AttachPerformer<NextAreaGA>(NextAreaPerformer);
        ActionSystem.SubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<LootCardGA>();
        ActionSystem.DetachPerformer<CampfireGA>();
        ActionSystem.DetachPerformer<NextAreaGA>();
        ActionSystem.UnsubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
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
                case 2: areaTitle.text = $"The Hot Spot"; break; // fire area
                case 3: areaTitle.text = $"The Breezeway"; break; // wind area
                case 4: areaTitle.text = $"The Splash Zone"; break; // water area
                case 5: areaTitle.text = $"The Sandbox"; break; // earth area
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

    public void NextLevel(int requestedNextLevel = -1)
    { 
        if (requestedNextLevel > 0)
            currentLevel = requestedNextLevel - 1;

        if (currentAreaType == 0)
        {
            currentAreaType = 1;
            overworldMap.Remove(1);

            // start the actual game once completing tutorial level
            StartCoroutine(StartTransition(true));
        }
        // Area "segment" complete: either finished floor 3, or Convergence mode (one floor per area then move on).
        else if (currentLevel == 3 || (isConvergence))
        {
            // Final boss only after the third overworld area — do NOT key this on isConvergence (that skipped areas incorrectly).
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

                if (overworldMap.Count == 0)
                {
                    Debug.LogError("LevelSystem: overworld map exhausted (no area types left).");
                    currentAreaType = 1;
                }
                else
                {
                    currentAreaType = overworldMap[0];
                    overworldMap.RemoveAt(0);
                }

                if (currentArea == 2)
                    GameData.Area2 = currentAreaType;
                else if (currentArea == 3)
                    GameData.Area3 = currentAreaType;

                StartCoroutine(StartTransition(true));
            }
        }      
        else
        {
            // go to the next level of the current area
            currentLevel++;

            StartCoroutine(StartTransition());
        }
        
        UpdateUI();
    }

    IEnumerator StartTransition(bool areaTransition = false, TextAsset file = null)
    {
        // take control from player
        playerMovement.enabled = false;

        // transition swipe effect
        transitionScreen.SetActive(true);
        transitionScreen.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);

        if (areaTransition)
        {
            SetActiveArea();
            SetActiveEnemyBank();
            createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Exclude);
        }

        // Same-area transitions never refreshed createLevel; inactive boards also fail Exclude searches.
        if (createLevel == null)
            createLevel = FindFirstObjectByType<CreateLevel>(FindObjectsInactive.Include);
        if (createLevel == null)
        {
            Debug.LogError("LevelSystem: CreateLevel not found in scene — assign a CreateLevel or ensure the active area's board is enabled.");
        }
        else if (file != null)
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

        // show the intro for the next area if the player made it to a new area
        if (areaTransition)
            StartCoroutine(ShowAreaIntro(currentAreaType));

        // transition swipe out and reset position
        transitionScreen.transform.DOMoveY(50, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1f);
        transitionScreen.SetActive(false);
        transitionScreen.transform.position = new Vector3(0, -50, -5);

        // return control to the player  
        playerMovement.enabled = true;

        yield return null;
    }

    IEnumerator ShowAreaIntro(int areaType)
    {
        // take control from player
        playerMovement.enabled = false;

        areaIntros.SetActive(true);

        switch (areaType)
        {
            case 0: 
                tutorialAreaIntro.SetActive(true); 
                break;
            case 1: 
                neutralAreaIntro.SetActive(true); 
                neutralAreaNumber.text = $"Area #{currentArea}";
                break;
            case 2: 
                fireAreaIntro.SetActive(true);
                fireAreaNumber.text = $"Area #{currentArea}";
                break;
            case 3: 
                windAreaIntro.SetActive(true); 
                windAreaNumber.text = $"Area #{currentArea}";
                break;
            case 4: 
                waterAreaIntro.SetActive(true); 
                waterAreaNumber.text = $"Area #{currentArea}";
                break;
            case 5: 
                earthAreaIntro.SetActive(true); 
                earthAreaNumber.text = $"Area #{currentArea}";
                break;
            case 6: 
                finalBossAreaIntro.SetActive(true); 
                break;
        }

        yield return StartCoroutine(FadeCanvas(1f));

        yield return new WaitForSeconds(1f);

        // return control to the player  
        playerMovement.enabled = true;

        yield return StartCoroutine(FadeCanvas(0f));

        neutralAreaIntro.SetActive(false);
        neutralAreaIntro.SetActive(false);
        fireAreaIntro.SetActive(false);
        windAreaIntro.SetActive(false);
        waterAreaIntro.SetActive(false);
        earthAreaIntro.SetActive(false);
        neutralAreaIntro.SetActive(false);

        areaIntros.SetActive(false);
        
        yield return null;
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float fadeDuration = 0.5f;
        float startAlpha = areaIntrosCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            areaIntrosCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        areaIntrosCanvasGroup.alpha = targetAlpha;
    }

    /*
        Lootboxes
    */

    public IEnumerator LootBoxPerformer(LootCardGA lootCardGA)
    {
        LootView.SetActive(true);
        AudioSystem.Instance.PlayMusic("victory");
        LootSelectionCompleted = false;
        yield return null;
    }

    private void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.PlayMusic("overworld", true);
        LootView.SetActive(false);
        LootSelectionCompleted = true;
    }

    public void OnSkipButtonClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.PlayMusic("overworld", true);
        LootView.SetActive(false);
        LootSelectionCompleted = true;
    }

    /*
        Campfires
    */
    
    public IEnumerator CampfirePerformer(CampfireGA campfireGA)
    {
        campfireView.SetActive(true);
        AudioSystem.Instance.PlayMusic("victory");
        CampfireInteractCompleted = false;

        // heal player
        PlayerSystem.Instance.AddHealth(campfireGA.amount);

        yield return null;
    }

    public void OnContinueButtonClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.PlayMusic("overworld", true);
        campfireView.SetActive(false);
        CampfireInteractCompleted = true;
    }

    public IEnumerator NextAreaPerformer(NextAreaGA nextAreaGA)
    {
        if (nextAreaGA.applyLevelTransition)
            NextLevel(nextAreaGA.nextLevel);
        yield return null;
    }

    /*
        Helper functions
    */

    private void SetActiveArea()
    {
        tutorialLevel.SetActive(false);
        neutralArea.SetActive(false); 
        fireArea.SetActive(false); 
        windArea.SetActive(false); 
        waterArea.SetActive(false);
        earthArea.SetActive(false);
        finalBossLevel.SetActive(false);

        // transition to level 1 of the next area
        switch (currentAreaType)
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

    private void SetActiveEnemyBank()
    {
        GameObject areaRoot = null;
        switch (currentAreaType)
        {
            case 1: areaRoot = neutralArea; break;
            case 2: areaRoot = fireArea; break;
            case 3: areaRoot = windArea; break;
            case 4: areaRoot = waterArea; break;
            case 5: areaRoot = earthArea; break;
            default: return;
        }

        if (areaRoot == null)
        {
            Debug.LogWarning("LevelSystem.SetActiveEnemyBank: area root is null for currentAreaType " + currentAreaType);
            return;
        }

        Transform bankTransform = areaRoot.transform.Find("Enemy Bank");
        if (bankTransform == null)
        {
            Debug.LogWarning("LevelSystem.SetActiveEnemyBank: no child named \"Enemy Bank\" under " + areaRoot.name + ". Check hierarchy spelling.");
            return;
        }

        GameObject enemyBank = bankTransform.gameObject;
        if (enemyBank.transform.childCount < 3)
        {
            Debug.LogWarning("LevelSystem.SetActiveEnemyBank: Enemy Bank needs 3 children (A1–A3). Found " + enemyBank.transform.childCount + ".");
            return;
        }

        enemyBank.transform.GetChild(0).gameObject.SetActive(false); // A1 enemy bank
        enemyBank.transform.GetChild(1).gameObject.SetActive(false); // A2 enemy bank
        enemyBank.transform.GetChild(2).gameObject.SetActive(false); // A3 enemy bank

        // activate the correct area enemy bank for enemy difficulty scaling
        switch (currentArea)
        {
            case 1: enemyBank.transform.GetChild(0).gameObject.SetActive(true); break;
            case 2: enemyBank.transform.GetChild(1).gameObject.SetActive(true); break;
            case 3: enemyBank.transform.GetChild(2).gameObject.SetActive(true); break;
            default: return;
        }
    }

   
}