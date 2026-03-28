using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This script generates a 15x15 grid to define a level, along with top and botttom edges 
to define the start/end points of the level.

Indexing is (y, x), and coordinates are done as if in 4th quadrant. 

Example:

0 1 1 1 1 1 1 1 1 1 1 1 1 1 1 (topEdge)

2 0 3 0 0 0 0 0 0 0 0 0 0 0 3 (grid)
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

1 0 1 1 1 1 1 1 1 1 1 1 1 1 1 (bottomEdge)

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
    private static List<List<int>> grid;
    private static List<int> bottomEdge;
    private static List<int> topEdge;

    // maze parameters
    private const int GRID_SIZE = 15;
    private static int startX;
    private static int endX;
    private static List<Vector2Int> correctPath;

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

        // fill edges with walls
        bottomEdge = new List<int>();
        topEdge = new List<int>();
        for (int x = 0; x < GRID_SIZE; x++)
        {
            bottomEdge.Add(1);
            topEdge.Add(1);
        }

        // set up correct path list
        correctPath = new List<Vector2Int>();
    }

    private static void GenerateMaze()
    {
        grid = Dfs.DfsMazeGenerate(grid, GRID_SIZE);
    }

    private static void StartFinish()
    {
        // find open tiles for entrance/exit (after maze is generated)
        List<int> openBottomTiles = new List<int>();
        List<int> openTopTiles = new List<int>();

        for (int x = 0; x < GRID_SIZE; x++)
        {
            if (grid[GRID_SIZE - 1][x] == 0)
                openBottomTiles.Add(x);
            if (grid[0][x] == 0)
                openTopTiles.Add(x);
        }

        // select random start
        if (openBottomTiles.Count > 0)
            startX = openBottomTiles[Random.Range(0, openBottomTiles.Count)];
        else
            return;

        // select random end
        if (openTopTiles.Count > 0)
            endX  = openTopTiles[Random.Range(0, openTopTiles.Count)];
        else
            return;

        bottomEdge[startX] = 0;
        topEdge[endX] = 0;
    }

    private static void GeneratePath()
    {
        Vector2Int startPos = new Vector2Int(GRID_SIZE - 1, startX);
        Vector2Int endPos = new Vector2Int(0, endX);

        correctPath = AStar.AStarFindPath(grid, GRID_SIZE, startPos, endPos);

        if (correctPath != null && correctPath.Count > 0)
        {
            // highlight correct path on the floor tilemap
            foreach (Vector2Int pos in correctPath)
            {
                // 2 = correct path
                grid[pos.x][pos.y] = 2;
            }
        }
        else
        {
            return;
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
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

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
                foreach (Vector2Int dir in directions)
                {
                    int dy = y + dir.y;
                    int dx = x + dir.x;

                    // bounds check to avoid errors
                    if (dy >= 0 && dy < GRID_SIZE && dx >= 0 && dx < GRID_SIZE)
                    {
                        neighbors.Add(grid[dy][dx]);
                    }
                }

                // if a 2 is in neighbors, ignore putting loot on this cell
                if (neighbors.Contains(2) || neighbors.Contains(3))
                    continue;

                // if there is one 0 and three 1s, this cell is a dead end, so add loot
                int numWalls = 0, numFloors = 0;
                foreach (int n in neighbors)
                {
                    if (n == 0) 
                        numFloors++;
                    else if (n == 1) 
                        numWalls++;
                }

                if (numFloors == 1 && numWalls == 3)
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
        
    }
}