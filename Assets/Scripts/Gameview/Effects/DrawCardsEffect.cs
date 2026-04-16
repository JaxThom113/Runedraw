using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawCardsEffect : Effect
{
    [SerializeField] public int drawAmount; 

    public override GameAction GetGameAction()
    { 
        if(isPlayer)
        {
             DrawCardGA drawCardGA = new(drawAmount);
            return drawCardGA;
        } 
        else
        {
            // Advance + deck rebuild happen in DrawEnemyCardPerformer when the action runs (not here),
            // so enemyDeck matches the "next hand" data before any DrawFront/Add cycles.
            DrawEnemyCardGA drawEnemyCardGA = new(drawAmount, advanceToNextHandBeforeDraw: true);
            return drawEnemyCardGA;
        }
       
    } 
    public override string GetDescription()
    {
        return $"Draw {drawAmount} cards";
    }
}
