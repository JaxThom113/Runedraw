using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class CardSystem : Singleton<CardSystem>
{ 
     //Hold the LOGIC, takes GA's as input for data
    

    /* 
    DrawCardGA -> DrawCardPerformer -> DrawCard() -> AddCard() -> ApplyCard()
    DiscardCardGA -> DiscardCardPerformer -> DiscardCard() -> RemoveCard() -> ApplyCard()
    PlayCardGA -> PlayCardPerformer -> PlayCard() -> RemoveCard() -> ApplyCard()

    Turn flow (not in PlayerSystem — lives here + EnemySystem):
    - Player ends turn -> Perform(EnemyTurnGA)
      PRE:  EnemyTurnPreReaction -> queues DiscardCardGA (discard player hand)
      PERFORM: EnemyTurnPerformer (EnemySystem) -> play each card in enemy hand, then EnemyTurnHandler() bumps enemyTurnCount
      POST: EnemyTurnPostReaction -> queues StartRoundGA (NOT DrawCardGA directly; comment was stale)
    - StartRoundGA PERFORM: SetupEnemyDeck (fills enemyDeck from GetCurrentEnemyHand for new enemyTurnCount),
      then queues player DrawCardGA, DrawEnemyCardGA, status follow-ups.
    - enemyTurnCount advances in EnemyTurnHandler at end of enemy play phase (and KillEnemy reset). Enemy DrawCardsEffect uses DrawEnemyCardGA(AdvanceToNextHandBeforeDraw) so advance + SetupEnemyDeck run in the performer, not when the effect is queued.
    */     

    public Canvas cardCanvas; 
    bool enableRayCast = false;
    private GraphicRaycaster cardCanvasRaycaster;
    [SerializeField] private float cardCanvasReEnableDelay = 5f;

    [SerializeField] public LootCardBank lootCardBank;
    // Start is called before the first frame update 
    [SerializeField] private CardSO cardSO; 
    
    [FormerlySerializedAs("drawPileTransform")]
    [SerializeField] private Transform playerDrawPileTransform;
    [FormerlySerializedAs("discardPileTransform")]
    [SerializeField] private Transform playerDiscardPileTransform;
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private Transform enemyHandContainer;

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
        ActionSystem.SubscribeReaction<KillEnemyGA>(DiscardEnemyCardPreReaction, ReactionTiming.PRE);
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
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(DiscardEnemyCardPreReaction, ReactionTiming.PRE);
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
        StartCoroutine(DisableCardCanvasForPlay());
        // Status effects first and before discard tween so stacks/performers are not delayed by animation
        // or by other effects listed later on the card (e.g. draw before stun on the same CardSO). 
         
        if (playCardGA.card.data is AttactionSO)
            ActionSystem.Instance.AddReaction(new SpellCastGA(playCardGA.card.GetElementIndex(), true));
        foreach (var effect in playCardGA.card.effects)
        {
            if (effect is StatusEffect statusEffect)
            {
                if (statusEffect.effectSelf)
                    ActionSystem.Instance.AddReaction(new AddStatusEffect(statusEffect, statusEffect.duration, instigatorIsPlayer: false));
                else
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
            if (effect.effectSelf)
                ActionSystem.Instance.AddReaction(new PerformEffectGA(effect, instigatorIsPlayer: false));
            else
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

        if (drawEnemyCardGA.AdvanceToNextHandBeforeDraw)
        {
            EnemySystem.Instance.EnemyTurnHandler();
            SetupEnemyDeck(EnemySystem.Instance.enemy.enemyDeck);
        }

        // Requested amount (inspector / StartRoundGA), data hand size, and cards in pile (after optional SetupEnemyDeck).
        // DrawEnemyCard() uses DrawFront then Add — the list never shrinks, so looping past enemyDeck.Count repeats the same rotation.
        int cardAmount = Mathf.Min(
            drawEnemyCardGA.Amount,
            EnemySystem.Instance.GetDrawAmount(),
            enemyDeck.Count);
        for (int i = 0; i < cardAmount; i++)
        {
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
    private void DiscardEnemyCardPreReaction(KillEnemyGA killEnemyGA)
    {
        ShieldSystem.Instance.ClearAllShields();
        EnemyHandView.Instance.ClearEnemyHand();
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
        AudioSystem.Instance.PlaySFX("cardDraw");
        Card card = drawPile.Draw(); 
        if (card == null) yield break;
        hand.Add(card);
        Transform playerDraw = playerDrawPileTransform != null ? playerDrawPileTransform : playerHandContainer;
        Vector3 spawnPos = playerDraw != null ? playerDraw.position : Vector3.zero;
        Quaternion spawnRot = playerDraw != null ? playerDraw.rotation : Quaternion.identity;
        ApplyCard applyCard = CardCreator.Instance.CreateCard(card, spawnPos, spawnRot, false, playerHandContainer);
        yield return StartCoroutine(HandView.Instance.AddCard(applyCard));
    } 
    private void CreateLootCardsPostReaction(LootCardGA lootCardGA)  
    { 
        List<CardSO> lootCards;
        if (lootCardGA.fromEnemy)
        {
            CardSO ultimateCard = EnemySystem.Instance.enemy?.ultimateCard;
            lootCards = lootCardBank.GetRandomCardsEnemy(ultimateCard);
        }
        else
        {
            lootCards = lootCardBank.GetRandomCards(lootCardGA.amount);
        }

        foreach(var cardSO in lootCards) {
            Card card = new Card(cardSO);
            ApplyCard applyCard = LootCardCreator.Instance.CreateCard(card, Vector3.zero, Quaternion.identity, false);
            applyCard.LootFromEnemy = lootCardGA.fromEnemy;
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
        AudioSystem.Instance.PlaySFX("cardDraw");
        Card card = enemyDeck.DrawFront();
        if (card == null)
        {
            yield break;
        }
          
        enemyDeck.Add(card);
        if (enemyHandContainer == null)
            yield break;
        Transform enemyDrawPile = EnemySystem.Instance.enemyDrawPileTransform;
        if (enemyDrawPile == null)
            yield break;

        ApplyCard applyCard = CardCreator.Instance.CreateCard(
            card,
            enemyDrawPile.position,
            enemyDrawPile.rotation,
            true,
            enemyHandContainer);
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
            if (!card.IsUltimate)
                yield return StartCoroutine(EnemySystem.Instance.TweenEnemyCardToPlayZone(applyCard));
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
        ActionSystem.Instance.AddReaction(new ManaRefreshGA());
        ActionSystem.Instance.AddReaction(new EndBattleViewGA());
        LootCardGA lootCardGA = new LootCardGA(3, true);
        ActionSystem.Instance.AddReaction(lootCardGA);
    }

    private IEnumerator DiscardCard(ApplyCard applyCard) 
    {
        if(applyCard == null || !applyCard.gameObject.activeInHierarchy) yield break;
        AudioSystem.Instance.PlaySFX("cardDiscard");
        applyCard.transform.DOScale(Vector3.zero, 0.15f);
        Transform pile = playerDiscardPileTransform != null ? playerDiscardPileTransform : playerDrawPileTransform;
        Vector3 discardWorld = pile != null ? pile.position : applyCard.transform.position;
        Tween tween = applyCard.transform.DOMove(discardWorld, 0.15f);
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
    private IEnumerator DisableCardCanvasForPlay()
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
        yield return new WaitForSeconds(2.5f);
        cardCanvasRaycaster.enabled = true;
    }
    
}
