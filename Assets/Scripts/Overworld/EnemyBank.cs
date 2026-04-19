using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBank : MonoBehaviour
{
    public List<EnemySO> enemies;
    public List<EnemySO> rareEnemies;

    private int rareRemaining;

    public void InitializeRareEnemyCount()
    {
        // always spawn 0 to 2 rare enemies (if rare enemies are specified)
        if (rareEnemies.Count > 0)
            rareRemaining = Random.Range(0, 3);
        else
            rareRemaining = 0;
    }

    public EnemySO GetRandomEnemy()
    {
        if (rareRemaining > 0)
        {
            // because of the way this is set up, rare enemies will always spawn
            // on the top of the maze
            rareRemaining--;
            return rareEnemies[Random.Range(0, rareEnemies.Count)];
        }

        return enemies[Random.Range(0, enemies.Count)];
    }
}
