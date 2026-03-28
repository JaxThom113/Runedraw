using UnityEngine;

public enum StatusTurnPhase
{
   Damage,
   Effect
}

[System.Serializable]
public abstract class StatusEffect : Effect
{
   [SerializeField] public int duration; 
   [SerializeField] public StatusTurnPhase turnPhase;
   public virtual StatusTurnPhase TurnPhase => turnPhase;

   public virtual void PerformStatusEffects(StatusSystem statusSystem, int stacks, bool afflictedUnitIsPlayer, bool consumeDuration = true) { }
}
