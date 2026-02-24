using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBank : MonoBehaviour
{
    public List<EnemySO> enemies; 

    public EnemySO GetRandomEnemy()
    {
        return enemies[Random.Range(0, enemies.Count)];
    } 
    
}
