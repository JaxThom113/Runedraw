using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameGA : GameAction
{ 
    public int maxMana;
    public EndGameGA(int maxMana)
    {
        this.maxMana = maxMana;
    }
}
