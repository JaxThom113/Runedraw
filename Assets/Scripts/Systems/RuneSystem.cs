using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneSystem : Singleton<RuneSystem>
{ 
    void OnEnable() { 
        ActionSystem.AttachPerformer<RuneGA>(RunePerformer);
    }
    void OnDisable() { 
        ActionSystem.DetachPerformer<RuneGA>();
    }       
    private IEnumerator RunePerformer(RuneGA runeGA){ 
        if (StatusSystem.Instance == null) { 
            yield break;
        } 
        bool afflictedUnitIsPlayer = runeGA.afflictedUnitIsPlayer; 
        bool effectTargetsPlayer = runeGA.appliedToSelf ? afflictedUnitIsPlayer : !afflictedUnitIsPlayer;
        if (runeGA.statusEffect.TurnPhase == StatusTurnPhase.Damage)
        {
            effectTargetsPlayer = !effectTargetsPlayer;
        }
        Dictionary<StatusEffect, int> stacksMap = StatusSystem.Instance.GetStacksMap(afflictedUnitIsPlayer); 
        Dictionary<StatusEffect, int> turnMap = StatusSystem.Instance.GetTurnMap(afflictedUnitIsPlayer);  
        if (runeGA.duration > 0) { 
            if (!stacksMap.TryGetValue(runeGA.statusEffect, out int stacks) || stacks <= 0) { 
                turnMap.Remove(runeGA.statusEffect);
            }
            else { 
                int turnsRemaining = turnMap.TryGetValue(runeGA.statusEffect, out int turns) ? turns : runeGA.duration;
                turnsRemaining--; 
                turnMap[runeGA.statusEffect] = turnsRemaining; 
                PerformEffectGA performEffectGA = new(runeGA.effect, effectTargetsPlayer);
                ActionSystem.Instance.AddReaction(performEffectGA);
                if (turnsRemaining <= 0) { 
                    stacksMap.Remove(runeGA.statusEffect);
                    turnMap.Remove(runeGA.statusEffect);
                }
            }
        }
        else { 
            if (!stacksMap.TryGetValue(runeGA.statusEffect, out int stacks) || stacks <= 0) { 
                turnMap.Remove(runeGA.statusEffect);
            }
            else { 
                stacksMap.Remove(runeGA.statusEffect);
            } 
             turnMap.Remove(runeGA.statusEffect);
        }
       
        yield return null;
    }
}
