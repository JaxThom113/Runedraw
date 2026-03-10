using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBank : MonoBehaviour
{
    public List<EnemySO> enemies;
    private List<EnemySO> drawPile = new List<EnemySO>();

    public EnemySO GetRandomEnemy()
    {
        if (drawPile.Count == 0)
            RefillDrawPile();

        if (drawPile.Count == 0)
            return null;

        int index = Random.Range(0, drawPile.Count);
        EnemySO chosen = drawPile[index];
        drawPile.RemoveAt(index);
        return chosen;
    }

    public void RefillDrawPile()
    {
        drawPile.Clear();
        if (enemies != null && enemies.Count > 0)
            drawPile.AddRange(enemies);
    }
}
