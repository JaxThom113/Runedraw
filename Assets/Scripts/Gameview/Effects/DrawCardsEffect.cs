using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class DrawCardsEffect : Effect
{
    [FormerlySerializedAs("drawAmount")]
    [SerializeField] public int magnitude; 
    public override int Magnitude => magnitude;

    public override GameAction GetGameAction()
    { 
        if(isPlayer)
        {
             DrawCardGA drawCardGA = new(magnitude);
            return drawCardGA;
        } 
        else
        {
            // Peek rebuild happens in DrawEnemyCardPerformer when the action runs (not here), using
            // hand[enemyTurnCount + 1] without mutating the counter — so these draws match next turn's hand.
            DrawEnemyCardGA drawEnemyCardGA = new(magnitude, useNextHand: true);
            return drawEnemyCardGA;
        }
       
    } 
    protected override string GetBaseDescription()
    {
        return $"Draw {magnitude} cards";
    }
}
