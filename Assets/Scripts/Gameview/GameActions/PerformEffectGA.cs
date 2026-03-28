using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect effect { get; set; }
    /// <summary>True if the player triggered this effect; false if the enemy did (play or play-when-drawn).</summary>
    public bool instigatorIsPlayer;

    public PerformEffectGA(Effect effect, bool instigatorIsPlayer)
    {
        this.effect = effect;
        this.instigatorIsPlayer = instigatorIsPlayer;
    }
}
