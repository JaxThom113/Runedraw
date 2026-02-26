using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{ 
    [SerializeField] public Enemy enemy {get; private set;}   
    [SerializeField] public int enemyTurnCount {get;  set;} = 0;
    [SerializeField] public OverworldEnemy overworldEnemy;
    public void Setup(OverworldEnemy overworldEnemy)
    {
        enemy = new Enemy();
        enemy.Setup(overworldEnemy.enemyData); 
        this.overworldEnemy = overworldEnemy;
    }
    //Performers are created in the system
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer); 
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
    } 

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA) 
    {  
   
        foreach(var card in EnemyHandView.Instance.GetShownCards()) {  
              
            PlayEnemyCardGA playEnemyCardGA = new PlayEnemyCardGA(card); 
            ActionSystem.Instance.AddReaction(playEnemyCardGA);  
            foreach(var effect in card.effects) { 
                effect.isPlayer = false;
                PerformEffectGA performEffectGA = new(effect);
                ActionSystem.Instance.AddReaction(performEffectGA); //add to subscriber list, since we cant call a perfomer in a performer  
                //This is protected in the IsPerforming check at the start of the perform method
            } 
            yield return EnemyHandView.Instance.RemoveEnemyCard(card); 
            
        } 
          yield return new WaitForSeconds(1f); 
          enemyTurnCount++; 
          if(enemyTurnCount >= enemy.enemyDeck.Count) {
            enemyTurnCount = 0;
          }
    }   
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {  
         DiscardCardGA discardCardGA = new(); 
        ActionSystem.Instance.AddReaction(discardCardGA);  
        

        yield return new WaitForSeconds(1f); 
        overworldEnemy.gameObject.SetActive(false); 
        CameraTransitionSystem.Instance.endGame(); 
        
          
    }
    public List<CardSO> GetCurrentEnemyHand()
    {
        return enemy.enemyDeck[enemyTurnCount].enemyHand;
    } 
    public int GetDrawAmount() { 
        if(enemyTurnCount >= enemy.enemyDeck.Count ) {
            enemyTurnCount = 0;
        } 
        return enemy.enemyDeck[enemyTurnCount].enemyHand.Count;
    }

}
