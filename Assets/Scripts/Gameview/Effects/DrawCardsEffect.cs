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
        else{  
            EnemySystem.Instance.EnemyTurnHandler();
            DrawEnemyCardGA drawEnemyCardGA = new(drawAmount);
            return drawEnemyCardGA;
        }
       
    } 
    public override string GetDescription()
    {
        return $"Draw {drawAmount} cards";
    }
}
