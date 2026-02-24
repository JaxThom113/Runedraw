using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyGA : GameAction
{
     public EnemyView enemyView {get; private set;} 

     public KillEnemyGA(EnemyView enemyView)
     {
        this.enemyView = enemyView;
     }
}
