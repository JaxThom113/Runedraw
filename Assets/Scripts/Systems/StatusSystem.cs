using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : Singleton<StatusSystem>
{ 
    // How many stacks of each status (per unit). Not the same as turn countdown — see *TurnRemaining.
    Dictionary<StatusEffect, int> enemyStatusEffects = new Dictionary<StatusEffect, int>();
    Dictionary<StatusEffect, int> playerStatusEffects = new Dictionary<StatusEffect, int>();
    // Turns remaining until this keyed effect procs (e.g. poison burst). Separate from stack count.
    Dictionary<StatusEffect, int> enemyStatusTurnRemaining = new Dictionary<StatusEffect, int>();
    Dictionary<StatusEffect, int> playerStatusTurnRemaining = new Dictionary<StatusEffect, int>();
    // Original applied duration for the currently active status instance. Used for visual normalization.
    Dictionary<StatusEffect, int> enemyStatusAppliedDuration = new Dictionary<StatusEffect, int>();
    Dictionary<StatusEffect, int> playerStatusAppliedDuration = new Dictionary<StatusEffect, int>();
    public StatusUI playerStatusUI;
    public StatusUI enemyStatusUI;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyStatusDamageGA>(ApplyStatusDamagePerformer);
        ActionSystem.AttachPerformer<ApplyStatusGA>(ApplyStatusEffectPerformer);
        ActionSystem.AttachPerformer<ApplyStatusEffectGA>(ApplyLateStatusEffectPerformer);
        ActionSystem.AttachPerformer<AddStatusEffect>(AddStatusEffectPerformer);
        ActionSystem.AttachPerformer<RefreshStatusUIGA>(RefreshStatusUIPerformer);
        ActionSystem.SubscribeReaction<KillEnemyGA>(ClearAllStatusesPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyStatusDamageGA>();
        ActionSystem.DetachPerformer<ApplyStatusGA>();
        ActionSystem.DetachPerformer<ApplyStatusEffectGA>();
        ActionSystem.DetachPerformer<AddStatusEffect>();
        ActionSystem.DetachPerformer<RefreshStatusUIGA>();
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(ClearAllStatusesPostReaction, ReactionTiming.POST);
    }

    public int GetStatusTurnRemaining(StatusEffect effect, bool afflictedUnitIsPlayer)
    { 
        
        Dictionary<StatusEffect, int> turnMap = afflictedUnitIsPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
        if (turnMap.TryGetValue(effect, out int turnsRemaining))
            return turnsRemaining;
        return effect.duration;
    }

    public int GetAppliedDuration(StatusEffect effect, bool afflictedUnitIsPlayer)
    {
        Dictionary<StatusEffect, int> appliedDurationMap = afflictedUnitIsPlayer ? playerStatusAppliedDuration : enemyStatusAppliedDuration;
        if (appliedDurationMap.TryGetValue(effect, out int appliedDuration))
            return appliedDuration;
        return effect.duration;
    }

    // Called once at the start of the turn before shield clear so existing shield can absorb DOT.
    IEnumerator ApplyStatusDamagePerformer(ApplyStatusDamageGA applyStatusDamageGA)
    {    
        VunerableSystem.Instance?.ResetAdditionalDamage();
        ApplyStatusEffectsForSide(playerStatusEffects, true, StatusTurnPhase.Damage);
        ApplyStatusEffectsForSide(enemyStatusEffects, false, StatusTurnPhase.Damage);
        VunerableSystem.Instance?.ResetAdditionalDamage();
        yield return null;
    }

    // Called once at the start of the turn after shield clear for non-damage status effects.
    IEnumerator ApplyStatusEffectPerformer(ApplyStatusGA applyStatusGA)
    {
        VunerableSystem.Instance?.ResetAdditionalDamage();
        ApplyStatusEffectsForSide(playerStatusEffects, true, StatusTurnPhase.Effect);
        ApplyStatusEffectsForSide(enemyStatusEffects, false, StatusTurnPhase.Effect);
        VunerableSystem.Instance?.ResetAdditionalDamage();

        // Refresh UI after all queued status GameActions for the turn finish.
        RefreshStatusUIGA refreshStatusUIGA = new RefreshStatusUIGA();
        ActionSystem.Instance.AddReaction(refreshStatusUIGA);
        yield return null;
    }

    // Called after draws so delayed control/status effects can use the latest visible hand state.
    IEnumerator ApplyLateStatusEffectPerformer(ApplyStatusEffectGA applyStatusEffectGA)
    {
        ApplyDeferredStatusEffectsForSide(playerStatusEffects, true);
        ApplyDeferredStatusEffectsForSide(enemyStatusEffects, false);

        RefreshStatusUIGA refreshStatusUIGA = new RefreshStatusUIGA();
        ActionSystem.Instance.AddReaction(refreshStatusUIGA);
        yield return null;
    }

    private IEnumerator RefreshStatusUIPerformer(RefreshStatusUIGA ga)
    {
        if (ga.refreshBothSides)
        {
            PoisonSystem.Instance?.RefreshBothSides();
            BleedSystem.Instance?.RefreshBothSides();
            VunerableSystem.Instance?.RefreshBothSides();
            StunSystem.Instance?.RefreshBothSides();
            ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());
        }
        else
        {
            PoisonSystem.Instance?.RefreshStatusUI(ga.afflictedUnitIsPlayer);
            BleedSystem.Instance?.RefreshStatusUI(ga.afflictedUnitIsPlayer);
            VunerableSystem.Instance?.RefreshStatusUI(ga.afflictedUnitIsPlayer);
            StunSystem.Instance?.RefreshStatusUI(ga.afflictedUnitIsPlayer);
            ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());
        }

        yield return null;
    }

    private void ClearAllStatusesPostReaction(KillEnemyGA killEnemyGA)
    {
        playerStatusEffects.Clear();
        playerStatusTurnRemaining.Clear();
        playerStatusAppliedDuration.Clear();
        enemyStatusEffects.Clear();
        enemyStatusTurnRemaining.Clear();
        enemyStatusAppliedDuration.Clear();

        if (playerStatusUI != null)
        {
            playerStatusUI.UpdatePoison(0, 0);
            playerStatusUI.UpdateBleed(0, 0);
            playerStatusUI.UpdateVunerable(0, 0);
            playerStatusUI.UpdateStun(0, 0);
        }

        if (enemyStatusUI != null)
        {
            enemyStatusUI.UpdatePoison(0, 0);
            enemyStatusUI.UpdateBleed(0, 0);
            enemyStatusUI.UpdateVunerable(0, 0);
            enemyStatusUI.UpdateStun(0, 0);
        }
    }

    private void ApplyStatusEffectsForSide(Dictionary<StatusEffect, int> statusMap, bool afflictedUnitIsPlayer, StatusTurnPhase turnPhase)
    {  
        foreach (KeyValuePair<StatusEffect, int> kvp in statusMap)
        { 
            int stacks = kvp.Value;
            if (stacks <= 0) continue;
            if (kvp.Key is VunerableStatusEffect || kvp.Key is StunStatusEffect) continue;
            if (kvp.Key.TurnPhase != turnPhase) continue;
            VunerableSystem.Instance?.ResetAdditionalDamage(); //vunerable does not affect status damage
            kvp.Key.PerformStatusEffects(this, stacks, afflictedUnitIsPlayer);
        }
    }

    private void ApplyDeferredStatusEffectsForSide(Dictionary<StatusEffect, int> statusMap, bool afflictedUnitIsPlayer)
    {
        foreach (KeyValuePair<StatusEffect, int> kvp in statusMap)
        {
            int stacks = kvp.Value;
            if (stacks <= 0) continue;
            if (kvp.Key is not VunerableStatusEffect && kvp.Key is not StunStatusEffect) continue;
            kvp.Key.PerformStatusEffects(this, stacks, afflictedUnitIsPlayer);
        }
    }

    // Card adds stacks; turn countdown is set only on first application (same timer for all stacks).
    IEnumerator AddStatusEffectPerformer(AddStatusEffect addStatusEffect)
    {  
        bool instigatorIsPlayer = addStatusEffect.instigatorIsPlayer;
        bool afflictedUnitIsPlayer = !instigatorIsPlayer;
        Dictionary<StatusEffect, int> map = afflictedUnitIsPlayer ? playerStatusEffects : enemyStatusEffects;
        Dictionary<StatusEffect, int> turnMap = afflictedUnitIsPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
        Dictionary<StatusEffect, int> appliedDurationMap = afflictedUnitIsPlayer ? playerStatusAppliedDuration : enemyStatusAppliedDuration;
        StatusEffect key = addStatusEffect.statusEffect;

        // Enemy does not use mana, so stun converts into random enemy card discard instead.
        if (key is StunStatusEffect && !afflictedUnitIsPlayer)
        {
            map[key] = map.TryGetValue(key, out int sEnemy) ? sEnemy + 1 : 1;
            if (!turnMap.ContainsKey(key))
            {
                int initialTurns = addStatusEffect.duration > 0 ? addStatusEffect.duration : key.duration;
                turnMap[key] = initialTurns;
                appliedDurationMap[key] = initialTurns;
            }

            List<Card> shownEnemyCards = EnemyHandView.Instance.GetShownCards();
            if (shownEnemyCards != null && shownEnemyCards.Count > 0)
            {
                Card randomCard = shownEnemyCards[Random.Range(0, shownEnemyCards.Count)];
                CardSystem.Instance.enemyDeck.Remove(randomCard);
                yield return StartCoroutine(EnemyHandView.Instance.RemoveEnemyCard(randomCard));
            }

            RefreshStatusUIGA refreshStatusAfterEnemyStunDiscard = new RefreshStatusUIGA(afflictedUnitIsPlayer);
            ActionSystem.Instance.AddReaction(refreshStatusAfterEnemyStunDiscard);
            yield return null;
            yield break;
        }

        int durationToApply = addStatusEffect.duration > 0 ? addStatusEffect.duration : key.duration;

        if (key is RuneStatusEffect)
        {
            // Reapplying a rune refreshes it to one active instance with the latest duration.
            map[key] = 1;
            turnMap[key] = durationToApply;
            appliedDurationMap[key] = durationToApply;
        }
        else
        {
            map[key] = map.TryGetValue(key, out int s) ? s + 1 : 1;

            if (!turnMap.ContainsKey(key))
            {
                turnMap[key] = durationToApply;
                appliedDurationMap[key] = durationToApply;
            }
        }

        if (key is VunerableStatusEffect || key is StunStatusEffect || key is RuneStatusEffect)
        {
            bool consumeDuration = key is VunerableStatusEffect || key is StunStatusEffect ? false : true;
            key.PerformStatusEffects(this, map[key], afflictedUnitIsPlayer, consumeDuration);
        }

        // Queued last: runs after PerformStatusEffects (StunEffectGA, VunerableGA, …) added above.
        RefreshStatusUIGA refreshStatusAfterApply = new RefreshStatusUIGA(afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(refreshStatusAfterApply);
        yield return null;
    }

    public Dictionary<StatusEffect, int> GetStacksMap(bool afflictedUnitIsPlayer)
    {
        return afflictedUnitIsPlayer ? playerStatusEffects : enemyStatusEffects;
    }

    public Dictionary<StatusEffect, int> GetTurnMap(bool afflictedUnitIsPlayer)
    {
        return afflictedUnitIsPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
    }

    public StatusUI GetStatusUI(bool afflictedUnitIsPlayer)
    {
        return afflictedUnitIsPlayer ? playerStatusUI : enemyStatusUI;
    }
}
