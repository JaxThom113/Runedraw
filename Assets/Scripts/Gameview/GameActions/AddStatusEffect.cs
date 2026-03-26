using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffect : GameAction
{
    public StatusEffect statusEffect;
    public int duration;
    public bool isPlayer;

    public AddStatusEffect(StatusEffect statusEffect, int duration, bool isPlayer)
    {
        this.statusEffect = statusEffect;
        this.duration = duration;
        this.isPlayer = isPlayer;
    }
}
