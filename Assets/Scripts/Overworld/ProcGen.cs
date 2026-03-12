using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This script generates a 15x15 matrix to define a level.
It also returns the top and obttom edges to define the start/end points of the level.

Example:

0 1 1 1 1 1 1 1 1 1 1 1 1 1 1 

2 0 3 0 0 0 0 0 0 0 0 0 0 0 3 
2 1 1 1 1 1 1 1 1 1 1 1 1 1 0 
2 2 2 2 2 1 2 2 2 1 4 3 0 0 0 
3 1 1 1 2 1 2 1 2 1 1 1 1 1 1 
0 0 4 1 2 2 3 1 2 2 2 1 3 2 2 
0 1 1 1 1 1 0 1 1 1 2 1 2 1 2 
0 1 3 2 2 1 0 0 3 1 3 1 2 1 2 
1 1 2 1 2 1 1 1 0 1 2 1 2 1 2 
2 2 2 1 2 2 2 1 4 1 2 2 2 1 2 
2 1 1 1 1 1 2 1 1 1 1 1 0 1 2 
2 1 3 0 0 1 2 2 2 2 3 1 4 1 2 
2 1 0 1 0 1 1 1 1 1 2 1 1 1 2 
2 1 4 1 0 0 0 1 2 2 2 1 2 2 3 
2 1 1 1 0 1 0 1 2 1 1 1 2 1 0 
2 2 0 1 0 1 0 0 2 2 2 2 2 1 0 

1 0 1 1 1 1 1 1 1 1 1 1 1 1 1 

*/

public static class ProcGen
{
    /*
        Grid matrix
            0 = floor
            1 = wall
            2 = correct path
            3 = enemy
            4 = interactable
    */
    private static List<List<int>> grid = new List<List<int>>();
    private static List<int> bottomEdge;
    private static List<int> topEdge;

    // maze parameters
    private const int GRID_SIZE = 15;
    private static Vector2Int start = new Vector2Int(0, 0);
    private static Vector2Int end = new Vector2Int(0, GRID_SIZE - 1);
    private static List<Vector2Int> correctPath = new List<Vector2Int>();

    /*
        Return functions
    */

    public static List<List<int>> GetLevel()
    {
        return grid;
    }

    public static List<int> GetTopEdge()
    {
        return topEdge;
    }

    public static List<int> GetBottomEdge()
    {
        return bottomEdge;
    }

    /*
        Main generation function
    */

    public static void GenerateLevel()
    {
        // Step #1: Initialize grid full of walls (1's)
        CreateMaze();

        // Step #2: Flood Fill / DFS maze generation
        GenerateMaze();

        // Step #3: Select random start / finish, edit edge tiles
        StartFinish();

        // Step #4: Use A* pathfinding to find that most direct route start -> finish
        GeneratePath();

        // Step #5: Place enemies along correct path
        SpawnEnemiesAlongPath();

        // Step #6: Place loot at dead ends
        SpawnLootDeadEnds();

        // Step #7: Place enemies in front of loot / in branching paths
        SpawnEnemiesBranchingPaths();

        // print results to console for debugging
        GridDebug();
    }

    /*
        Helper functions for GenerateLevel()
    */

    private static void CreateMaze()
    {
        // fill grid with walls
        grid = new List<List<int>>();
        for (int y = 0; y < GRID_SIZE; y++)
        {
            grid.Add(new List<int>());
            for (int x = 0; x < GRID_SIZE; x++)
            {
                // 0 = floor, 1 = wall
                grid[y].Add(1);
            }
        }
    }

    private static void GenerateMaze()
    {
        grid = Dfs.DfsMazeGenerate(grid, GRID_SIZE, 0, 0);
    }

    private static void StartFinish()
    {
        // find open tiles for entrance/exit (after maze is generated)
        List<int> bottomOpenTiles = new List<int>();
        List<int> topOpenTiles = new List<int>();

        for (int x = 0; x < GRID_SIZE; x++)
        {
            if (grid[x][GRID_SIZE - 1] == 0)
                topOpenTiles.Add(x);
            if (grid[x][0] == 0)
                bottomOpenTiles.Add(x);
        }

        // select random start
        if (bottomOpenTiles.Count > 0)
            start.x = bottomOpenTiles[Random.Range(0, bottomOpenTiles.Count)];
        else
            start.x = 0;

        // select random end
        if (topOpenTiles.Count > 0)
            end.x = topOpenTiles[Random.Range(0, topOpenTiles.Count)];
        else
            end.x = 0;

        // set up edges
        bottomEdge = new List<int>();
        topEdge = new List<int>();

        for (int x = 0; x < GRID_SIZE; x++)
        {
            bottomEdge.Add(1);
            topEdge.Add(1);
        }

        bottomEdge[start.x] = 0;
        topEdge[end.x] = 0;
    }

