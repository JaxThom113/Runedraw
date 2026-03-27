using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{ 
    [Header("Player SO References")]
    [SerializeField] public PlayerSO drawthurPlayerData;
    [SerializeField] public PlayerSO decklanPlayerData;
    [SerializeField] public PlayerSO shufflynnPlayerData;
    public PlayerSO currentPlayerData;

    [Header("Other References")]
    public GameObject playerSprite;

    public Player player; 
    public PlayerView playerView; 
    public GameObject DeathView; 
    public GameObject GameView;
    public int storedHealth; 

    void Start()
    {   
        // assign the correct player so depending on what character was picked in the menu
        switch (GameData.SelectedPlayer)
        {
            case 0: currentPlayerData = drawthurPlayerData; break;
            case 1: currentPlayerData = decklanPlayerData; break;
            case 2: currentPlayerData = shufflynnPlayerData; break;
        }
        playerSprite.GetComponent<SpriteRenderer>().sprite = currentPlayerData.entityIcon;

        playerView.Setup(currentPlayerData);
        Setup(playerView); 
        Inventory.Instance.Setup(player.playerDeck);
    }

    public void Setup(PlayerView playerView)
    {
        player = new Player();
        player.Setup(currentPlayerData);

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
