using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;

public class LevelSystem : Singleton<LevelSystem>
{
    [Header("Debug")]
    public bool debug = false;
    public bool enemies = true;
    public bool interactables = true; 
    public int debugAreaType = 1; 
    public int debugAreaNum = 1;

    [Header("Areas")]
    public GameObject tutorialArea;    
    public GameObject earthArea;
    public GameObject fireArea;
    public GameObject neutralArea;
    public GameObject waterArea;
    public GameObject windArea;
    public GameObject finalBossArea;

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

    /*
        Current area type
            0 = tutorial
            1 = neutral
            2 = fire
            3 = wind
            4 = water
            5 = earth
            6 = final boss
    */
    private int currentArea;
    private int currentAreaType;
    public int CurrentArea => currentArea; // allow access from other scripts
    public int CurrentAreaType => currentAreaType; // allow access from other scripts
    private List<int> areaList;

    // interactable view variables
    public bool LootSelectionCompleted { get; private set; } = false;
    public bool CampfireInteractCompleted { get; private set; } = false;

    // Tracks whether the currently-open loot session came from an enemy kill
    // (true) or a chest interactable (false). Used by OnSkipButtonClicked so
    // the skipped LootCardPickupGA carries the same fromEnemy flag a real
    // pickup would, allowing all post-reaction subscribers (ShaderSystem,
    // ManaSystem, Inventory, etc.) to clean up correctly.
    private bool currentLootFromEnemy;

    // invoke this event when ready to run DrawLevel() in CreateLevel
    public event Action<TextAsset, int, List<EnemySO>, List<EnemySO>> OnReady;  

    public bool FinalBossFightStarted = false;

    private TextAsset tutorialFile;
    private TextAsset finalBossFile;
    private SpecialSeedSO specialSeed;
    private const int DEFAULT_NUM_TORCHES = 30;
    private const int TUTORIAL_NUM_TORCHES = 10;
    private const int FINALBOSS_NUM_TORCHES = 20;

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

