using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Cinemachine;

public class MatchSetupSystem : MonoBehaviour
{ 
    [SerializeField] public PlayerView playerView;
    [SerializeField] public EnemyView enemyView; 
    [SerializeField] public CinemachineVirtualCamera playerCamera;
      
   
    public void SetupMatch(OverworldEnemy overworldEnemy){  
      
        //playerCamera.Follow = overworldEnemy.SpriteGameObject.transform;
        EnemySO enemyData = overworldEnemy.enemyData; 
        PlayerSystem.Instance.Setup(playerView);
        EnemySystem.Instance.Setup(overworldEnemy); 
        overworldEnemy.ApplyMaterial(EnemySystem.Instance.CurrentEnemyMaterial);
        DamageSystem.Instance.Setup(playerView, enemyView);
        ShieldSystem.Instance.Setup(playerView, enemyView); 
        if (enemyData.entityDialogue != null)
            DialogueSystem.Instance.Setup(enemyData.entityDialogue);
        playerView.Setup(PlayerSystem.Instance.currentPlayerData); 
        
        enemyView.Setup(enemyData, overworldEnemy); 
        
         StartCoroutine(SetupCards());
    }
 

    private IEnumerator SetupCards(){  
        yield return new WaitForSeconds(1f); 
        //DialogueSystem.Instance.IntroDialogue();
        List<CardSO> playerDeck = PlayerSystem.Instance.player.playerDeck;  
        List<CardSOList> enemyDeck = EnemySystem.Instance.enemy.enemyDeck; 
        CardSystem.Instance.Setup(playerDeck, enemyDeck); 

        StartRoundGA startRoundGA = new(5, EnemySystem.Instance.GetDrawAmount());
        ActionSystem.Instance.Perform(startRoundGA, () => ManaSystem.Instance.InitializeMana());
    } 

}
