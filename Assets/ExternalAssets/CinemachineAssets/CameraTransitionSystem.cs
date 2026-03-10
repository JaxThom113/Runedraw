using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
public class CameraTransitionSystem : Singleton<CameraTransitionSystem>
{ 
    [SerializeField] GameObject GameViewContainer; 
    [SerializeField] MatchSetupSystem matchSetupSystem;
    [SerializeField] GameObject playerContainer; 
    [SerializeField] GameObject OverworldHUD;
    [SerializeField] CinemachineVirtualCamera gameViewCamera; 
    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f; 

    private Transform playerSprite;


    void Start()
    { 
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
        SoundEffectSystem.Instance.PlayBattleTheme();
        OverworldHUD.SetActive(false);
         StartCoroutine(showGameView(overworldEnemy)); 
        GameObject EnemySprite = overworldEnemy.SpriteGameObject;
         
        
       
        EnemySprite.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), rotationTweenDuration); 
        EnemySprite.transform.DOLocalMove(new Vector3(0.0f, 2.3f, -3f), rotationTweenDuration); 
        EnemySprite.transform.DOScale(new Vector3(0.15f, 0.15f, 0.15f), rotationTweenDuration); 
        playerContainer.transform.DOLocalMove(new Vector3(0f, -1f, 0f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(90f, 0f, 0f), rotationTweenDuration);
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
        SoundEffectSystem.Instance.PlayOverworldTheme();
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

