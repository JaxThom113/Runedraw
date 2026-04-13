using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections; 

public class CameraTransitionSystem : Singleton<CameraTransitionSystem>
{ 
    [SerializeField] public GameObject GameViewContainer;  
    [SerializeField] MatchSetupSystem matchSetupSystem;
    [SerializeField] GameObject playerSprite;  
    [SerializeField] GameObject EnemyContainer;
    [SerializeField] GameObject OverworldHUD;
    [SerializeField] CinemachineVirtualCamera gameViewCamera;  
    [SerializeField] float overworldNoise = 1.0f; 
    [SerializeField] float gameNoise = 0.3f; 

    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f; 

 

    private CinemachineBasicMultiChannelPerlin gameNoisePerlin;
    public bool inBattleScene = false;
    void Start()
    { 
        AudioSystem.Instance.PlayMusic("overworld", true);
    } 
    void OnEnable()
    {
        ActionSystem.AttachPerformer<LootCardPickupGA>(LootCardPickupPerformer); 
        gameNoisePerlin = gameViewCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>(); 
        gameNoisePerlin.m_AmplitudeGain = overworldNoise;
        gameNoisePerlin.m_FrequencyGain = overworldNoise;
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<LootCardPickupGA>();
    }

   
   
    public void startGame(OverworldEnemy overworldEnemy) {
        inBattleScene = true;
        AudioSystem.Instance.PlayMusic("battle");
        OverworldHUD.SetActive(false);  
        
         gameNoisePerlin.m_AmplitudeGain = gameNoise;
         gameNoisePerlin.m_FrequencyGain = gameNoise;
         StartCoroutine(showGameView(overworldEnemy));  
         GameObject EnemyContainer = GameObject.Find("EnemyContainer");
        string EnemySprite = overworldEnemy.gameObject.name; 
        foreach(Transform child in EnemyContainer.transform) {
            if(child.name == EnemySprite) {
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }
        } 
        overworldEnemy.FadeIn(); 
        
        
        
        // transform enemy to look correct in Gameview
       // EnemySprite.transform.DOLocalRotate(new Vector3(-90f, 0f, -90f), rotationTweenDuration); 
        //EnemySprite.transform.DOLocalMove(new Vector3(0.0f, 0.5f, -0.5f), rotationTweenDuration); 
       // EnemySprite.transform.DOScale(new Vector3(0.025f, 0.025f, 0.025f), rotationTweenDuration); 

        PlayerSystem.Instance.ViewTweenToInteractLocal();

        playerSprite.SetActive(false);
    }  
    private IEnumerator showGameView(OverworldEnemy overworldEnemy) {  
        GameViewContainer.SetActive(true);  
        matchSetupSystem.enemyCanvas.SetActive(true);
        matchSetupSystem.SetupMatch(overworldEnemy); 
        yield return new WaitForSeconds(PlayerSystem.Instance.viewTweenDuration);   

        // overworldViewCamera.Priority = 0; 
        // gameViewCamera.Priority = 10; 
        
        
    } 
    private void endGameViewPreReaction(LootCardGA lootCardGA) {  
        
        
    }

    private IEnumerator LootCardPickupPerformer(LootCardPickupGA lootCardPickupGA)
    { 
        
        if (!lootCardPickupGA.fromEnemy)
            yield break;

        // FogSystem.Instance.BeginFogHideDistanceTweenToLower();

        EnemySystem.Instance.overworldEnemy.ClearStatusVisuals();
        gameNoisePerlin.m_AmplitudeGain = overworldNoise;
        gameNoisePerlin.m_FrequencyGain = overworldNoise;
        inBattleScene = false;
        AudioSystem.Instance.PlayMusic("overworld", true);
        playerSprite.SetActive(true);
        PlayerSystem.Instance.ViewTweenToDefaultLocal();
        OverworldHUD.SetActive(true);
        GameViewContainer.SetActive(false);
        matchSetupSystem.enemyCanvas.SetActive(false);
        yield return null;
    } 


    
}

