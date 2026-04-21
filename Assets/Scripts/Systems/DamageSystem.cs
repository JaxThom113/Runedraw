using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{   
    [SerializeField] public PlayerView playerView;  
    [SerializeField] public EnemyView enemyView;    
    public int additionalDamage;
    public bool additionalDamageAfflictsPlayer = true;
    private bool killQueued;
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
        killQueued = false;
        additionalDamage = 0;
        additionalDamageAfflictsPlayer = true;
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA) { 
        bool damageHitsPlayer = !dealDamageGA.isPlayer;
        bool applyAdditionalDamage =
            (damageHitsPlayer && additionalDamageAfflictsPlayer) ||
            (!damageHitsPlayer && !additionalDamageAfflictsPlayer);
        int damageAmount = dealDamageGA.magnitude + (applyAdditionalDamage ? additionalDamage : 0);   
        if(dealDamageGA.isPlayer) { 
            if (enemyView == null || killQueued || enemyView.currentHealth <= 0)
            {
                yield break;
            }
            enemyView.TakeDamage(damageAmount); 
            if(enemyView.currentHealth <= 0) {   
               
                killQueued = true;
                KillEnemyGA killEnemyGA = new(enemyView);
                ActionSystem.Instance.AddReaction(killEnemyGA);
            }
        } else { 
            // Drop player-directed damage once the enemy has been killed this flow.
            // Fire-card "deal X to self" effects are queued as sibling reactions of the
            // enemy-damage effect on the same PlayCardGA; without this guard the self
            // damage still resolves *after* KillEnemyGA has wrapped up the battle
            // (storedHealth already captured, EndBattleView queued), causing the
            // delayed hurt animation and the stale health on the next battle enter.
            if (playerView == null || killQueued) yield break;
            playerView.TakeDamage(damageAmount); 
            if(playerView.currentHealth <= 0) {   
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                // GameOverGA gameOverGA = new();
                // ActionSystem.Instance.AddReaction(gameOverGA);
            }
        }
        yield return null;
    } 
    

   
}
