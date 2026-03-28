using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : Singleton<EffectSystem>
{
    //Hold perfomer for performeffecr game action 

    void OnEnable() 
    { 
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    } 
    void OnDisable() 
    { 
        ActionSystem.DetachPerformer<PerformEffectGA>();
    } 
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        // Status effects must go through AddStatusEffect so stacks/turn maps and StatusUI stay in sync.
        // (Enemy "play when drawn" cards used to only queue e.g. StunEffectGA via GetGameAction, so UI never updated.)
        if (performEffectGA.effect is StatusEffect statusEffect)
        {
            ActionSystem.Instance.AddReaction(new AddStatusEffect(statusEffect, statusEffect.duration, performEffectGA.instigatorIsPlayer));
            yield return null;
            yield break;
        }

        Effect effect = performEffectGA.effect;
        bool savedIsPlayer = effect.isPlayer;
        GameAction effectAction;
        try
        {
            // Who played the card (or triggered play-when-drawn), not the serialized SO field alone.
            effect.isPlayer = performEffectGA.instigatorIsPlayer;
            effectAction = effect.GetGameAction();
        }
        finally
        {
            effect.isPlayer = savedIsPlayer;
        }

        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
}
