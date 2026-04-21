using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunStatusEffect : StatusEffect
{
    [SerializeField] int magnitude = 1;
    public override int Magnitude => magnitude;

    public override void PerformStatusEffects(StatusSystem statusSystem, int magnitude, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (magnitude <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new StunEffectGA(magnitude, turnsRemaining, afflictedUnitIsPlayer, this, consumeDuration));
    }

    public override GameAction GetGameAction()
    {
        return new StunEffectGA(magnitude, duration, isPlayer, this);
    }

    protected override string GetBaseDescription()
    {
        return $"Apply Stun for {duration} turns";
    }
}
