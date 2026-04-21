using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Per-status runtime state. Stored once per (status, side) pair.
// magnitude: stack count. Monotonically non-decreasing over the life of an entry; additions only until expiration.
// duration: turns remaining. First-apply value is frozen — reapplication never refreshes it. Tick happens in subsystem performers.
public struct StatusData
{
    public int magnitude;
    public int duration;
}

public class StatusSystem : Singleton<StatusSystem>
{
    // One dictionary per side. Entries are added/removed atomically — no desync risk.
    Dictionary<StatusEffect, StatusData> playerStatus = new Dictionary<StatusEffect, StatusData>();
    Dictionary<StatusEffect, StatusData> enemyStatus = new Dictionary<StatusEffect, StatusData>();
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

    // --- Public facade: all reads/writes to status state must go through these. ---

    public bool TryGet(StatusEffect effect, bool afflictedUnitIsPlayer, out StatusData data)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        return map.TryGetValue(effect, out data);
    }

    // Adds to magnitude if entry exists; creates a fresh entry with durationIfNew otherwise.
    // Duration is never refreshed on reapply — first-apply duration wins.
    public void AddMagnitude(StatusEffect effect, bool afflictedUnitIsPlayer, int amountToAdd, int durationIfNew)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        if (map.TryGetValue(effect, out StatusData data))
        {
            data.magnitude += amountToAdd;
            map[effect] = data;
        }
        else
        {
            map[effect] = new StatusData { magnitude = amountToAdd, duration = durationIfNew };
        }
    }

    public void RemoveStatus(StatusEffect effect, bool afflictedUnitIsPlayer)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        map.Remove(effect);
    }

    // Decrement duration by 1; remove the entry entirely if it hits 0.
    public void TickDuration(StatusEffect effect, bool afflictedUnitIsPlayer)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        if (!map.TryGetValue(effect, out StatusData data)) return;
        data.duration--;
        if (data.duration <= 0)
            map.Remove(effect);
        else
            map[effect] = data;
    }

    // Legacy wrapper preserved for existing callers (StatusEffect subclasses) — returns remaining turns, or the SO's declared duration as fallback.
    public int GetStatusTurnRemaining(StatusEffect effect, bool afflictedUnitIsPlayer)
    {
        if (TryGet(effect, afflictedUnitIsPlayer, out StatusData data))
            return data.duration;
        return effect.duration;
    }

    // Read-only iteration access for subsystems that aggregate across multiple SOs of the same type
    // (e.g. StunSystem summing magnitudes, VunerableSystem computing total additional damage).
    // Callers must not mutate the returned dictionary directly — use the facade methods.
    public Dictionary<StatusEffect, StatusData> GetStatusMap(bool afflictedUnitIsPlayer)
    {
        return afflictedUnitIsPlayer ? playerStatus : enemyStatus;
    }

    public StatusUI GetStatusUI(bool afflictedUnitIsPlayer)
    {
        return afflictedUnitIsPlayer ? playerStatusUI : enemyStatusUI;
    }

    // --- Turn-phase performers ---

    // Called once at the start of the turn before shield clear so existing shield can absorb DOT.
    IEnumerator ApplyStatusDamagePerformer(ApplyStatusDamageGA applyStatusDamageGA)
    {
        VunerableSystem.Instance?.ResetAdditionalDamage();
        ApplyStatusEffectsForSide(true, StatusTurnPhase.Damage);
        ApplyStatusEffectsForSide(false, StatusTurnPhase.Damage);
        VunerableSystem.Instance?.ResetAdditionalDamage();
        yield return null;
    }

    // Called once at the start of the turn after shield clear for non-damage status effects.
    IEnumerator ApplyStatusEffectPerformer(ApplyStatusGA applyStatusGA)
    {
        VunerableSystem.Instance?.ResetAdditionalDamage();
        ApplyStatusEffectsForSide(true, StatusTurnPhase.Effect);
        ApplyStatusEffectsForSide(false, StatusTurnPhase.Effect);
        VunerableSystem.Instance?.ResetAdditionalDamage();

        RefreshStatusUIGA refreshStatusUIGA = new RefreshStatusUIGA();
        ActionSystem.Instance.AddReaction(refreshStatusUIGA);
        yield return null;
    }

    // Called after draws so delayed control/status effects can use the latest visible hand state.
    IEnumerator ApplyLateStatusEffectPerformer(ApplyStatusEffectGA applyStatusEffectGA)
    {
        ApplyDeferredStatusEffectsForSide(true);
        ApplyDeferredStatusEffectsForSide(false);

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
        playerStatus.Clear();
        enemyStatus.Clear();

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

    private void ApplyStatusEffectsForSide(bool afflictedUnitIsPlayer, StatusTurnPhase turnPhase)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        foreach (KeyValuePair<StatusEffect, StatusData> kvp in map)
        {
            int magnitude = kvp.Value.magnitude;
            if (magnitude <= 0) continue;
            if (kvp.Key is VunerableStatusEffect || kvp.Key is StunStatusEffect) continue;
            if (kvp.Key.TurnPhase != turnPhase) continue;
            VunerableSystem.Instance?.ResetAdditionalDamage(); // vulnerable does not affect status damage

            if (kvp.Key is RuneStatusEffect rune)
                RuneCastVFX(rune);

            kvp.Key.PerformStatusEffects(this, magnitude, afflictedUnitIsPlayer);
        }
    }

    private void RuneCastVFX(RuneStatusEffect rune)
    {
        if (rune == null) return;
        // Only the spellcast VFX replays each tick. The card SFX is intentionally
        // *not* replayed: it already plays once through the normal card-play path
        // (CardSystem) on initial cast, and repeating it every turn is noisy.
        ActionSystem.Instance?.AddReaction(new SpellCastGA(rune.cachedElementIndex, rune.cachedCasterIsPlayer));
    }

    private void ApplyDeferredStatusEffectsForSide(bool afflictedUnitIsPlayer)
    {
        Dictionary<StatusEffect, StatusData> map = afflictedUnitIsPlayer ? playerStatus : enemyStatus;
        foreach (KeyValuePair<StatusEffect, StatusData> kvp in map)
        {
            int magnitude = kvp.Value.magnitude;
            if (magnitude <= 0) continue;
            if (kvp.Key is not VunerableStatusEffect && kvp.Key is not StunStatusEffect) continue;
            kvp.Key.PerformStatusEffects(this, magnitude, afflictedUnitIsPlayer);
        }
    }

    // Card adds magnitude. On first application, duration is set; reapplication never refreshes duration.
    // Rune no longer has a special refresh-on-reapply path — it follows the same rule as everything else.
    IEnumerator AddStatusEffectPerformer(AddStatusEffect addStatusEffect)
    {
        bool instigatorIsPlayer = addStatusEffect.instigatorIsPlayer;
        bool afflictedUnitIsPlayer = !instigatorIsPlayer;
        StatusEffect key = addStatusEffect.statusEffect;
        int durationToApply = addStatusEffect.duration > 0 ? addStatusEffect.duration : key.duration;

        if (key is RuneStatusEffect)
        {
            GameData.RunesPlayed++;
        }

        // Per-application magnitude comes from the SO (e.g. PoisonStatusEffect.magnitude = 5 adds +5 per cast).
        // Stun/Rune default to 1 via their SO, Poison/Bleed/Vulnerable carry their per-application damage amount.
        int magnitudeToAdd = key.Magnitude;

        // Enemy does not use mana, so stun converts into random enemy card discard instead.
        if (key is StunStatusEffect && !afflictedUnitIsPlayer)
        {
            AddMagnitude(key, afflictedUnitIsPlayer, magnitudeToAdd, durationToApply);
            RefreshAllStatusUINow(afflictedUnitIsPlayer);

            int discardsToApply = Mathf.Max(1, magnitudeToAdd);
            for (int i = 0; i < discardsToApply; i++)
            {
                List<Card> shownEnemyCards = EnemyHandView.Instance.GetShownCards();
                if (shownEnemyCards == null || shownEnemyCards.Count <= 0) break;

                Card randomCard = shownEnemyCards[Random.Range(0, shownEnemyCards.Count)];
                CardSystem.Instance.enemyDeck.Remove(randomCard);
                yield return StartCoroutine(EnemyHandView.Instance.RemoveEnemyCard(randomCard));
            }

            RefreshAllStatusUINow(afflictedUnitIsPlayer);
            yield return null;
            yield break;
        }

        AddMagnitude(key, afflictedUnitIsPlayer, magnitudeToAdd, durationToApply);

        // Immediate synchronous UI refresh so the applied status is visible in the same frame as the card resolution,
        // not deferred behind other reactions queued by the card (SpendMana, PerformEffect, etc.).
        RefreshAllStatusUINow(afflictedUnitIsPlayer);

        // On-cast immediate fire for vulnerable/stun only — effect fires now with the new magnitude, but duration is not consumed.
        // Runes intentionally do NOT fire on cast; they only trigger at the start of subsequent turns.
        if (key is VunerableStatusEffect || key is StunStatusEffect)
        {
            TryGet(key, afflictedUnitIsPlayer, out StatusData data);
            key.PerformStatusEffects(this, data.magnitude, afflictedUnitIsPlayer, consumeDuration: false);
        }
        yield return null;
    }

    // Synchronous UI fan-out across all status subsystems for the given side. Use when the UI must reflect
    // a status change in the same frame rather than waiting for a queued RefreshStatusUIGA to drain.
    private void RefreshAllStatusUINow(bool afflictedUnitIsPlayer)
    {
        PoisonSystem.Instance?.RefreshStatusUI(afflictedUnitIsPlayer);
        BleedSystem.Instance?.RefreshStatusUI(afflictedUnitIsPlayer);
        VunerableSystem.Instance?.RefreshStatusUI(afflictedUnitIsPlayer);
        StunSystem.Instance?.RefreshStatusUI(afflictedUnitIsPlayer);
    }
}
