using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CardSystem : Singleton<CardSystem>
{ 
     //Hold the LOGIC, takes GA's as input for data
    

    /* 
    DrawCardGA -> DrawCardPerformer -> DrawCard() -> AddCard() -> ApplyCard()
    DiscardCardGA -> DiscardCardPerformer -> DiscardCard() -> RemoveCard() -> ApplyCard()
    PlayCardGA -> PlayCardPerformer -> PlayCard() -> RemoveCard() -> ApplyCard()
    EnemyTurnGA -> EnemyTurnPreReaction -> DiscardCardGA -> AddReaction()
    EnemyTurnGA -> EnemyTurnPostReaction -> DrawCardGA -> AddReaction()
    */     

    [SerializeField] public LootCardBank lootCardBank;
    // Start is called before the first frame update 
    [SerializeField] private CardSO cardSO; 
    
    [SerializeField] private Transform drawPileTransform;
    [SerializeField] private Transform discardPileTransform;
    [SerializeField] private Transform enemyHandTransform;
    
    private List<Card> drawPile = new(); 
    private List<Card> discardPile = new();  
    private List<Card> hand = new();    
    public List<Card> enemyDeck = new();  
    private bool actionHooksBound = false;
    private EnemyTurnGA lastProcessedEnemyTurnGA = null;

    
    // Action System Setup
    private void OnEnable() 
    {  
        if (actionHooksBound) return;
        actionHooksBound = true;
        //Attach Performer to add to dictionary so we wont get an error when performperformer/performsubscriber
        ActionSystem.AttachPerformer<DrawCardGA>(DrawCardPerformer);
        ActionSystem.AttachPerformer<DrawEnemyCardGA>(DrawEnemyCardPerformer);
        ActionSystem.AttachPerformer<DiscardCardGA>(DiscardCardPerformer);  
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer); 
        ActionSystem.SubscribeReaction<LootCardGA>(CreateLootCardsPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE); //same thing as above, but if prereaction or postreaction we call subscribe reaction instead of attach perfomer
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST); 
        ActionSystem.SubscribeReaction<KillEnemyGA>(DiscardEnemyCardPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<KillEnemyGA>(RefillDeckPostReaction, ReactionTiming.POST);
     } 
    private void OnDisable() 
    {   
        if (!actionHooksBound) return;
        actionHooksBound = false;
        UnsubscribeAll();
    }
    private void OnDestroy()
    { 
        Debug.LogError("CardSystem OnDestroy");
        if (!actionHooksBound) return;
        actionHooksBound = false;
        UnsubscribeAll(); // static subscriber list outlives this instance; remove so destroyed instance is never invoked
    }
    private void UnsubscribeAll()
    {
        ActionSystem.DetachPerformer<DrawCardGA>(); //remove from dictionary so we wont get an error when detaching performer
        ActionSystem.DetachPerformer<DrawEnemyCardGA>();
        ActionSystem.DetachPerformer<DiscardCardGA>(); 
        ActionSystem.DetachPerformer<PlayCardGA>();     
        ActionSystem.UnsubscribeReaction<LootCardGA>(CreateLootCardsPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE); 
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST); 
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(DiscardEnemyCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(RefillDeckPostReaction, ReactionTiming.POST);
        //Remove from dictionary so we wont get an error when unsubscribing reaction
    }  
    //Public Methods 
    public void Setup(List<CardSO> cardSOs, List<CardSOList> enemyCardSOs)  
    {  
        drawPile.Clear();
        foreach(var cardSO in cardSOs) {
            if (cardSO == null) { 
                Debug.LogError("CardSO is null in CardSystem Setup"); 
                continue;
            }; // skip null refs (e.g. empty slots in SO list)
            Card card = new Card(cardSO); 
            if (card == null) { 
                Debug.LogError("Card is null in CardSystem Setup"); 
                continue;
            };
            drawPile.Add(card);
        } 
        List<CardSO> enemyHand = EnemySystem.Instance.GetCurrentEnemyHand();
        foreach(var cardSO in enemyHand) {  
            Card card = new Card(cardSO);
            enemyDeck.Add(card);
        }
    } 
    private void SetupEnemyDeck(List<CardSOList> enemyCardSOs){ 
        enemyDeck.Clear();
        List<CardSO> enemyHand = EnemySystem.Instance.GetCurrentEnemyHand();
        foreach(var cardSO in enemyHand) {  
            Card card = new Card(cardSO);
            enemyDeck.Add(card);
        }
    }

    //Performers 
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    { 
        
        hand.Remove(playCardGA.card); 
        discardPile.Add(playCardGA.card);
        ApplyCard applyCard = HandView.Instance.RemoveCard(playCardGA.card);
        yield return DiscardCard(applyCard);
        
        if (playCardGA.card.sound != null)
        {
            ActionSystem.Instance.AddReaction(new SoundEffectGA(playCardGA.card.sound));
        }  

        //ManaSystem.Instance.SpendMana(playCardGA.card.cardCost);  
        //DONT SPEND MANA DIRECLTY! add it to que in the action system to avoid any bugs
        SpendManaGA spendManaGA = new(manaAmount: playCardGA.card.cardCost); 
        ActionSystem.Instance.AddReaction(spendManaGA); 
        foreach(var effect in playCardGA.card.effects) { 
                effect.isPlayer = true;
                PerformEffectGA performEffectGA = new(effect);
                ActionSystem.Instance.AddReaction(performEffectGA); //add to subscriber list, since we cant call a perfomer in a performer  
                //This is protected in the IsPerforming check at the start of the perform method
        }
    }
    private IEnumerator DrawCardPerformer(DrawCardGA drawCardGA)
    {
        int cardAmount = Mathf.Min(drawCardGA.Amount, drawPile.Count);  
        if(cardAmount < drawCardGA.Amount) {  
            RefillDeck();
            cardAmount = Mathf.Min(drawCardGA.Amount, drawPile.Count);
        }
        int notDrawnAmount = drawCardGA.Amount - cardAmount; 
        for (int i = 0; i < cardAmount; i++) 
        { 
            yield return DrawCard();
        }
        if (notDrawnAmount > 0) { 
            RefillDeck(); 
            for(int i = 0; i < notDrawnAmount; i++) { 
                yield return DrawCard();
            }
        }
        //Card card = new Card(cardSO);  
    } 
    private IEnumerator DrawEnemyCardPerformer(DrawEnemyCardGA drawEnemyCardGA)
    {
        int cardAmount = Mathf.Min(drawEnemyCardGA.Amount, EnemySystem.Instance.GetDrawAmount());  
        
        int notDrawnAmount = drawEnemyCardGA.Amount - cardAmount; 
        for(int i = 0; i < cardAmount; i++) { 
            yield return DrawEnemyCard();
        }
    } 
   
    private IEnumerator DiscardCardPerformer(DiscardCardGA discardCardGA)
    { 
        foreach(var card in hand) { 
            discardPile.Add(card); 
            ApplyCard applyCard = HandView.Instance.RemoveCard(card); 
            yield return DiscardCard(applyCard);
        }
        hand.Clear();
    } 
    private void DiscardEnemyCardPostReaction(KillEnemyGA killEnemyGA)
    { 
        ShieldSystem.Instance.ClearAllShields();
        foreach(var card in enemyDeck) { 
            
           EnemyHandView.Instance.ClearEnemyHand(); 
            
        }
        enemyDeck.Clear();
    }
      
    // Reactions 
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA){  
        Debug.Log("EnemyTurnPreReaction");
         DiscardCardGA discardCardGA = new(); 
         ActionSystem.Instance.AddReaction(discardCardGA);

    } 
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA) 
    {   
        // Defense against infinite loop
        if (ReferenceEquals(lastProcessedEnemyTurnGA, enemyTurnGA))
        {
            return;
        }
        lastProcessedEnemyTurnGA = enemyTurnGA;

        ShieldSystem.Instance.ClearAllShields();
        SetupEnemyDeck(EnemySystem.Instance.enemy.enemyDeck);
        DrawCardGA drawCardGA = new(5); 
        ActionSystem.Instance.AddReaction(drawCardGA);  
        DrawEnemyCardGA drawEnemyCardGA = new(EnemySystem.Instance.GetDrawAmount()); 
        ActionSystem.Instance.AddReaction(drawEnemyCardGA); 
        // RefillManaGA refillManaGA = new(ManaSystem.Instance.maxMana);  
        // ActionSystem.Instance.AddReaction(refillManaGA); 
       
    }
    //Helper Methods
    private IEnumerator DrawCard() 
    {
        SoundEffectSystem.Instance.PlayCardDrawSound();
        Card card = drawPile.Draw(); 
        if (card == null) { 
            Debug.LogError("Card is null in DrawCard"); 
            yield break;
        };
        hand.Add(card);
        ApplyCard applyCard = CardCreator.Instance.CreateCard(card, drawPileTransform.position, drawPileTransform.rotation, false);    
        yield return  StartCoroutine(HandView.Instance.AddCard(applyCard));
    } 
    private void CreateLootCardsPostReaction(LootCardGA lootCardGA)  
    { 
        CardSO ultimateCard = EnemySystem.Instance != null ? EnemySystem.Instance.enemy?.ultimateCard : null;
        List<CardSO> lootCards = lootCardBank.GetRandomCardsEnemy(ultimateCard);
        foreach(var cardSO in lootCards) {
            Card card = new Card(cardSO);
            ApplyCard applyCard = LootCardCreator.Instance.CreateCard(card, Vector3.zero, Quaternion.identity, false);
             StartCoroutine(LootHandView.Instance.AddCard(applyCard));
        }
    }
    private IEnumerator DrawEnemyCard() 
    {
        SoundEffectSystem.Instance.PlayCardDrawSound();
        Card card = enemyDeck.DrawFront();
          
        enemyDeck.Add(card);
        ApplyCard applyCard = CardCreator.Instance.CreateCard(card, enemyHandTransform.position, enemyHandTransform.rotation, true);    
       yield return StartCoroutine(EnemyHandView.Instance.AddCard(applyCard));
        bool playWhenDrawn = card.effects != null && card.effects.Exists(e => e.playWhenDrawnByEnemy);
        if (playWhenDrawn)
        {
            foreach (var effect in card.effects)
            {
                effect.isPlayer = false; 
                PerformEffectGA performEffectGA = new(effect);
                ActionSystem.Instance.AddReaction(performEffectGA); 
            }
            yield return EnemyHandView.Instance.RemoveEnemyCard(card); 
            
        }                  
        
    }

    private void RefillDeck() 
    { 
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        
    }  
    private void RefillDeckPostReaction(KillEnemyGA killEnemyGA)
    {
        RefillDeck();
        LootCardGA lootCardGA = new LootCardGA(3);
        ActionSystem.Instance.AddReaction(lootCardGA);
    }

    private IEnumerator DiscardCard(ApplyCard applyCard) 
    {
        if(applyCard == null || !applyCard.gameObject.activeInHierarchy) yield break;
        SoundEffectSystem.Instance.PlayCardDiscardSound();
        applyCard.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = applyCard.transform.DOMove(discardPileTransform.position, 0.15f); 
        yield return tween.WaitForCompletion();
        Destroy(applyCard.gameObject);
        
    }
    
}
