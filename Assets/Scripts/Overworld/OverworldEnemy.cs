using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    public GameObject SpriteGameObject; 
    public EnemySO enemyData;  
    public void UpdateEnemy(EnemySO enemyData)
    {
        SpriteGameObject.GetComponent<SpriteRenderer>().sprite = enemyData.entityIcon;
        this.enemyData = enemyData;
    }
}
