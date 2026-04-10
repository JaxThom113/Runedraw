using UnityEngine;

public class VunerableStatusEffect : StatusEffect
{
    [SerializeField] int damage;
    public int Damage => damage;
    public override void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (stacks <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new VunerableGA(Damage, turnsRemaining, afflictedUnitIsPlayer, this, consumeDuration));
    }

    public override GameAction GetGameAction()
    {
        return new VunerableGA(damage, duration, isPlayer, this);
    }

    public override string GetDescription()
    {
        return $"Apply {damage} Vulnerable for {duration} turns";
    }
}
