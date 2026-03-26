using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : Singleton<StatusSystem>
{ 
    // How many stacks of each status (per unit). Not the same as turn countdown — see *TurnRemaining.
    Dictionary<StatusEffect, int> enemyStatusEffects = new Dictionary<StatusEffect, int>();
    Dictionary<StatusEffect, int> playerStatusEffects = new Dictionary<StatusEffect, int>();
    // Turns remaining until this keyed effect procs (e.g. poison burst). Separate from stack count.
    Dictionary<StatusEffect, int> enemyStatusTurnRemaining = new Dictionary<StatusEffect, int>();
    Dictionary<StatusEffect, int> playerStatusTurnRemaining = new Dictionary<StatusEffect, int>();
    private StatusUI statusUI;   
    public StatusUI playerStatusUI;
    public StatusUI enemyStatusUI;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<PoisonGA>(PoisonPerformer);
        ActionSystem.AttachPerformer<ApplyStatusGA>(ApplyStatusEffectPerformer);
        ActionSystem.AttachPerformer<AddStatusEffect>(AddStatusEffectPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PoisonGA>();
        ActionSystem.DetachPerformer<ApplyStatusGA>();
        ActionSystem.DetachPerformer<AddStatusEffect>();
    }

    public int GetStatusTurnRemaining(StatusEffect effect, bool afflictedUnitIsPlayer)
    { 
        
        Dictionary<StatusEffect, int> turnMap = afflictedUnitIsPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
        Debug.Log("Turns remaining for effect: " + effect.GetType().Name + " " + turnMap.Count);
        if (turnMap.TryGetValue(effect, out int turnsRemaining))
            return turnsRemaining;
        return effect.duration;
    }

    // Called once at the start of the turn: each status overrides PerformStatusEffects (e.g. PoisonGA with runtime turns).
    IEnumerator ApplyStatusEffectPerformer(ApplyStatusGA applyStatusGA)
    { 
        
        foreach (KeyValuePair<StatusEffect, int> kvp in playerStatusEffects)
        {
            int stacks = kvp.Value;
            if (stacks <= 0) continue;
            kvp.Key.PerformStatusEffects(this, stacks, true);  
            
            // UI is refreshed after all status performers run.
        }
        foreach (KeyValuePair<StatusEffect, int> kvp in enemyStatusEffects)
        {
            int stacks = kvp.Value;
            if (stacks <= 0) continue;
            kvp.Key.PerformStatusEffects(this, stacks, false);
        }

        // Refresh poison UI for both sides.
        statusUI = playerStatusUI;
        RefreshPoisonUI(true);
        statusUI = enemyStatusUI;
        RefreshPoisonUI(false);
        yield return null;
    }  

    void RefreshPoisonUI(bool afflictedUnitIsPlayer)
    {
        Debug.Log("RefreshPoisonUI");
        if (statusUI == null) return;
        
        // Status UI currently supports a single poison display, so this reads the player's poison.
        StatusEffect poisonEffect = null;
        int poisonStacks = 0; 

        Dictionary<StatusEffect, int> stacksMap = afflictedUnitIsPlayer ? playerStatusEffects : enemyStatusEffects;
        Dictionary<StatusEffect, int> turnMap = afflictedUnitIsPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;

        foreach (var kvp in stacksMap)
        { 
            Debug.Log("Checking status effect: " + kvp.Key + " " + kvp.Value);
            if (kvp.Key.GetType().Name == "PoisonStatusEffect")
            { 
                Debug.Log("SUCCESS: Found poison effect: " + kvp.Key.GetType().Name);
                poisonEffect = kvp.Key;
                poisonStacks = kvp.Value;
                break;
            }
        }

        if (poisonEffect == null || poisonStacks <= 0)
        {
            statusUI.SetPoisonVisible(false);
            return;
        }

        int poisonTicks = turnMap.TryGetValue(poisonEffect, out int ticks) ? ticks : poisonEffect.duration;
        statusUI.UpdatePoison(poisonTicks, poisonStacks);
    }
    // Advances poison timer; at 0 queues damage. Turn countdown lives in *StatusTurnRemaining, not in stack map.
    IEnumerator PoisonPerformer(PoisonGA poisonGA)
    { 
        // Ensure we shake/update the correct side's UI.
        statusUI = poisonGA.isPlayer ? playerStatusUI : enemyStatusUI;
        
        // DealDamageGA second arg: true = enemy takes damage, false = player takes damage.
        bool damageHitsEnemy = !poisonGA.isPlayer;
        Dictionary<StatusEffect, int> stacksMap = poisonGA.isPlayer ? playerStatusEffects : enemyStatusEffects;
        Dictionary<StatusEffect, int> turnMap = poisonGA.isPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
        if (poisonGA.duration == 0)
        {  
            Debug.Log("PoisonGA duration is 0: " + poisonGA.statusEffect.GetType().Name);
            if (!stacksMap.TryGetValue(poisonGA.statusEffect, out int stacks) || stacks <= 0)
            {
                turnMap.Remove(poisonGA.statusEffect);
            }
            else
            {
                int totalDamage = poisonGA.damage * stacks;
                ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, damageHitsEnemy)); 
                statusUI.ScreenShake();
                stacksMap.Remove(poisonGA.statusEffect);
                turnMap.Remove(poisonGA.statusEffect); 
                RefreshPoisonUI(poisonGA.isPlayer);
            }
        }
        else
        { 
            int turnsRemaining = 0; 
            Debug.Log("Turn Map Count: " + turnMap.Count);
            if(turnMap.TryGetValue(poisonGA.statusEffect, out int turns)) {  
                Debug.Log("Succesfully got turns remaining: " + turns);
                turnsRemaining = turns;
            }
            else
            {
                Debug.Log("Failed to get turns remaining, set to default duration: " + poisonGA.statusEffect.GetType().Name);
                turnsRemaining = poisonGA.duration;
            }
            
            turnsRemaining--;  
            
            Debug.Log("Turns remaining: " + turnsRemaining);
            turnMap[poisonGA.statusEffect] = turnsRemaining;
        }

        yield return null;
    }
    // Card adds stacks; turn countdown is set only on first application (same timer for all stacks).
    IEnumerator AddStatusEffectPerformer(AddStatusEffect addStatusEffect)
    { 
        statusUI = !addStatusEffect.isPlayer ? playerStatusUI : enemyStatusUI; 

        Dictionary<StatusEffect, int> map = !addStatusEffect.isPlayer ? playerStatusEffects : enemyStatusEffects; 
        Dictionary<StatusEffect, int> turnMap = !addStatusEffect.isPlayer ? playerStatusTurnRemaining : enemyStatusTurnRemaining;
        StatusEffect key = addStatusEffect.statusEffect;
        map[key] = map.TryGetValue(key, out int s) ? s + 1 : 1; 
      
        if (!turnMap.ContainsKey(key))
        {
            int initialTurns = addStatusEffect.duration > 0 ? addStatusEffect.duration : key.duration;
            turnMap[key] = initialTurns;
        }  

        // addStatusEffect.isPlayer indicates the source owner; this converts to afflicted-unit side.
        RefreshPoisonUI(!addStatusEffect.isPlayer);
        yield return null;
    }
    
}
