using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class RuneStatusEffect : StatusEffect
{
    [SerializeReference, SR(typeof(Effect))]
    [SerializeField] Effect effect;
    [SerializeField] bool ApplyToEnemy = true;
    // Per-cast stack delta. Defers to the inner effect's magnitude so stacking rune cards grows the stored
    // magnitude in "inner effect units" (e.g. +5 per cast when wrapping DealDamage 5), matching how Poison
    // et al. grow their stacks. Falls back to the serialized field only when the inner effect is null or has
    // no numeric magnitude (e.g. SpecialEffect, UltimateCardEffect).
    [SerializeField] int magnitude = 1;
    public override int Magnitude => effect != null && effect.Magnitude > 0 ? effect.Magnitude : magnitude;

    // Cast-time context captured by CardSystem / EnemySystem when the rune card is played. StatusSystem
    // reads these on every rune tick so it can fire the matching spellcast VFX + card sound without needing
    // a live Card reference (which it doesn't have during deferred status ticks).
    // Default elementIndex = 1 ("neutral") matches ShaderSystem's spellIndex convention.
    [System.NonSerialized] public int cachedElementIndex = 1;
    [System.NonSerialized] public AudioClip cachedSound;
    [System.NonSerialized] public bool cachedCasterIsPlayer;
    [System.NonSerialized] public bool hasCapturedCastContext;

    public void CaptureCastContext(Card card, bool casterIsPlayer)
    {
        if (card != null)
        {
            cachedElementIndex = card.GetElementIndex();
            cachedSound = card.sound;
        }
        cachedCasterIsPlayer = casterIsPlayer;
        hasCapturedCastContext = true;
    }
    
    public override void PerformStatusEffects(StatusSystem statusSystem, int magnitude, bool afflictedUnitIsPlayer, bool consumeDuration = true)
    {
        if (magnitude <= 0) return;
        isPlayer = afflictedUnitIsPlayer;
        int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer); 

        ActionSystem.Instance.AddReaction(new RuneGA(effect, magnitude, turnsRemaining, afflictedUnitIsPlayer, ApplyToEnemy, this, consumeDuration));
    }
    
    public override GameAction GetGameAction()
    {
        return new RuneGA(effect, magnitude, duration, isPlayer, ApplyToEnemy, this);
    }

    protected override string GetBaseDescription()
    {
        return $"Once per {duration} turns, {effect.GetDescription()} ";
    }
}
