using UnityEngine;

public class LootCardPickupGA : GameAction
{
    public bool fromEnemy { get; private set; }

    public LootCardPickupGA(bool fromEnemy)
    {
        this.fromEnemy = fromEnemy;
    }
}
