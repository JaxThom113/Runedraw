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

        GetActivePoison(afflictedUnitIsPlayer, out StatusEffect effect, out StatusData data);

        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI != null)
            statusUI.UpdatePoison(data.duration, data.magnitude);

        SyncEnemyPoisonVisual(afflictedUnitIsPlayer, effect, data);
    }

    private void GetActivePoison(bool afflictedUnitIsPlayer, out StatusEffect effect, out StatusData data)
    {
        effect = null;
        data = default;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is PoisonStatusEffect && kvp.Value.magnitude > 0)
            {
                effect = kvp.Key;
                data = kvp.Value;
                return;
            }
        }
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

        if (!StatusSystem.Instance.TryGet(poisonGA.statusEffect, afflictedUnitIsPlayer, out StatusData data) || data.magnitude <= 0)
        {
            StatusSystem.Instance.RemoveStatus(poisonGA.statusEffect, afflictedUnitIsPlayer);
            yield break;
        }

        // Poison accumulates magnitude across its duration; the damage only fires on the last tick.
        // Under the unified magnitude model, data.magnitude already represents the total damage to deal.
        if (poisonGA.duration == 1)
        {
            int totalDamage = data.magnitude;
            ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, damageHitsEnemy));
            if (statusUI != null) statusUI.ShakePoisonIcon();
            StatusSystem.Instance.RemoveStatus(poisonGA.statusEffect, afflictedUnitIsPlayer);
        }
        else
        {
            StatusSystem.Instance.TickDuration(poisonGA.statusEffect, afflictedUnitIsPlayer);
            if (statusUI != null) statusUI.ShakePoisonIcon();
        }

        RefreshStatusUI(afflictedUnitIsPlayer);
        yield return null;
    }

    private void SyncEnemyPoisonVisual(bool afflictedUnitIsPlayer, StatusEffect effect, StatusData data)
    {
        if (afflictedUnitIsPlayer)
            return;

        OverworldEnemy overworldEnemy = EnemySystem.Instance.overworldEnemy;
        if (overworldEnemy == null)
            return;

        // Constant "first color of green" — pass (duration, duration) so the shader resolves to 1/duration intensity.
        int turnsForVisual = data.magnitude > 0 ? Mathf.Max(1, data.duration) : 0;
        int maxForVisual = data.magnitude > 0 ? Mathf.Max(1, data.duration) : 0;
        overworldEnemy.SetPoisonTurnsRemaining(turnsForVisual, maxForVisual);

        if (overworldEnemy.poisonParticles != null)
            overworldEnemy.poisonParticles.SetActive(data.magnitude > 0);
    }
}
