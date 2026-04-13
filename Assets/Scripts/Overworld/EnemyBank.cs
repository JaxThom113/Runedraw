using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBank : MonoBehaviour
{
    public List<EnemySO> enemies;

    private bool spawnWizardLizard;

    public void SetupSpawnWeights()
    {
        // this bool being true will make a CHANCE that an enemy will spawn
        spawnWizardLizard = Random.value > 0.3f;
    }

    public EnemySO GetRandomEnemy()
    {
        int index;

        if (spawnWizardLizard)
        {
            index = 2;
            spawnWizardLizard = false;
        }
        else
        {
            index = Random.Range(0, enemies.Count-1);
        }

        return enemies[index];
    }
}