    private static void GeneratePath()
    {
        List<Vector2Int> path = AStar.AStarFindPath(grid, GRID_SIZE, start, end);
        correctPath = path;

        if (path != null && path.Count > 0)
        {
            // Highlight the path on the floor tilemap
            foreach (Vector2Int pos in path)
            {
                // 2 = correct path
                grid[pos.x][pos.y] = 2;
            }
        }
        else
        {
            Debug.LogWarning("No path found between start and end!");
        }
    }

    private static void SpawnEnemiesAlongPath()
    {     
        // have one enemy spawn for each unit of distance along the correct path
        int step = Random.Range(8, 16);

        for (int i = step; i < correctPath.Count; i += step)
        {
            step = Random.Range(8, 16);

            // 3 = enemy
            grid[correctPath[i].x][correctPath[i].y] = 3;
        }
    }

    private static void SpawnLootDeadEnds()
    {
        int[] dy = { -1, 1, 0, 0 }; // up, down
        int[] dx = { 0, 0, -1, 1 }; // left, right

        // search through maze matrix and look for dead ends to place loot
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                // ignore cells with walls, the correct path, and enemies
                if (grid[y][x] == 1 || grid[y][x] == 2 || grid[y][x] == 3)
                    continue;
                
                // up, down, left, right
                List<int> neighbors = new List<int>();

                // check the adjacent cells
                for (int i = 0; i < 4; i++)
                {
                    int ny = y + dy[i];
                    int nx = x + dx[i];

                    // bounds check to avoid errors
                    if (ny >= 0 && ny < GRID_SIZE && nx >= 0 && nx < GRID_SIZE)
                    {
                        neighbors.Add(grid[ny][nx]);
                    }
                }

                // if a 2 is in neighbors, ignore putting loot on this cell
                if (neighbors.Contains(2) || neighbors.Contains(3))
                    continue;

                // if there is one 0 and three 1s, this cell is a dead end, so add loot
                int zeros = 0, ones = 0;
                foreach (int n in neighbors)
                {
                    if (n == 0) 
                        zeros++;
                    else if (n == 1) 
                        ones++;
                }

                if (zeros == 1 && ones == 3)
                {
                    // 4 = interactable
                    grid[y][x] = 4; // add a 4 to represent this interactable instance
                }
            }
        }
    }

    private static void SpawnEnemiesBranchingPaths()
    {
        List<Vector2Int> openFloorTiles = new List<Vector2Int>();

        // create a list of all of the open floor tiles (0s) and place a random range of enemies in random positions from that list
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                if (grid[y][x] == 0)
                    openFloorTiles.Add(new Vector2Int(y, x));
            }
        }

        // number of enemies to be placed on branching paths, changes depending on how many open tiles there are
        int numEnemies;

        if (openFloorTiles.Count >= 80)
            numEnemies = Random.Range(5, 11);
        else if (openFloorTiles.Count >= 60)
            numEnemies = Random.Range(4, 9);
        else if (openFloorTiles.Count >= 40)
            numEnemies = Random.Range(3, 7);
        else if (openFloorTiles.Count >= 20)
            numEnemies = Random.Range(2, 5);
        else
            numEnemies = Random.Range(1, 3);
        
        // place enemies randomly on branching paths (from enemy bank like along path)
        for (int i = 0; i < numEnemies; i++)
        {
            int randPos = Random.Range(0, openFloorTiles.Count);

            // 3 = enemy
            grid[openFloorTiles[randPos].x][openFloorTiles[randPos].y] = 3;
        }
    }

    private static void GridDebug()
    {
        string topEdgeDebug = "";
        for (int x = 0; x < GRID_SIZE; x++)
        {
            if (topEdge[x] == 1)
                topEdgeDebug += "1 ";
            else
                topEdgeDebug += "0 ";
        }

        string gridDebug = "";
        for (int y = GRID_SIZE - 1; y >= 0; y--)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                if (grid[x][y] == 4)
                    gridDebug += "4 ";
                else if (grid[x][y] == 3)
                    gridDebug += "3 ";
                else if (grid[x][y] == 2)
                    gridDebug += "2 ";
                else if (grid[x][y] == 1)
                    gridDebug += "1 ";
                else
                    gridDebug += "0 ";
            }
            gridDebug += "\n";
        }

        string bottomEdgeDebug = "";
        for (int x = 0; x < GRID_SIZE; x++)
        {
            if (bottomEdge[x] == 1)
                bottomEdgeDebug += "1 ";
            else
                bottomEdgeDebug += "0 ";
        }


        Debug.Log(topEdgeDebug);
        Debug.Log(gridDebug);
        Debug.Log(bottomEdgeDebug);
    }
}