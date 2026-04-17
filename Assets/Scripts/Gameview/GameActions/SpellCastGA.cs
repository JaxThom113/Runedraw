using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCastGA : GameAction
{
   public int spellIndex; 
   public bool isPlayer;
   public bool hasSpecialEffect;
   public SpellCastGA(int spellIndex, bool isPlayer, bool hasSpecialEffect = false){  
    this.spellIndex = spellIndex;
    this.isPlayer = isPlayer;
    this.hasSpecialEffect = hasSpecialEffect;
   }
}
