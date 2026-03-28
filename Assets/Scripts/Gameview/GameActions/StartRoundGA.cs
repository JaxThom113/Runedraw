using UnityEngine;

public class StartRoundGA : GameAction
{
    public int playerDrawAmount;
    public int enemyDrawAmount;

    public StartRoundGA(int playerDrawAmount, int enemyDrawAmount)
    {
        this.playerDrawAmount = playerDrawAmount;
        this.enemyDrawAmount = enemyDrawAmount;
    }
}
