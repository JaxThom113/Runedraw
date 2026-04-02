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
    [SerializeField] GameObject playerContainer; 
    [SerializeField] GameObject OverworldHUD;
    [SerializeField] CinemachineVirtualCamera gameViewCamera; 
    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f; 

    private Transform playerSprite;

    public bool inBattleScene = false;
    void Start()
    { 
        AudioSystem.Instance.PlayMusic("overworld", true);
        playerSprite = playerContainer.transform.Find("Sprite");
    } 
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
        AudioSystem.Instance.PlayMusic("battle");
        OverworldHUD.SetActive(false);
         StartCoroutine(showGameView(overworldEnemy)); 
        GameObject EnemySprite = overworldEnemy.SpriteGameObject;
         
        
        // transform enemy to look correct in Gameview
        EnemySprite.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), rotationTweenDuration); 
        EnemySprite.transform.DOLocalMove(new Vector3(0.0f, 0.5f, -0.5f), rotationTweenDuration); 
        EnemySprite.transform.DOScale(new Vector3(0.025f, 0.025f, 0.025f), rotationTweenDuration); 

        // transform player so they are not in the way of the camera
        playerContainer.transform.DOLocalMove(new Vector3(0f, -0.5f, 0f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), rotationTweenDuration);
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
        AudioSystem.Instance.PlayMusic("overworld", true);
        overworldViewCamera.Priority = 10;  
        gameViewCamera.Priority = 0;  
         
        playerContainer.transform.parent.DOLocalRotate(new Vector3(0f, 0f, 0f), rotationTweenDuration);
        playerContainer.transform.DOLocalMove(new Vector3(0f, 0f, 0f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(-23f, 0f, 0f), rotationTweenDuration);
        OverworldHUD.SetActive(true); 
         GameViewContainer.SetActive(false);
        yield return null; 
    }

    
}

