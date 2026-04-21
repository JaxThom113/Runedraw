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

        GetActiveBleed(afflictedUnitIsPlayer, out StatusData data);

        StatusUI statusUI = StatusSystem.Instance.GetStatusUI(afflictedUnitIsPlayer);
        if (statusUI != null)
            statusUI.UpdateBleed(data.duration, data.magnitude);
    }

    private void GetActiveBleed(bool afflictedUnitIsPlayer, out StatusData data)
    {
        data = default;
        foreach (var kvp in StatusSystem.Instance.GetStatusMap(afflictedUnitIsPlayer))
        {
            if (kvp.Key is BleedStatusEffect && kvp.Value.magnitude > 0)
            {
                data = kvp.Value;
                return;
            }
        }
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

        if (bleedGA.duration > 0)
        {
            if (!StatusSystem.Instance.TryGet(bleedGA.statusEffect, afflictedUnitIsPlayer, out StatusData data) || data.magnitude <= 0)
            {
                StatusSystem.Instance.RemoveStatus(bleedGA.statusEffect, afflictedUnitIsPlayer);
                yield break;
            }

            // data.magnitude is the pre-aggregated per-tick damage under the unified magnitude model.
            int totalDamage = data.magnitude;
            ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, damageHitsEnemy));
            if (damageHitsEnemy && EnemySystem.Instance.overworldEnemy != null)
                EnemySystem.Instance.overworldEnemy.PlayBleedHitFlash();
            if (statusUI != null) statusUI.ShakeBleedIcon();

            StatusSystem.Instance.TickDuration(bleedGA.statusEffect, afflictedUnitIsPlayer);
            RefreshStatusUI(afflictedUnitIsPlayer);
        }
        else
        {
            // Dispel path: remove unconditionally.
            StatusSystem.Instance.RemoveStatus(bleedGA.statusEffect, afflictedUnitIsPlayer);
            RefreshStatusUI(afflictedUnitIsPlayer);
        }

        yield return null;
    }
}
