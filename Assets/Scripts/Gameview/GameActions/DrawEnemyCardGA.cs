using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEnemyCardGA : GameAction
{
    public int Amount { get; set; }

    /// <summary>
    /// When true (enemy card Draw effect): advance enemy turn index, rebuild the in-battle enemy draw pile from that hand, then animate draws. Start-round draws leave this false.
    /// </summary>
    public bool AdvanceToNextHandBeforeDraw { get; set; }

    public DrawEnemyCardGA(int amount, bool advanceToNextHandBeforeDraw = false)
    {
        Amount = amount;
        AdvanceToNextHandBeforeDraw = advanceToNextHandBeforeDraw;
    }
}
