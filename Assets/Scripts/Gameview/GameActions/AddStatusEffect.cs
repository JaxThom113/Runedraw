using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffect : GameAction
{
    public StatusEffect statusEffect;
    public int duration;
    /// <summary>True if the player played/applied this; false if the enemy did. Afflicted unit is the opposite side.</summary>
    public bool instigatorIsPlayer;

    public AddStatusEffect(StatusEffect statusEffect, int duration, bool instigatorIsPlayer)
    {
        this.statusEffect = statusEffect;
        this.duration = duration;
        this.instigatorIsPlayer = instigatorIsPlayer;
    }
}
