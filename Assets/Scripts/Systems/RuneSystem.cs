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

        if (runeGA.duration > 0) { 
            if (!StatusSystem.Instance.TryGet(runeGA.statusEffect, afflictedUnitIsPlayer, out StatusData data) || data.magnitude <= 0) { 
                StatusSystem.Instance.RemoveStatus(runeGA.statusEffect, afflictedUnitIsPlayer);
                yield break;
            }

            // data.magnitude is stored in "inner effect units" (see RuneStatusEffect.Magnitude). Divide by the
            // inner's base magnitude to recover the cast count, so 2 rune casts of a DealDamage(5) rune fires
            // the inner effect twice. If the inner effect has no numeric magnitude, fall back to treating the
            // stored value as the raw fire count (old behavior).
            int baseInner = runeGA.effect != null ? runeGA.effect.Magnitude : 0;
            int fireCount = baseInner > 0
                ? Mathf.Max(1, data.magnitude / baseInner)
                : Mathf.Max(1, data.magnitude);
            for (int i = 0; i < fireCount; i++)
            {
                PerformEffectGA performEffectGA = new(runeGA.effect, effectTargetsPlayer);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }

            if (runeGA.consumeDuration)
            {
                StatusSystem.Instance.TickDuration(runeGA.statusEffect, afflictedUnitIsPlayer);
            }
        }
        else { 
            // Dispel path: remove unconditionally.
            StatusSystem.Instance.RemoveStatus(runeGA.statusEffect, afflictedUnitIsPlayer);
        }
       
        yield return null;
    }
}
