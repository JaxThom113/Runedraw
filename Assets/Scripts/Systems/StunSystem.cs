using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunSystem : Singleton<StunSystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<StunEffectGA>(StunPerformer);
        ActionSystem.SubscribeReaction<RefillManaGA>(RefillManaPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<StunEffectGA>();
        ActionSystem.UnsubscribeReaction<RefillManaGA>(RefillManaPostReaction, ReactionTiming.POST);
    }

    private void RefillManaPostReaction(RefillManaGA refillManaGA)
    {
        ClearPlayerStun();
    }

    private void ClearPlayerStun()
    { 
        ManaSystem.Instance?.SetAdditionalMana(0);
        RefreshStatusUI(true);
        ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());
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
        GetStatusData(stacksMap, turnMap, out int stunTicks, out int stunStacks);
        statusUI.UpdateStun(stunTicks, stunStacks);
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
            if (kvp.Key.GetType().Name == "StunStatusEffect")
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

        int remainingTurns;
        if (turnMap.TryGetValue(effect, out remainingTurns))
        {
            ticks = remainingTurns;
        }
        else
        {
            ticks = effect.duration;
        }
    }

   
    private static int GetTotalStunStacks(Dictionary<StatusEffect, int> stacksMap)
    { 
    
        int total = 0;
        foreach (var kvp in stacksMap)
        {
            if (kvp.Key.GetType().Name != "StunStatusEffect") continue;
            if (kvp.Value > 0) total += kvp.Value;
        }
        return total;
    }

    private IEnumerator StunPerformer(StunEffectGA stunEffectGA)
    { 
        if (StatusSystem.Instance == null)
        { 
            yield break;
        }

        bool afflictedUnitIsPlayer = stunEffectGA.isPlayer;
        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer);
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);

        if (!afflictedUnitIsPlayer)
        { 
            if (!stacksMap.TryGetValue(stunEffectGA.statusEffect, out int enemyStacks) || enemyStacks <= 0)
            {
                turnMap.Remove(stunEffectGA.statusEffect);
                yield break;
            }

            int discardsToApply = enemyStacks;
            for (int i = 0; i < discardsToApply; i++)
            {
                List<Card> shownEnemyCards = EnemyHandView.Instance.GetShownCards();
                if (shownEnemyCards == null || shownEnemyCards.Count <= 0)
                {
                    break;
                }

                Card randomCard = shownEnemyCards[Random.Range(0, shownEnemyCards.Count)];
                CardSystem.Instance.enemyDeck.Remove(randomCard);
                yield return StartCoroutine(EnemyHandView.Instance.RemoveEnemyCard(randomCard));
            }

            int turnsRemaining = turnMap.TryGetValue(stunEffectGA.statusEffect, out int turns) ? turns : stunEffectGA.duration;
            if (stunEffectGA.consumeDuration)
            {
                turnsRemaining--;
                if (turnsRemaining <= 0)
                {
                    stacksMap.Remove(stunEffectGA.statusEffect);
                    turnMap.Remove(stunEffectGA.statusEffect);
                }
                else
                {
                    turnMap[stunEffectGA.statusEffect] = turnsRemaining;
                }
            }

            RefreshStatusUI(false);
            yield break;
        }

        if (!stacksMap.TryGetValue(stunEffectGA.statusEffect, out int stacks) || stacks <= 0)
        { 
            turnMap.Remove(stunEffectGA.statusEffect);
            ManaSystem.Instance?.SetAdditionalMana(GetTotalStunStacks(stacksMap));
            yield break;
        }

        int playerTurnsRemaining = turnMap.TryGetValue(stunEffectGA.statusEffect, out int playerTurns) ? playerTurns : stunEffectGA.duration;
        if (stunEffectGA.consumeDuration)
        {
            playerTurnsRemaining--;
            if (playerTurnsRemaining <= 0)
            {
                stacksMap.Remove(stunEffectGA.statusEffect);
                turnMap.Remove(stunEffectGA.statusEffect);
            }
            else
            {
                turnMap[stunEffectGA.statusEffect] = playerTurnsRemaining;
            }
        }
         if(GetTotalStunStacks(stacksMap) > 0) { 
            ManaSystem.Instance?.SetAdditionalMana(GetTotalStunStacks(stacksMap));
         }
        else
        {
            ManaSystem.Instance?.SetAdditionalMana(0);
        }

        RefreshStatusUI(true);
        ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());

        yield return null;
    }
}
