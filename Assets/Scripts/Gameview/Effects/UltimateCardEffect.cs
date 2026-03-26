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
    public override string GetDescription()
    {
        return $"Ultimate Ability";
    }
}

