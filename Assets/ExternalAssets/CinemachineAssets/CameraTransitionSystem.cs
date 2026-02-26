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
    [SerializeField] GameObject AreaLevelInfo;
    [SerializeField] CinemachineVirtualCamera gameViewCamera; 
    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f; 

    private Transform playerSprite;


    void Start()
    { 
        playerSprite = playerContainer.transform.Find("Sprite");
       
    }

   
   
    public void startGame(OverworldEnemy overworldEnemy) {    

         StartCoroutine(showGameView(overworldEnemy)); 
        GameObject EnemySprite = overworldEnemy.SpriteGameObject;
         
        AreaLevelInfo.SetActive(false);
       
        EnemySprite.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), rotationTweenDuration); 
        EnemySprite.transform.DOLocalMove(new Vector3(0.0f, 2.3f, -3f), rotationTweenDuration); 
        EnemySprite.transform.DOScale(new Vector3(0.15f, 0.15f, 0.15f), rotationTweenDuration); 
        playerContainer.transform.DOLocalMove(new Vector3(0f, -1f, 0f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(90f, 0f, 0f), rotationTweenDuration);
        Debug.Log("Camera has transitioned"); 
    }  
    private IEnumerator showGameView(OverworldEnemy overworldEnemy) {  
        GameViewContainer.SetActive(true); 
        matchSetupSystem.SetupMatch(overworldEnemy); 
        yield return new WaitForSeconds(rotationTweenDuration);  
        overworldViewCamera.Priority = 0; 
        gameViewCamera.Priority = 10; 
        
        
    } 
    private IEnumerator endGameView() { 
        yield return new WaitForSeconds(rotationTweenDuration);
        GameViewContainer.SetActive(false);
        AreaLevelInfo.SetActive(true);
        Debug.Log("Camera has transitioned");
    }
    public void endGame() {    
        overworldViewCamera.Priority = 10;  
        gameViewCamera.Priority = 0;  
        StartCoroutine(endGameView());  
        playerContainer.transform.parent.DOLocalRotate(new Vector3(0f, 0f, 0f), rotationTweenDuration);
        playerContainer.transform.DOLocalMove(new Vector3(0f, 0f, 0f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(-23f, 0f, 0f), rotationTweenDuration);
        AreaLevelInfo.SetActive(true);
        Debug.Log("Camera has transitioned"); 
    }

    
}

