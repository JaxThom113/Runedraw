using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VunerableSystem : Singleton<VunerableSystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<VunerableGA>(VunerablePerformer);
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<VunerableGA>();
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
    }

    private void StartRoundPreReaction(StartRoundGA startRoundGA)
    {
        ResetAdditionalDamage();
        RefreshBothSides();
    }

    public void ResetAdditionalDamage()
    {
        if (DamageSystem.Instance == null) return;
        DamageSystem.Instance.additionalDamage = 0;
    }

    public void RefreshBothSides()
    {
        RefreshStatusUI(true);
        RefreshStatusUI(false);
    }

    public int GetTotalAdditionalDamage(bool afflictedUnitIsPlayer)
    {
        if (StatusSystem.Instance == null) return 0;

        int totalAdditionalDamage = 0;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is not VunerableStatusEffect) continue;
            if (kvp.Value.magnitude <= 0) continue;
            // Magnitude is already the pre-aggregated damage bonus under the unified magnitude model.
            totalAdditionalDamage += kvp.Value.magnitude;
        }

        return totalAdditionalDamage;
    }

    public void RefreshStatusUI(bool afflictedUnitIsPlayer)
    {
        if (StatusSystem.Instance == null) return;
        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI == null) return;

        GetAggregateVunerable(afflictedUnitIsPlayer, out int ticks, out int stacks);
        statusUI.UpdateVunerable(ticks, stacks);
    }

    // Aggregate magnitude across all vulnerable SO instances; report the longest duration as "ticks remaining."
    private void GetAggregateVunerable(bool afflictedUnitIsPlayer, out int ticks, out int stacks)
    {
        stacks = 0;
        ticks = 0;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is not VunerableStatusEffect) continue;
            if (kvp.Value.magnitude <= 0) continue;
            stacks += kvp.Value.magnitude;
            if (kvp.Value.duration > ticks) ticks = kvp.Value.duration;
        }
    }

    private IEnumerator VunerablePerformer(VunerableGA vunerableGA)
    {
        if (StatusSystem.Instance == null || DamageSystem.Instance == null)
        {
            yield break;
        }

        bool afflictedUnitIsPlayer = vunerableGA.isPlayer;

        List<StatusEffect> vunerableEffects = new List<StatusEffect>();
        int totalStacks = 0;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is not VunerableStatusEffect) continue;
            if (kvp.Value.magnitude <= 0) continue;
            vunerableEffects.Add(kvp.Key);
            totalStacks += kvp.Value.magnitude;
        }

        if (totalStacks <= 0)
        {
            DamageSystem.Instance.additionalDamage = 0;
            yield break;
        }

        // totalStacks already sums StatusData.magnitude across all vulnerable entries — no multiply needed.
        DamageSystem.Instance.additionalDamage = totalStacks;
        DamageSystem.Instance.additionalDamageAfflictsPlayer = afflictedUnitIsPlayer;

        if (!vunerableGA.consumeDuration)
        {
            yield return null;
            yield break;
        }

        foreach (StatusEffect effect in vunerableEffects)
        {
            StatusSystem.Instance.TickDuration(effect, afflictedUnitIsPlayer);
        }

        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI != null) statusUI.ShakeVunerableIcon();

        yield return null;
    }
}
