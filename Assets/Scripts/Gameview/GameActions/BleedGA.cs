using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedGA : GameAction
{
    public int magnitude;
    public int duration;
    public bool isPlayer;
    public StatusEffect statusEffect;

    public BleedGA(int magnitude, int duration, bool isPlayer, StatusEffect statusEffect)
    {
        this.magnitude = magnitude;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
    }
}
