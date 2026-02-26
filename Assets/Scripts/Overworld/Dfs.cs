using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dfs
{
    public static List<List<int>> DfsMazeGenerate(List<List<int>> grid, int gridSize, int x, int y)
    {
        // randomize directions
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 2),  // up
            new Vector2Int(0, -2), // down
            new Vector2Int(2, 0),  // right
            new Vector2Int(-2, 0)  // left
        };
        Shuffle(directions);

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // check bounds
            if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
            {
                if (grid[nx][ny] == 1) // unvisited
                {
                    // carve path between current and neighbor
                    grid[x + dir.x / 2][y + dir.y / 2] = 0;
                    grid[nx][ny] = 0;

                    DfsMazeGenerate(grid, gridSize, nx, ny);
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
