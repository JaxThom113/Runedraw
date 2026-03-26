using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class StatusEffect : Effect
{
   [SerializeField] public int duration;

   /// <summary>
   /// Turn-start tick for this status. Override when the effect needs runtime data (turns left, target side, etc.).
   /// Default does nothing — add overrides for Poison, Bleed, Stun, Vulnerable.
   /// </summary>
   /// <param name="afflictedUnitIsPlayer">True if the unit with this status is the player.</param>
   public virtual void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer) { }
}
