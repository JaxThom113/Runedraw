using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
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

    public Canvas cardCanvas; 
    bool enableRayCast = false;
    private GraphicRaycaster cardCanvasRaycaster;
    [SerializeField] private float cardCanvasReEnableDelay = 5f;

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
        cardCanvasRaycaster = cardCanvas != null ? cardCanvas.GetComponent<GraphicRaycaster>() : null;
       
        //Attach Performer to add to dictionary so we wont get an error when performperformer/performsubscriber
        ActionSystem.AttachPerformer<DrawCardGA>(DrawCardPerformer);
        ActionSystem.AttachPerformer<DrawEnemyCardGA>(DrawEnemyCardPerformer);
        ActionSystem.AttachPerformer<DiscardCardGA>(DiscardCardPerformer);  
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        ActionSystem.AttachPerformer<StartRoundGA>(StartRoundPerformer);
        ActionSystem.AttachPerformer<ShuffleGA>(ShuffleDeckPerformer); 
        ActionSystem.SubscribeReaction<LootCardGA>(CreateLootCardsPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardUpdateApplyCardPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE); //same thing as above, but if prereaction or postreaction we call subscribe reaction instead of attach perfomer
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST); 
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
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
        ActionSystem.DetachPerformer<StartRoundGA>();
        ActionSystem.DetachPerformer<ShuffleGA>();
        ActionSystem.UnsubscribeReaction<LootCardGA>(CreateLootCardsPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardUpdateApplyCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE); 
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST); 
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(DiscardEnemyCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(RefillDeckPostReaction, ReactionTiming.POST); 
        //Remove from dictionary so we wont get an error when unsubscribing reaction
    }  
    //Public Methods 
    public void Setup(List<CardSO> cardSOs, List<CardSOList> enemyCardSOs)  
    {  
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();
        enemyDeck.Clear();
        foreach(var cardSO in cardSOs) {
            if (cardSO == null) continue; // skip null refs (e.g. empty slots in SO list)
            Card card = new Card(cardSO); 
            if (card == null) continue;
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
        // Status effects first and before discard tween so stacks/performers are not delayed by animation
        // or by other effects listed later on the card (e.g. draw before stun on the same CardSO).
        foreach (var effect in playCardGA.card.effects)
        {
            if (effect is StatusEffect statusEffect)
            {
                ActionSystem.Instance.AddReaction(new AddStatusEffect(statusEffect, statusEffect.duration, instigatorIsPlayer: true));
            }
        }

        hand.Remove(playCardGA.card);
        discardPile.Add(playCardGA.card);
        ApplyCard applyCard = HandView.Instance.RemoveCard(playCardGA.card);
        yield return DiscardCard(applyCard);

        if (playCardGA.card.sound != null)
        {
            ActionSystem.Instance.AddReaction(new SoundEffectGA(playCardGA.card.sound));
        }

        ActionSystem.Instance.AddReaction(new SpendManaGA(manaAmount: playCardGA.card.cardCost));

        foreach (var effect in playCardGA.card.effects)
        {
            if (effect is StatusEffect)
                continue;
            ActionSystem.Instance.AddReaction(new PerformEffectGA(effect, instigatorIsPlayer: true));
        }
    }
    private IEnumerator DrawCardPerformer(DrawCardGA drawCardGA)
    {
        if (LevelSystem.Instance != null && LevelSystem.Instance.LootView != null && LevelSystem.Instance.LootView.activeInHierarchy)
        {
            yield break;
        }

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
        if (LevelSystem.Instance != null && LevelSystem.Instance.LootView != null && LevelSystem.Instance.LootView.activeInHierarchy)
        {
            yield break;
        }

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
    private IEnumerator StartRoundPerformer(StartRoundGA startRoundGA)
    {
        ActionSystem.Instance.AddReaction(new ApplyStatusDamageGA());
        ApplyStatusGA applyStatusGA = new();
        ApplyStatusEffectGA applyStatusEffectGA = new();
        ActionSystem.Instance.AddReaction(new ClearAllShieldsGA());
        ActionSystem.Instance.AddReaction(applyStatusGA);
        SetupEnemyDeck(EnemySystem.Instance.enemy.enemyDeck);
        DrawCardGA drawCardGA = new(startRoundGA.playerDrawAmount);
        ActionSystem.Instance.AddReaction(drawCardGA);
        DrawEnemyCardGA drawEnemyCardGA = new(startRoundGA.enemyDrawAmount);
        ActionSystem.Instance.AddReaction(drawEnemyCardGA);
        ActionSystem.Instance.AddReaction(applyStatusEffectGA);
        yield return null;
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
         
         DiscardCardGA discardCardGA = new(); 
         ActionSystem.Instance.AddReaction(discardCardGA);

    } 
    private void PlayCardUpdateApplyCardPostReaction(PlayCardGA playCardGA)
    {
        ActionSystem.Instance.AddReaction(new UpdateApplyCardGA());
    }
    private void StartRoundPreReaction(StartRoundGA startRoundGA)
    {
        Debug.Log("StartRoundPreReaction");
        StartCoroutine(DisableCardCanvasForDraws()); 
        PoisonSystem.Instance?.RefreshBothSides();
        BleedSystem.Instance?.RefreshBothSides();
        VunerableSystem.Instance?.RefreshBothSides();
        StunSystem.Instance?.RefreshBothSides();
    }
    
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA) 
    {   
        // Defense against infinite loop
        if (ReferenceEquals(lastProcessedEnemyTurnGA, enemyTurnGA))
        {
            return;
        }
        lastProcessedEnemyTurnGA = enemyTurnGA;

        if (DamageSystem.Instance != null && DamageSystem.Instance.enemyView != null && DamageSystem.Instance.enemyView.currentHealth <= 0)
        {
            return;
        }

        StartRoundGA startRoundGA = new(5, EnemySystem.Instance.GetDrawAmount());
        ActionSystem.Instance.AddReaction(startRoundGA);
       
        // RefillManaGA refillManaGA = new(ManaSystem.Instance.maxMana);  
        // ActionSystem.Instance.AddReaction(refillManaGA); 
       
    }
    //Helper Methods
    private IEnumerator DrawCard() 
    {
        SoundEffectSystem.Instance.PlayCardDrawSound();
        Card card = drawPile.Draw(); 
        if (card == null) yield break;
        hand.Add(card);
        ApplyCard applyCard = CardCreator.Instance.CreateCard(card, drawPileTransform.position, drawPileTransform.rotation, false);    
        yield return  StartCoroutine(HandView.Instance.AddCard(applyCard));
    } 
    private void CreateLootCardsPostReaction(LootCardGA lootCardGA)  
    { 
        List<CardSO> lootCards;
        if (lootCardGA.fromEnemy)
        {
            CardSO ultimateCard = EnemySystem.Instance != null ? EnemySystem.Instance.enemy?.ultimateCard : null;
            lootCards = lootCardBank.GetRandomCardsEnemy(ultimateCard);
        }
        else
        {
            lootCards = lootCardBank.GetRandomCards(lootCardGA.amount);
        }

        foreach(var cardSO in lootCards) {
            Card card = new Card(cardSO);
            ApplyCard applyCard = LootCardCreator.Instance.CreateCard(card, Vector3.zero, Quaternion.identity, false);
             StartCoroutine(LootHandView.Instance.AddCard(applyCard));
        }
    } 
    private IEnumerator ShuffleDeckPerformer(ShuffleGA shuffleGA)
    {
        int drawCount = hand.Count;
        foreach (var card in hand)
        {
            discardPile.Add(card);
            ApplyCard applyCard = HandView.Instance.RemoveCard(card);
            yield return DiscardCard(applyCard);
        }
        hand.Clear();

        int cardAmount = Mathf.Min(drawCount, drawPile.Count);
        if (cardAmount < drawCount)
        {
            RefillDeck();
            cardAmount = Mathf.Min(drawCount, drawPile.Count);
        }
        int notDrawnAmount = drawCount - cardAmount;
        for (int i = 0; i < cardAmount; i++)
        {
            yield return DrawCard();
        }
        if (notDrawnAmount > 0)
        {
            RefillDeck();
            for (int i = 0; i < notDrawnAmount; i++)
            {
                yield return DrawCard();
            }
        }
    }
    private IEnumerator DrawEnemyCard() 
    {
        SoundEffectSystem.Instance.PlayCardDrawSound();
        Card card = enemyDeck.DrawFront();
        if (card == null)
        {
            yield break;
        }
          
        enemyDeck.Add(card);
        ApplyCard applyCard = CardCreator.Instance.CreateCard(card, enemyHandTransform.position, enemyHandTransform.rotation, true);    
       yield return StartCoroutine(EnemyHandView.Instance.AddCard(applyCard));
        bool playWhenDrawn = card.effects != null && card.effects.Exists(e => e.playWhenDrawnByEnemy);
        if (playWhenDrawn)
        {
            foreach (var effect in card.effects)
            {
                if (!effect.playWhenDrawnByEnemy)
                    continue;
                ActionSystem.Instance.AddReaction(new PerformEffectGA(effect, instigatorIsPlayer: false));
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
        LootCardGA lootCardGA = new LootCardGA(3, true);
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

   
   
    private IEnumerator DisableCardCanvasForDraws()
    { 
        Debug.Log("DisableCardCanvasForDraws");
        if (cardCanvasRaycaster == null)
        { 
            Debug.LogError("CardCanvas is null");
            cardCanvasRaycaster = cardCanvas != null ? cardCanvas.GetComponent<GraphicRaycaster>() : null;
        }

        if (cardCanvasRaycaster == null)
        { 
            Debug.LogError("CardCanvasRaycaster is null");
            yield break;
        }

        cardCanvasRaycaster.enabled = false; 
        yield return new WaitForSeconds(7f);
        cardCanvasRaycaster.enabled = true;
    }
    
}
