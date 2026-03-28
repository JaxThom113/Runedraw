using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSystem : Singleton<PoisonSystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PoisonGA>(PoisonPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PoisonGA>();
    }

    public void RefreshBothSides()
    {
        RefreshStatusUI(true);
        RefreshStatusUI(false);
    }

    public void RefreshStatusUI(bool afflictedUnitIsPlayer)
    {
        if (StatusSystem.Instance == null) return;
        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI == null) return;

        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);
        GetStatusData(stacksMap, turnMap, out int poisonTicks, out int poisonStacks);
        statusUI.UpdatePoison(poisonTicks, poisonStacks);
    }

    private void GetStatusData(
        Dictionary<StatusEffect, int> stacksMap,
        Dictionary<StatusEffect, int> turnMap,
        out int ticks,
        out int stacks)
    {
        StatusEffect effect = null;
        stacks = 0;
        ticks = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key.GetType().Name == "PoisonStatusEffect")
            {
                effect = kvp.Key;
                stacks = kvp.Value;
                break;
            }
        }

        if (effect == null || stacks <= 0)
        {
            stacks = 0;
            ticks = 0;
            return;
        }

        ticks = turnMap.TryGetValue(effect, out int remaining) ? remaining : effect.duration;
    }

    private IEnumerator PoisonPerformer(PoisonGA poisonGA)
    {
        if (StatusSystem.Instance == null)
        {
            yield break;
        }

        bool afflictedUnitIsPlayer = poisonGA.isPlayer;
        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        bool damageHitsEnemy = !afflictedUnitIsPlayer;
        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);

        if (poisonGA.duration == 1)
        {
            if (!stacksMap.TryGetValue(poisonGA.statusEffect, out int stacks) || stacks <= 0)
            {
                turnMap.Remove(poisonGA.statusEffect);
            }
            else
            {
                int totalDamage = poisonGA.damage * stacks;
                ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, damageHitsEnemy));
                if (statusUI != null) statusUI.ScreenShake();
                stacksMap.Remove(poisonGA.statusEffect);
                turnMap.Remove(poisonGA.statusEffect);
                RefreshStatusUI(afflictedUnitIsPlayer);
            }
        }
        else
        {
            int turnsRemaining = turnMap.TryGetValue(poisonGA.statusEffect, out int turns) ? turns : poisonGA.duration;
            turnsRemaining--;
            turnMap[poisonGA.statusEffect] = turnsRemaining;
        }

        yield return null;
    }
}
