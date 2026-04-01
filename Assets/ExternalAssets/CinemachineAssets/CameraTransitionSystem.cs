using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
public class CameraTransitionSystem : Singleton<CameraTransitionSystem>
{ 
    [SerializeField] GameObject GameViewContainer;  
    [SerializeField] GameObject WorldSpaceCanvas;
    [SerializeField] MatchSetupSystem matchSetupSystem;
    [SerializeField] GameObject playerSprite;  
    [SerializeField] GameObject playerViewContainer; 
    [SerializeField] GameObject EnemyContainer;
    [SerializeField] GameObject OverworldHUD;
    [SerializeField] CinemachineVirtualCamera gameViewCamera; 
    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f; 

 

    public bool inBattleScene = false;
 
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PlayerWinGA>(endGamePerformer); 
        //ActionSystem.SubscribeReaction<LootCardGA>(endGameViewPreReaction, ReactionTiming.PRE);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayerWinGA>();
        //ActionSystem.UnsubscribeReaction<LootCardGA>(endGameViewPreReaction, ReactionTiming.PRE);
    }

   
   
    public void startGame(OverworldEnemy overworldEnemy) {
        inBattleScene = true;
        SoundEffectSystem.Instance.PlayBattleTheme();
        OverworldHUD.SetActive(false);
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

        // transform player so they are not in the way of the camera
        playerViewContainer.transform.DOLocalMove(new Vector3(0f,-2.0f,-1.0f), rotationTweenDuration); 
        playerSprite.SetActive(false);
    }  
    private IEnumerator showGameView(OverworldEnemy overworldEnemy) {  
        GameViewContainer.SetActive(true); 
        matchSetupSystem.SetupMatch(overworldEnemy); 
        yield return new WaitForSeconds(rotationTweenDuration);  
        overworldViewCamera.Priority = 0; 
        gameViewCamera.Priority = 10; 
        
        
    } 
    private void endGameViewPreReaction(LootCardGA lootCardGA) {  
       
        
    }
    public IEnumerator endGamePerformer(PlayerWinGA playerWinGA) {
        inBattleScene = false;
        SoundEffectSystem.Instance.PlayOverworldTheme();
        overworldViewCamera.Priority = 10;  
        gameViewCamera.Priority = 0;  
        playerSprite.SetActive(true);
        playerViewContainer.transform.DOLocalMove(new Vector3(0f,0.0f,-1.0f), rotationTweenDuration);
        OverworldHUD.SetActive(true); 
         GameViewContainer.SetActive(false);
        yield return null; 
    }

    
}

