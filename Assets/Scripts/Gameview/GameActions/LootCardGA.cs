using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootCardGA : GameAction
{
    public int amount; 
    public bool fromEnemy;

    public LootCardGA(int amount)
    {
        this.amount = amount;
        this.fromEnemy = false;
    }

    public LootCardGA(int amount, bool fromEnemy)
    {
        this.amount = amount;
        this.fromEnemy = fromEnemy;
    }
    
}
