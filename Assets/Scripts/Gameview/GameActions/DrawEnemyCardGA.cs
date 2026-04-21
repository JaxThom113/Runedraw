using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEnemyCardGA : GameAction
{
    public int magnitude { get; set; }

    /// <summary>
    /// When true (enemy card Draw effect): peek at hand[enemyTurnCount + 1] and rebuild the in-battle
    /// enemy draw pile from that hand WITHOUT advancing enemyTurnCount. The enemy's next natural turn
    /// will still land on that same hand, so the drawn cards match what they'll play next turn.
    /// Start-round draws leave this false (they use the current turn's hand).
    /// </summary>
    public bool UseNextHand { get; set; }

    public DrawEnemyCardGA(int magnitude, bool useNextHand = false)
    {
        this.magnitude = magnitude;
        UseNextHand = useNextHand;
    }
}
