using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateCardEffect : Effect
{
    public override GameAction GetGameAction()
    {
        UltimateGA ultimateGA = new();  
        return ultimateGA;

    } 
    protected override string GetBaseDescription()
    {
        return $"Ultimate Ability";
    }
}

