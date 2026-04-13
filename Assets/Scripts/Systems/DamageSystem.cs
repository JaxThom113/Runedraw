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
        int damageAmount = dealDamageGA.Amount + (applyAdditionalDamage ? additionalDamage : 0);   
        if(dealDamageGA.isPlayer) { 
            if (enemyView == null || killQueued || enemyView.currentHealth <= 0)
            {
                yield break;
            }
            enemyView.TakeDamage(damageAmount); 
            if(enemyView.currentHealth <= 0) {   
               
                killQueued = true;
                KillEnemyGA killEnemyGA = new(enemyView);  
                CameraTransitionSystem.Instance.GameViewContainer.SetActive(false);
                ActionSystem.Instance.AddReaction(killEnemyGA);
            }
        } else { 
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
