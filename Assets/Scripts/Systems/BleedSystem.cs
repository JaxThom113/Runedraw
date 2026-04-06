using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedSystem : Singleton<BleedSystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<BleedGA>(BleedPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<BleedGA>();
    }

    public void RefreshBothSides()
    {
        RefreshStatusUI(true);
        RefreshStatusUI(false);
    }

    public void RefreshStatusUI(bool afflictedUnitIsPlayer)
    {
        if (StatusSystem.Instance == null) return;

        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);
        int bleedTicks = 0;
        int bleedStacks = 0;
        GetStatusData(stacksMap, turnMap, out bleedTicks, out bleedStacks);

        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI != null)
            statusUI.UpdateBleed(bleedTicks, bleedStacks);
    }

    private void GetStatusData(Dictionary<StatusEffect, int> stacksMap, Dictionary<StatusEffect, int> turnMap, out int ticks, out int stacks)
    {
        StatusEffect effect = null;
        stacks = 0;
        ticks = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key.GetType().Name == "BleedStatusEffect")
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

    private IEnumerator BleedPerformer(BleedGA bleedGA)
    {
        if (StatusSystem.Instance == null)
        {
            yield break;
        }

        bool afflictedUnitIsPlayer = bleedGA.isPlayer;
        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        bool damageHitsEnemy = !afflictedUnitIsPlayer;
        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);

        if (bleedGA.duration > 0)
        {
            if (!stacksMap.TryGetValue(bleedGA.statusEffect, out int stacks) || stacks <= 0)
            {
                turnMap.Remove(bleedGA.statusEffect);
            }
            else
            {
                int turnsRemaining = turnMap.TryGetValue(bleedGA.statusEffect, out int turns) ? turns : bleedGA.duration;
                turnsRemaining--;
                turnMap[bleedGA.statusEffect] = turnsRemaining;
                int totalDamage = bleedGA.damage * stacks;
                ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, damageHitsEnemy));
                if (damageHitsEnemy && EnemySystem.Instance.overworldEnemy != null)
                    EnemySystem.Instance.overworldEnemy.PlayBleedHitFlash();
                if (statusUI != null) statusUI.ScreenShake();
                if (turnsRemaining <= 0)
                {
                    stacksMap.Remove(bleedGA.statusEffect);
                    turnMap.Remove(bleedGA.statusEffect);
                }
                RefreshStatusUI(afflictedUnitIsPlayer);
            }
        }
        else
        {
            if (!stacksMap.TryGetValue(bleedGA.statusEffect, out int stacks) || stacks <= 0)
            {
                turnMap.Remove(bleedGA.statusEffect);
            }
            else
            {
                stacksMap.Remove(bleedGA.statusEffect);
            }
            turnMap.Remove(bleedGA.statusEffect);
            RefreshStatusUI(afflictedUnitIsPlayer);
        }

        yield return null;
    }
}
