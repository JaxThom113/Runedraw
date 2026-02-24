using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
public class CameraTransitionSystem : Singleton<CameraTransitionSystem>
{ 
    [SerializeField] GameObject GameViewContainer; 
    [SerializeField] MatchSetupSystem matchSetupSystem;
    [SerializeField] GameObject playerSprite; 
    [SerializeField] GameObject AreaLevelInfo;
    [SerializeField] CinemachineVirtualCamera gameViewCamera; 
    [SerializeField] CinemachineVirtualCamera overworldViewCamera;   
    [SerializeField] int gameOverVirtualCameraPriority;
    [SerializeField] float rotationTweenDuration = 1f;


    void Start()
    { 

       
    }

   
   
    public void startGame(OverworldEnemy overworldEnemy) {    
        GameObject EnemySprite = overworldEnemy.SpriteGameObject;
        overworldViewCamera.Priority = 0; 
        gameViewCamera.Priority = 10;  
        AreaLevelInfo.SetActive(false);
        StartCoroutine(showGameView(overworldEnemy));
        EnemySprite.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), rotationTweenDuration); 
        EnemySprite.transform.DOLocalMove(new Vector3(3.0f, 2.43f, -4.8f), rotationTweenDuration); 
        EnemySprite.transform.DOScale(new Vector3(0.15f, 0.15f, 0.15f), rotationTweenDuration);
        playerSprite.transform.DOLocalRotate(new Vector3(90f, 0f, 0f), rotationTweenDuration);
        Debug.Log("Camera has transitioned"); 
    }  
    private IEnumerator showGameView(OverworldEnemy overworldEnemy) {
        yield return new WaitForSeconds(rotationTweenDuration); 
        GameViewContainer.SetActive(true); 
        matchSetupSystem.SetupMatch(overworldEnemy);
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
        playerSprite.transform.DOLocalRotate(new Vector3(23f, 0f, 0f), rotationTweenDuration);
        AreaLevelInfo.SetActive(true);
        Debug.Log("Camera has transitioned"); 
    }

    
}

