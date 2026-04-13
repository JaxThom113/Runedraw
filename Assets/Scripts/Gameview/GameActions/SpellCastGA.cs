using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCastGA : GameAction
{
   public int spellIndex; 
   public bool isPlayer;  
   public SpellCastGA(int spellIndex, bool isPlayer){  
    this.spellIndex = spellIndex;
    this.isPlayer = isPlayer;
   }
}
