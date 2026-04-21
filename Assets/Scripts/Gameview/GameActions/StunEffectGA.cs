using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffectGA : GameAction
{
    public int magnitude;
    public int duration;
    public bool isPlayer; // afflicted unit is player
    public bool consumeDuration;
    public StatusEffect statusEffect;

    public StunEffectGA(int magnitude, int duration, bool isPlayer, StatusEffect statusEffect, bool consumeDuration = true)
    {
        this.magnitude = magnitude;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.consumeDuration = consumeDuration;
        this.statusEffect = statusEffect;
    }
}
