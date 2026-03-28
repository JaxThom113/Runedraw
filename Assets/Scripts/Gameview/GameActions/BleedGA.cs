using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedGA : GameAction
{
    public int damage;
    public int duration;
    public bool isPlayer;
    public StatusEffect statusEffect;

    public BleedGA(int damage, int duration, bool isPlayer, StatusEffect statusEffect)
    {
        this.damage = damage;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
    }
}
