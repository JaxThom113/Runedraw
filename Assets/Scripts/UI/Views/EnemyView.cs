using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnemyView : EntityView
{ 
    [SerializeField] public TMP_Text enemyNameText;
    public void Setup(EnemySO enemyData, OverworldEnemy overworldEnemy) {  
        firstSetup = true;
        SetupBase(enemyData); 
        enemyNameText.text = enemyData.entityName;  
        if (overworldEnemy != null)
        {
            this.overworldEnemy = overworldEnemy;
        }
        
    } 
}
