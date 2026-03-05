using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Cinemachine;

public class MatchSetupSystem : MonoBehaviour
{ 

    [SerializeField] public PlayerSO playerData; //FIXME: add different characters later 

    [SerializeField] public PlayerView playerView;
    [SerializeField] public EnemyView enemyView; 
    [SerializeField] public CinemachineVirtualCamera playerCamera;
      
   
    public void SetupMatch(OverworldEnemy overworldEnemy){  
      
        playerCamera.Follow = overworldEnemy.SpriteGameObject.transform;
        EnemySO enemyData = overworldEnemy.enemyData; 
        PlayerSystem.Instance.Setup(playerData, playerView);
        EnemySystem.Instance.Setup(overworldEnemy); 
        DamageSystem.Instance.Setup(playerView, enemyView);
        ShieldSystem.Instance.Setup(playerView, enemyView);
        playerView.Setup(playerData); 
        
        enemyView.Setup(enemyData); 
        
         StartCoroutine(SetupCards());
    }
 

    private IEnumerator SetupCards(){  
        yield return new WaitForSeconds(1f); 
        
        List<CardSO> playerDeck = PlayerSystem.Instance.player.playerDeck;  
        List<CardSOList> enemyDeck = EnemySystem.Instance.enemy.enemyDeck; 
        CardSystem.Instance.Setup(playerDeck, enemyDeck); 
        DrawEnemyCardGA drawEnemyCardGA = new(EnemySystem.Instance.GetDrawAmount()); 
        
        DrawCardGA drawCardGA = new(5);   
        ActionSystem.Instance.Perform(drawCardGA, ()=> {
            ActionSystem.Instance.Perform(drawEnemyCardGA, () => 
            ManaSystem.Instance.InitializeMana());
        }); 
    } 

}
