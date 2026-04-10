using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonStatusEffect : StatusEffect
{
   [SerializeField] int damage;
   public int Damage => damage;
   public override StatusTurnPhase TurnPhase => StatusTurnPhase.Damage;

   public override void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer, bool consumeDuration = true)
   { 
      
      if (stacks <= 0) return;
      isPlayer = afflictedUnitIsPlayer;
      int turnsRemaining = statusSystem.GetStatusTurnRemaining(this, afflictedUnitIsPlayer);
      ActionSystem.Instance.AddReaction(new PoisonGA(Damage, turnsRemaining, afflictedUnitIsPlayer, this));
   }

   public override GameAction GetGameAction()  
   { 
    return new PoisonGA(damage, duration, isPlayer, this);
   } 

   public override string GetDescription()
   {
    return $"Apply {damage} Poison for {duration} turns";
   }
}
