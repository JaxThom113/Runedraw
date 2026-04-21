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

        GetActiveStun(afflictedUnitIsPlayer, out StatusData data);
        statusUI.UpdateStun(data.duration, data.magnitude);
    }

    private void GetActiveStun(bool afflictedUnitIsPlayer, out StatusData data)
    {
        data = default;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is StunStatusEffect && kvp.Value.magnitude > 0)
            {
                data = kvp.Value;
                return;
            }
        }
    }

    // Sum magnitude across every StunStatusEffect SO (supports multiple stun variants sharing the side).
    private static int GetTotalStunMagnitude(bool afflictedUnitIsPlayer)
    {
        int total = 0;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is StunStatusEffect && kvp.Value.magnitude > 0)
                total += kvp.Value.magnitude;
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

        if (!afflictedUnitIsPlayer)
        { 
            if (!StatusSystem.Instance.TryGet(stunEffectGA.statusEffect, afflictedUnitIsPlayer, out StatusData enemyData) || enemyData.magnitude <= 0)
            {
                StatusSystem.Instance.RemoveStatus(stunEffectGA.statusEffect, afflictedUnitIsPlayer);
                yield break;
            }

            int discardsToApply = enemyData.magnitude;
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

            if (stunEffectGA.consumeDuration)
            {
                StatusSystem.Instance.TickDuration(stunEffectGA.statusEffect, afflictedUnitIsPlayer);
                StatusUI enemyStatusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
                if (enemyStatusUI != null) enemyStatusUI.ShakeStunIcon();
            }

            RefreshStatusUI(false);
            yield break;
        }

        if (!StatusSystem.Instance.TryGet(stunEffectGA.statusEffect, afflictedUnitIsPlayer, out StatusData playerData) || playerData.magnitude <= 0)
        { 
            StatusSystem.Instance.RemoveStatus(stunEffectGA.statusEffect, afflictedUnitIsPlayer);
            ManaSystem.Instance?.SetAdditionalMana(GetTotalStunMagnitude(afflictedUnitIsPlayer));
            yield break;
        }

        // Apply the mana penalty BEFORE ticking duration. On the final stunned turn, TickDuration
        // removes the entry from the map; if we summed after the tick, GetTotalStunMagnitude would
        // return 0 and the player would skip paying on the very turn the stun is still supposed to
        // be in effect (causing N duration to only charge for N-1 turns).
        int totalStun = GetTotalStunMagnitude(afflictedUnitIsPlayer);
        ManaSystem.Instance?.SetAdditionalMana(totalStun > 0 ? totalStun : 0);

        if (stunEffectGA.consumeDuration)
        {
            StatusSystem.Instance.TickDuration(stunEffectGA.statusEffect, afflictedUnitIsPlayer);
            StatusUI playerStatusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
            if (playerStatusUI != null) playerStatusUI.ShakeStunIcon();
        }

        RefreshStatusUI(true);
        ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());

        yield return null;
    }
}
