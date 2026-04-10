using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunStatusEffect : StatusEffect
{
    public override void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (stacks <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new StunEffectGA(turnsRemaining, afflictedUnitIsPlayer, this, consumeDuration));
    }

    public override GameAction GetGameAction()
    {
        return new StunEffectGA(duration, isPlayer, this);
    }

    public override string GetDescription()
    {
        return $"Apply Stun for {duration} turns";
    }
}
