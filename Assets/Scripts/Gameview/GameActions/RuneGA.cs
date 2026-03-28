using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneGA : GameAction
{
    public Effect effect; 
    public int duration;  
    public bool afflictedUnitIsPlayer;
    public bool appliedToSelf;
    public StatusEffect statusEffect;
    public RuneGA(Effect effect, int duration, bool afflictedUnitIsPlayer, bool appliedToSelf, StatusEffect statusEffect)
    {
        this.effect = effect;
        this.duration = duration;
        this.afflictedUnitIsPlayer = afflictedUnitIsPlayer;
        this.appliedToSelf = appliedToSelf;
        this.statusEffect = statusEffect;
    }
}
