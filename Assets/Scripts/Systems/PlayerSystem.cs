using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{ 
    [SerializeField] public PlayerSO starterPlayerData;
    public Player player; 
    public PlayerView playerView; 
    public GameObject DeathView; 
    public GameObject GameView;
    public int storedHealth; 
    void Start(){   

        playerView.Setup(starterPlayerData);
        Setup(starterPlayerData, playerView); 
        Inventory.Instance.Setup(player.playerDeck);
    }

    public void Setup(PlayerSO playerData, PlayerView playerView)
    {
        starterPlayerData = playerData;
        player = new Player();
        player.Setup(starterPlayerData);

        // First time setup inventory cards are not available, so we use the PlayerSO deck as a fallback
        List<CardSO> inventoryCards = Inventory.Instance != null ? Inventory.Instance.GetCards() : null;
        
        if (inventoryCards != null && inventoryCards.Count > 0)
            player.SetupDeck(inventoryCards);

        this.playerView = playerView;
        storedHealth = playerView.currentHealth; // sync so OnEnable restores correct value
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
        storedHealth = playerView.currentHealth;
    } 
    private IEnumerator GameOverPerformer(GameOverGA gameOverGA)
    { 
        DeathView.SetActive(true);
        GameView.SetActive(false);
        yield return null;
    }


}