    void Start()
    { 
        specialSeed = null;
        tutorialFile = Resources.Load<TextAsset>("Levels/Tutorial");
        finalBossFile = Resources.Load<TextAsset>("Levels/FinalBoss");

        if (GameData.IsSeededRun)
        {
            if (GameData.SpecialSeed != null)
            {
                // apply a special seed if one was given in the menu
                specialSeed = SeedSystem.Instance.GetSpecialSeed(GameData.SpecialSeed);

                // special seeds can contain random areas, make it so these areas are differeny every time
                GameData.SelectedSeed = Environment.TickCount;
                UnityEngine.Random.InitState(GameData.SelectedSeed);
            }

            // get the seed that was selected on the menu
            UnityEngine.Random.InitState(GameData.SelectedSeed);
        }
        else
        {
            // if unseeded run, generate a random seed from current TickCount and save it
            GameData.SelectedSeed = Environment.TickCount;
            UnityEngine.Random.InitState(GameData.SelectedSeed);
        }

        // refresh list available areas
        areaList = new List<int>() { 1, 2, 3, 4, 5 };

        if (debug) 
        { 
            // if debug active at start, just set the starting area type and number
            currentAreaType = debugAreaType;   
            currentArea = debugAreaNum;
        }
        else if (GameData.StartedFromTutorial)
        { 
            // if starting in tutorial, first area type is 0
            currentArea = 0;
            currentAreaType = 0;
        }
        else
        {
            currentArea = 1;

            if (specialSeed != null)
            {
                currentAreaType = specialSeed.areas[0].GetAreaTypeIndex();
            }
            else
            {
                // if not starting from tutorial, roll a random dungeon type for first area
                int randIndex = UnityEngine.Random.Range(0, areaList.Count);
                currentAreaType = areaList[randIndex];
                areaList.RemoveAt(randIndex);
            }

            GameData.Area1 = currentAreaType; 
        }

        SetActiveArea();
        SetActiveEnemyBank();

        if (currentAreaType == 0)
        {
            // load in Tutorial custom area
            OnReady?.Invoke(tutorialFile, TUTORIAL_NUM_TORCHES, null, null);
        }
        else if (currentAreaType == 6)
        {
            // load in FinalBoss custom area (finalboss being the first area is only possible by setting through LevelSystem Debug)
            OnReady?.Invoke(finalBossFile, FINALBOSS_NUM_TORCHES, null, null);
            currentArea = 0;
        }
        else if (specialSeed != null)
        {
            // use level layout and number of torches from the first area specified in given SpecialSeedSO
            // if levelCsv is null, a random level is generated in CreateLevel
            OnReady?.Invoke(
                specialSeed.areas[0].levelCsv, 
                specialSeed.areas[0].numTorches,
                specialSeed.areas[0].enemies,
                specialSeed.areas[0].rareEnemies
            ); 
        }
        else
        {
            // generate random level
            OnReady?.Invoke(null, DEFAULT_NUM_TORCHES, null, null);
        }

        StartCoroutine(ShowAreaIntro());
        UpdateUI();

        // emit one startup area event so reactive systems (fog/shader) sync the initial area
        if (ActionSystem.Instance != null)
            ActionSystem.Instance.Perform(new NextAreaGA(currentArea, applyLevelTransition: false));
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
            // area number in topbar
            areaLevel.text = $"Area #{currentArea}";
        }
    }

    /*
        Level transition/coroutine
    */

    public void NextArea()
    { 
        if (specialSeed == null && currentArea == 3)
        {
            // transition to the final boss level 
            currentAreaType = 6;
            currentArea = 0;

            StartCoroutine(StartTransition(finalBossFile));
        }
        else
        {
            currentArea++;

            if (specialSeed != null)
            {
                if (currentArea > specialSeed.areas.Count)
                {
                    // this is the last area in a special seed
                    currentAreaType = 6;
                    currentArea = 0;

                    StartCoroutine(StartTransition(finalBossFile));
                    UpdateUI();
                    return;
                }
                else
                {
                    currentAreaType = specialSeed.areas[currentArea-1].GetAreaTypeIndex(); 
                }
            }
            else
            {
                // pick the next random area, excluding areas that have already been picked
                int randIndex = UnityEngine.Random.Range(0, areaList.Count);
                currentAreaType = areaList[randIndex];
                areaList.RemoveAt(randIndex);
            }

            // update run info
            switch (currentArea)
            {
                case 1: GameData.Area1 = currentAreaType; break;
                case 2: GameData.Area2 = currentAreaType; break;
                case 3: GameData.Area3 = currentAreaType; break;
            }

            StartCoroutine(StartTransition());
        }
        
        UpdateUI();
    }

    IEnumerator StartTransition(TextAsset file = null)
    {
        // take control from player
        playerMovement.enabled = false;
        yield return new WaitForSeconds(1f);

        SetActiveArea();
        SetActiveEnemyBank();

        if (file != null)
        {
            // transition to a hard-coded custom area (FinalBoss)
            OnReady?.Invoke(file, FINALBOSS_NUM_TORCHES, null, null); 
        }
        else if (specialSeed != null)
        {
            // transition to next area in SpecialSeedSO
            OnReady?.Invoke(
                specialSeed.areas[currentArea-1].levelCsv, 
                specialSeed.areas[currentArea-1].numTorches,
                specialSeed.areas[currentArea-1].enemies,
                specialSeed.areas[currentArea-1].rareEnemies
            ); 
        }
        else
        {
            // transition to next random area
            OnReady?.Invoke(null, DEFAULT_NUM_TORCHES, null, null); 
        }
        
        playerMovement.ResetMovePoint();

        // show the intro for the next area if the player made it to a new area
        StartCoroutine(ShowAreaIntro());
        yield return new WaitForSeconds(1f);

        // return control to the player  
        playerMovement.enabled = true;
        yield return null;
    }

    IEnumerator ShowAreaIntro()
    {
        areaIntros.SetActive(true);

        switch (currentAreaType)
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
        currentLootFromEnemy = lootCardGA.fromEnemy;
        yield return null;
    }
    public void StartFinalBossFight()
    { 
        if(currentAreaType == 6)
        {
            FinalBossFightStarted = true;
        }
    }
    private void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    {
        AudioSystem.Instance.PlaySFX("click");
        AudioSystem.Instance.PlayMusic("overworld", true);
        LootView.SetActive(false);
        LootSelectionCompleted = true;

        // final boss area cleared -> roll credits
        if (FinalBossFightStarted)
        {
            SceneManager.LoadScene("EndCredits");
        }
    }

    public void OnSkipButtonClicked()
    {
        AudioSystem.Instance.PlaySFX("click");
        // Route Skip through the same LootCardPickupGA as a real pickup (minus
        // the "add card to deck" step) so every subscriber —
        // LevelSystem.LootCardPickupPostReaction (hide LootView + music),
        // ShaderSystem (end distortion), ManaSystem (reset mana), Inventory
        // (refresh) — runs. Previously Skip just hid the view inline, which
        // left the battle's shader/mana state dirty after an enemy kill.
        StartCoroutine(SkipLootRoutine());
    }

    private IEnumerator SkipLootRoutine()
    {
        const float timeoutSec = 15f;
        float deadline = Time.realtimeSinceStartup + timeoutSec;
        while (ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (ActionSystem.Instance == null)
            yield break;

        if (ActionSystem.Instance.IsPerforming)
        {
            AudioSystem.Instance.PlayMusic("overworld", true);
            LootView.SetActive(false);
            LootSelectionCompleted = true;
            yield break;
        }

        ActionSystem.Instance.Perform(new LootCardPickupGA(currentLootFromEnemy));
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
            NextArea();
        yield return null;
    }

    /*
        Helper functions
    */

    private void SetActiveArea()
    {
        tutorialArea.SetActive(false);
        neutralArea.SetActive(false); 
        fireArea.SetActive(false); 
        windArea.SetActive(false); 
        waterArea.SetActive(false);
        earthArea.SetActive(false);
        finalBossArea.SetActive(false);

        // transition to level 1 of the next area
        switch (currentAreaType)
        {
            case 0: tutorialArea.SetActive(true); break;
            case 1: neutralArea.SetActive(true); break;
            case 2: fireArea.SetActive(true); break;
            case 3: windArea.SetActive(true); break;
            case 4: waterArea.SetActive(true); break;
            case 5: earthArea.SetActive(true); break;
            case 6: finalBossArea.SetActive(true); break;
        }
    }

    private void SetActiveEnemyBank()
    {
        GameObject areaRoot;

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
            return;

        // access the enemy bank parent object
        GameObject enemyBank = areaRoot.transform.Find("Enemy Bank").gameObject;
        foreach (Transform child in enemyBank.transform)
            child.gameObject.SetActive(false);

        // activate the correct area enemy bank for enemy difficulty scaling
        switch (currentArea)
        {
            case 1: enemyBank.transform.GetChild(0).gameObject.SetActive(true); break; // A1 enemy bank
            case 2: enemyBank.transform.GetChild(1).gameObject.SetActive(true); break; // A2 enemy bank
            case 3: enemyBank.transform.GetChild(2).gameObject.SetActive(true); break; // A3 enemy bank
            default: return;
        }
    }
}