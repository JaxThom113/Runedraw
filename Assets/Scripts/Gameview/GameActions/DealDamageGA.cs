using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction
{
    public int magnitude;
    public bool isPlayer;
    public DealDamageGA(int magnitude, bool isPlayer) { 
        this.magnitude = magnitude;
        this.isPlayer = isPlayer;
    }
}
