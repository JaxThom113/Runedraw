using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldSystem : Singleton<OverworldSystem>
{ 
    public EnemyView enemyView;  
    public EnemySO enemyData; 
    private void Start() { 
        SetupEnemy();
    }
    private void SetupEnemy() { 
        enemyView.Setup(enemyData, null);
    } 
    public EnemySO GetCurrentEnemy() { 
        //FIXME: HAVE MULTIPLE ENEMIES IN THE FUTURE
        return enemyData;
    }
}
