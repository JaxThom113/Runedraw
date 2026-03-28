using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class RuneStatusEffect : StatusEffect
{
    [SerializeReference, SR(typeof(Effect))]
    [SerializeField] Effect effect;
    [SerializeField] bool ApplyToEnemy = true;
    
    public override void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (stacks <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
        ActionSystem.Instance.AddReaction(new RuneGA(effect, turnsRemaining, afflictedUnitIsPlayer, ApplyToEnemy, this));
    }
    
    public override GameAction GetGameAction()
    {
        return new RuneGA(effect, duration, isPlayer, ApplyToEnemy, this);
    }

    public override string GetDescription()
    {
        return $"Once per {duration} turns, {effect.GetDescription()} ";
    }
}
