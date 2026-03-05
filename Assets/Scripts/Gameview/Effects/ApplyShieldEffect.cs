using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyShieldEffect : Effect
{
    [SerializeField] public int shieldAmount;

    public override GameAction GetGameAction()
    {
        return new ApplyShieldGA(shieldAmount, isPlayer);
    } 
    public override string GetDescription()
    {
        return $"Apply {shieldAmount} shield";
    }
}
