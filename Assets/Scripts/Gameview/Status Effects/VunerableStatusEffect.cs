using UnityEngine;
using UnityEngine.Serialization;

public class VunerableStatusEffect : StatusEffect
{
    [FormerlySerializedAs("damage")]
    [SerializeField] int magnitude;
    public override int Magnitude => magnitude;
    public override void PerformStatusEffects(StatusSystem statusSystem, int magnitude, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (magnitude <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new VunerableGA(magnitude, turnsRemaining, afflictedUnitIsPlayer, this, consumeDuration));
    }

    public override GameAction GetGameAction()
    {
        return new VunerableGA(magnitude, duration, isPlayer, this);
    }

    protected override string GetBaseDescription()
    {
        return $"Apply {magnitude} Vulnerable for {duration} turns";
    }
}
