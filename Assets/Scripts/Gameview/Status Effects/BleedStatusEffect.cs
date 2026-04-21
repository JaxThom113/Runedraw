using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BleedStatusEffect : StatusEffect
{
    [FormerlySerializedAs("damage")]
    [SerializeField] int magnitude;
    public override int Magnitude => magnitude;
    public override StatusTurnPhase TurnPhase => StatusTurnPhase.Damage;

    public override void PerformStatusEffects(StatusSystem statusSystem, int magnitude, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (magnitude <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new BleedGA(magnitude, turnsRemaining, afflictedUnitIsPlayer, this));
    } 

    public override GameAction GetGameAction()
    {
        return new BleedGA(magnitude, duration, isPlayer, this);
    }

    protected override string GetBaseDescription()
    {
        return $"Apply {magnitude} Bleed for {duration} turns";
    }
}
