using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyShieldGA : GameAction
{
    public int Amount;
    public bool isPlayer;

    public ApplyShieldGA(int amount, bool isPlayer)
    {
        Amount = amount;
        this.isPlayer = isPlayer;
    }
}
