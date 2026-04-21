using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyShieldGA : GameAction
{
    public int magnitude;
    public bool isPlayer;

    public ApplyShieldGA(int magnitude, bool isPlayer)
    {
        this.magnitude = magnitude;
        this.isPlayer = isPlayer;
    }
}
