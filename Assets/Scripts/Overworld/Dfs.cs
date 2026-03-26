using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dfs
{
    public static List<List<int>> DfsMazeGenerate(List<List<int>> grid, int gridSize, int y = 0, int x = 0)
    {
        // randomize directions
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 2),
            new Vector2Int(0, -2),
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0)
        };
        Shuffle(directions);

        foreach (Vector2Int dir in directions)
        {
            int dy = y + dir.y;
            int dx = x + dir.x;

            // check bounds
            if (dy >= 0 && dy < gridSize && dx >= 0 && dx < gridSize)
            {
                if (grid[dy][dx] == 1) // unvisited
                {
                    // carve path between current and neighbor
                    grid[y + dir.y / 2][x + dir.x / 2] = 0;
                    grid[dy][dx] = 0;

                    DfsMazeGenerate(grid, gridSize, dy, dx);
                }
            }
        }

        return grid;
    }

    private static void Shuffle<T>(List<T> list)
    {
        // Fisher-Yates shuffle
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
