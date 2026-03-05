using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawCardsEffect : Effect
{
    [SerializeField] public int drawAmount; 

    public override GameAction GetGameAction()
    { 
        DrawCardGA drawCardGA = new(drawAmount);
        return drawCardGA;
    } 
    public override string GetDescription()
    {
        return $"Draw {drawAmount} cards";
    }
}
