using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneGA : GameAction
{
    public Effect effect; 
    public int magnitude;
    public int duration;  
    public bool afflictedUnitIsPlayer;
    public bool appliedToSelf;
    public StatusEffect statusEffect;
    public bool consumeDuration;
    public RuneGA(Effect effect, int magnitude, int duration, bool afflictedUnitIsPlayer, bool appliedToSelf, StatusEffect statusEffect, bool consumeDuration = true)
    {
        this.effect = effect;
        this.magnitude = magnitude;
        this.duration = duration;
        this.afflictedUnitIsPlayer = afflictedUnitIsPlayer;
        this.appliedToSelf = appliedToSelf;
        this.statusEffect = statusEffect;
        this.consumeDuration = consumeDuration;
    }
}
