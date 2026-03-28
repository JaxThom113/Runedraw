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

        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        int totalAdditionalDamage = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key is not VunerableStatusEffect vunerableEffect) continue;
            if (kvp.Value <= 0) continue;
            totalAdditionalDamage += kvp.Value * vunerableEffect.Damage;
        }

        return totalAdditionalDamage;
    }

    public void RefreshStatusUI(bool afflictedUnitIsPlayer)
    {
        if (StatusSystem.Instance == null) return;
        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI == null) return;

        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);
        GetStatusData(stacksMap, turnMap, out int vunerableTicks, out int vunerableStacks);
        statusUI.UpdateVunerable(vunerableTicks, vunerableStacks);
    }

    private void GetStatusData(
        Dictionary<StatusEffect, int> stacksMap,
        Dictionary<StatusEffect, int> turnMap,
        out int ticks,
        out int stacks)
    {
        stacks = 0;
        ticks = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key.GetType().Name == "VunerableStatusEffect")
            {
                if (kvp.Value <= 0) continue;
                stacks += kvp.Value;
                int effectTicks = turnMap.TryGetValue(kvp.Key, out int remaining) ? remaining : kvp.Key.duration;
                if (effectTicks > ticks) ticks = effectTicks;
            }
        }
    }

    private IEnumerator VunerablePerformer(VunerableGA vunerableGA)
    {
        if (StatusSystem.Instance == null || DamageSystem.Instance == null)
        {
            yield break;
        }

        bool afflictedUnitIsPlayer = vunerableGA.isPlayer;
        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);

        List<StatusEffect> vunerableEffects = new List<StatusEffect>();
        int totalStacks = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key.GetType().Name != "VunerableStatusEffect") continue;
            if (kvp.Value <= 0) continue;
            vunerableEffects.Add(kvp.Key);
            totalStacks += kvp.Value;
        }

        if (totalStacks <= 0)
        {
            DamageSystem.Instance.additionalDamage = 0;
            yield break;
        }

        DamageSystem.Instance.additionalDamage = totalStacks * vunerableGA.damage;
        DamageSystem.Instance.additionalDamageAfflictsPlayer = afflictedUnitIsPlayer;

        if (!vunerableGA.consumeDuration)
        {
            yield return null;
            yield break;
        }

        foreach (StatusEffect effect in vunerableEffects)
        {
            int turnsRemaining = turnMap.TryGetValue(effect, out int turns) ? turns : effect.duration;
            turnsRemaining--;
            if (turnsRemaining <= 0)
            {
                stacksMap.Remove(effect);
                turnMap.Remove(effect);
            }
            else
            {
                turnMap[effect] = turnsRemaining;
            }
        }

        yield return null;
    }
}
