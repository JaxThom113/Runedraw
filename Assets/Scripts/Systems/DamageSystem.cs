using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageSystem : Singleton<DamageSystem>
{   
    [SerializeField] public PlayerView playerView;  
    [SerializeField] public EnemyView enemyView;  
    // Start is called before the first frame update
    private void OnEnable() 
    { 
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    } 
    void OnDisable() 
    { 
        ActionSystem.DetachPerformer<DealDamageGA>();
    }   
    public void Setup(PlayerView playerView, EnemyView enemyView) {
        this.playerView = playerView;
        this.enemyView = enemyView;
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA) { 
        int damageAmount = dealDamageGA.Amount;   
        Debug.Log("Dealing damage: " + damageAmount);
        if(dealDamageGA.isPlayer) { 
            enemyView.TakeDamage(damageAmount); 
            if(enemyView.currentHealth <= 0) {  
                Debug.Log("Enemy killed");
                KillEnemyGA killEnemyGA = new(enemyView); 
                ActionSystem.Instance.AddReaction(killEnemyGA);
            }
        } else { 
            playerView.TakeDamage(damageAmount); 
            if(playerView.currentHealth <= 0) {  
                GameOverGA gameOverGA = new();
                ActionSystem.Instance.AddReaction(gameOverGA);
            }
        }
        yield return null;
    }

   
}
