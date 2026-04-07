using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemySystem : Singleton<EnemySystem>
{ 
    [SerializeField] public Enemy enemy {get; private set;}   
    [SerializeField] public int enemyTurnCount {get;  set;} = 0;
    [SerializeField] public OverworldEnemy overworldEnemy;

    [Header("Enemy card transforms")]
    public Transform enemyDrawPileTransform;
    public Transform enemyDiscardPileTransform;
    public Transform enemyPlayZoneTransform;
    public float enemyPlayZoneTweenDuration = 0.25f;
    public Material CurrentEnemyMaterial => enemy != null ? enemy.enemyMaterial : null;

    /// <summary>Move/rotate to play zone (normal enemy plays and play-when-drawn).</summary>
    public IEnumerator TweenEnemyCardToPlayZone(ApplyCard enemyCardView)
    {
        if (enemyCardView == null || enemyPlayZoneTransform == null)
            yield break;

        float d = enemyPlayZoneTweenDuration;
        Transform t = enemyCardView.transform;
        t.DOKill();
        t.DOMove(enemyPlayZoneTransform.position, d);
        t.DORotateQuaternion(enemyPlayZoneTransform.rotation, d);
        yield return new WaitForSeconds(d);
    }

    /// <summary>
    /// World-space ultimate: play zone, shake, hold, then discard. Player ultimates stay on <see cref="ApplyCard"/>.
    /// </summary>
    public IEnumerator EnemyUltimateWindupRoutine(ApplyCard enemyCardView)
    {
        if (enemyCardView == null || enemyPlayZoneTransform == null || enemyDiscardPileTransform == null)
            yield break;

        float playD = enemyPlayZoneTweenDuration;
        Transform t = enemyCardView.transform;
        t.DOKill();
        t.DOMove(enemyPlayZoneTransform.position, playD);
        t.DORotateQuaternion(enemyPlayZoneTransform.rotation, playD);
        yield return new WaitForSeconds(playD);

        if (UISystem.Instance != null)
            UISystem.Instance.TransformShake(t);
        yield return new WaitForSeconds(ApplyCard.UltimateWindupSeconds);

        float discardD = EnemyHandView.Instance != null
            ? EnemyHandView.Instance.duration
            : playD;
        t.DOKill();
        t.DOMove(enemyDiscardPileTransform.position, discardD);
        t.DORotateQuaternion(enemyDiscardPileTransform.rotation, discardD);
        yield return new WaitForSeconds(discardD);
    }

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
        ActionSystem.SubscribeReaction<PlayEnemyCardGA>(PlayEnemyCardUpdateApplyCardPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<KillEnemyGA>(ResetEnemyTurnCountPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.UnsubscribeReaction<PlayEnemyCardGA>(PlayEnemyCardUpdateApplyCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(ResetEnemyTurnCountPostReaction, ReactionTiming.POST);
    } 

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA) 
    {  
   
        foreach (var card in EnemyHandView.Instance.GetShownCards())
        {
            foreach (var effect in card.effects)
            {
                if (effect is StatusEffect statusEffect)
                {
                    ActionSystem.Instance.AddReaction(new AddStatusEffect(statusEffect, statusEffect.duration, instigatorIsPlayer: false));
                }
            }

            PlayEnemyCardGA playEnemyCardGA = new PlayEnemyCardGA(card);
            ActionSystem.Instance.AddReaction(playEnemyCardGA);
            if (card.sound != null)
            {
                ActionSystem.Instance.AddReaction(new SoundEffectGA(card.sound));
            }
            foreach (var effect in card.effects)
            {
                if (effect is StatusEffect)
                    continue;
                ActionSystem.Instance.AddReaction(new PerformEffectGA(effect, instigatorIsPlayer: false));
            }

            ApplyCard enemyCardView = EnemyHandView.Instance.GetApplyCardForCard(card);
            if (!card.IsUltimate)
                yield return TweenEnemyCardToPlayZone(enemyCardView);

            yield return EnemyHandView.Instance.RemoveEnemyCard(card);
        } 
        EnemyTurnHandler();
          yield return new WaitForSeconds(1f); 
          
    } 
    public void EnemyTurnHandler() {
        enemyTurnCount++; 
          if(enemyTurnCount >= enemy.enemyDeck.Count) {
            enemyTurnCount = 0;
          }
    }   
    private void PlayEnemyCardUpdateApplyCardPostReaction(PlayEnemyCardGA playEnemyCardGA)
    {
        ActionSystem.Instance.AddReaction(new UpdateApplyCardGA());
    }
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {  
        DiscardCardGA discardCardGA = new(); 
        ActionSystem.Instance.AddReaction(discardCardGA);  

        if (overworldEnemy != null)
        {
            overworldEnemy.DisableSphereCollider();
        }

        

        if (overworldEnemy != null)
        {
            Tween fadeOutTween = overworldEnemy.FadeOut();
            if (fadeOutTween != null)
                yield return fadeOutTween.WaitForCompletion();

            overworldEnemy.ClearStatusVisuals();
            overworldEnemy.gameObject.SetActive(false);
        } 
        yield return new WaitForSeconds(1f);
    }
    private void ResetEnemyTurnCountPostReaction(KillEnemyGA killEnemyGA)
    {
        enemyTurnCount = 0;
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
