using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    public Player player; 
    public PlayerView playerView; 
    public GameObject DeathView; 
    public GameObject GameView;
    public int storedHealth;
    public void Setup(PlayerSO playerData, PlayerView playerView)
    {  
        Debug.Log("PlayerSystem Setup");
        player = new Player();
        player.Setup(playerData); 
        this.playerView = playerView;
    } 
    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<KillEnemyGA>(StoreHealthPostReaction, ReactionTiming.POST);
        ActionSystem.AttachPerformer<GameOverGA>(GameOverPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(StoreHealthPostReaction, ReactionTiming.POST);
        ActionSystem.DetachPerformer<GameOverGA>();
    } 
    private void StoreHealthPostReaction(KillEnemyGA killEnemyGA)
    { 
        Debug.Log("StoreHealthPostReaction: " + playerView.currentHealth);
        storedHealth = playerView.currentHealth;
    } 
    private IEnumerator GameOverPerformer(GameOverGA gameOverGA)
    { 
        Debug.Log("Performed GameOverPerformer");
        DeathView.SetActive(true);
        GameView.SetActive(false);
        yield return null;
    }


}
