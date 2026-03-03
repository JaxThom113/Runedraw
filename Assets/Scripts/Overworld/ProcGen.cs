using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class ProcGen : MonoBehaviour
{
    [Header("Generation Settings")]
    [Range(0, 1000)]
    public int seed = 0;
    [TextArea(15, 20)]
    public string gridDebug;

    [Header("Tilemap References")]
    public Tilemap wallTilemap;
    public Tilemap edgeTilemap;
    public Tilemap floorTilemap;
    public TileBase wallTile;
    public TileBase edgeTile;
    public TileBase floorTile;
    public TileBase highlightFloorTile;

    [Header("Entity References")]
    public GameObject player;
    public GameObject enemy;    
    public GameObject interactable;
    public EnemyBank enemyBank;

    [Header("3D Elements")]
    public GameObject wallCube; 
    public GameObject edgeCube; 

    /*
        Grid matrix
            0 = floor
            1 = wall
            2 = correct path
            3 = enemy
            4 = interactable
    */
    private List<List<int>> grid = new List<List<int>>();

    // top/bottom edges
    private List<int> bottomEdge;
    private List<int> topEdge;

    // maze parameters
    private const int gridSize = 15;
    private Vector2Int start = new Vector2Int(0, 0);
    private Vector2Int end = new Vector2Int(0, gridSize - 1);
    private List<Vector2Int> correctPath = new List< Vector2Int>();

    // containers for wall/enemy/interactable prefabs
    private GameObject wallsContainer; 
    private GameObject enemyContainer;
    private GameObject interactableContainer;

    void Start()
    {
        Random.InitState(seed);
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateLevel();
        }
    }

    void UpdateGridDebug()
    {
        // display the grid in a text box in the inspector panel
        gridDebug = "";
        for (int y = gridSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // add squares to a textbox to make the grid visible in inspector
                if (grid[x][y] == 4)
                    gridDebug += "▤ ";
                else if (grid[x][y] == 3)
                    gridDebug += "▧ ";
                else if (grid[x][y] == 2)
                    gridDebug += "▣ ";
                else if (grid[x][y] == 1)
                    gridDebug += "□ ";
                else
                    gridDebug += "■ ";
            }
            gridDebug += "\n";
        }
    }

    /*
        Main generation function
    */

    public void GenerateLevel()
    {
        // Step #1: Initialize grid full of walls (1's)
        CreateMaze();

        // Step #2: Flood Fill / DFS maze generation
        GenerateMaze();

        // Step #3: Select random start / finish, edit edge tiles
        StartFinish();

        // Step #4: Use A* pathfinding to find that most direct route start -> finish
        GeneratePath();

        // Step #5: Draw the grid matrix to the tilemap
        DrawGrid();

        // Step #6: Place enemies along correct path
        SpawnEnemiesAlongPath();

        // Step #7: Place loot at dead ends
        SpawnLootDeadEnds();

        // Step #8: Place enemies in front of loot / in branching paths
        SpawnEnemiesBranchingPaths();

        UpdateGridDebug();
    }

    /*
        Helper functions for GenerateLevel()
    */

    void CreateMaze()
    {
        // fill grid with walls
        grid = new List<List<int>>();
        for (int y = 0; y < gridSize; y++)
        {
            grid.Add(new List<int>());
            for (int x = 0; x < gridSize; x++)
            {
                grid[y].Add(1);
            }
        }
    }

    void GenerateMaze()
    {
        grid = Dfs.DfsMazeGenerate(grid, gridSize, 0, 0);
    }

    void StartFinish()
    {
        // find open tiles for entrance/exit (after maze is generated)
        List<int> bottomOpenTiles = new List<int>();
        List<int> topOpenTiles = new List<int>();

        for (int x = 0; x < gridSize; x++)
        {
            if (grid[x][gridSize - 1] == 0)
                topOpenTiles.Add(x);
            if (grid[x][0] == 0)
                bottomOpenTiles.Add(x);
        }

        // select random start
        if (bottomOpenTiles.Count > 0)
            start.x = bottomOpenTiles[Random.Range(0, bottomOpenTiles.Count)];
        else
            start.x = 0;

        //Debug.Log("[" + string.Join(", ", bottomOpenTiles) + "]");

        // select random end
        if (topOpenTiles.Count > 0)
            end.x = topOpenTiles[Random.Range(0, topOpenTiles.Count)];
        else
            end.x = 0;

        //Debug.Log("[" + string.Join(", ", topOpenTiles) + "]");


        // set up edges
        bottomEdge = new List<int>();
        topEdge = new List<int>();

        for (int x = 0; x < gridSize; x++)
        {
            bottomEdge.Add(1);
            topEdge.Add(1);
        }

        bottomEdge[start.x] = 0;
        topEdge[end.x] = 0;
    }

    void GeneratePath()
    {
        List<Vector2Int> path = AStar.AStarFindPath(grid, gridSize, start, end);
        correctPath = path;

        if (path != null && path.Count > 0)
        {
            // Highlight the path on the floor tilemap
            foreach (Vector2Int pos in path)
            {
                grid[pos.x][pos.y] = 2;
            }
        }
        else
        {
            Debug.LogWarning("No path found between start and end!");
        }
    }

    void DrawGrid()
    {
        // Destroy old cubes before generating new level
        if (wallsContainer != null)
        {
            Destroy(wallsContainer);
        }
        
        // Create new container
        wallsContainer = new GameObject("WallsContainer");
        //wallsContainer.transform.parent = transform.parent;

        Vector3Int playerPos = new Vector3Int(0, 0, 0);

        // take the values from the grid array and draw them to the tilegrid
        for (int x = 0; x < gridSize; x++)
        {
            // top edge tiles
            Vector3Int topEdgePos = new Vector3Int(x + 1, gridSize + 1, 0);
            if (topEdge[x] == 0)
            {
                // this for loop carves a line after the exit tile
                for (int y = gridSize + 1; y <= 25; y++)
                {
                    topEdgePos.y = y;
                    edgeTilemap.SetTile(topEdgePos, null);
                }
            }
            else
            {
                for (int y = gridSize + 1; y <= 25; y++)
                {
                    topEdgePos.y = y;
                    edgeTilemap.SetTile(topEdgePos, edgeTile);

                    Vector3 cubePos = edgeTilemap.GetCellCenterWorld(topEdgePos);
                    cubePos.z = -0.5f;
                    Instantiate(edgeCube, cubePos, Quaternion.identity, wallsContainer.transform);
                }
            }

            // bottom edge tiles
            Vector3Int bottomEdgePos = new Vector3Int(x + 1, 0, 0);
            if (bottomEdge[x] == 0)
            {
                // this for loop carves a line leading up to the entrance tile
                for (int y = 0; y >= -9; y--)
                {
                    bottomEdgePos.y = y;
                    edgeTilemap.SetTile(bottomEdgePos, null);
                }

                // put the player in the entrance tile
                playerPos = new Vector3Int(x + 1, 0, 0);
            }
            else
            {
                for (int y = 0; y >= -9; y--)
                {
                    bottomEdgePos.y = y;
                    edgeTilemap.SetTile(bottomEdgePos, edgeTile);

                    Vector3 cubePos = edgeTilemap.GetCellCenterWorld(bottomEdgePos);
                    cubePos.z = -0.5f;
                    Instantiate(edgeCube, cubePos, Quaternion.identity, wallsContainer.transform);
                }
            }

            // place the player at the entrance
            player.transform.position = edgeTilemap.GetCellCenterWorld(playerPos);

            // place walls
            for (int y = 0; y < gridSize; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (grid[x][y] == 0 || grid[x][y] == 2)
                {
                    wallTilemap.SetTile(pos, null);
                }
                else
                {
                    wallTilemap.SetTile(pos, wallTile);

                    // add 3D cube on top of wall tiles
                    Vector3 cubePos = wallTilemap.GetCellCenterWorld(pos);
                    cubePos.z = -0.5f;
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);
                }

                if (grid[x][y] == 2)
                {
                    floorTilemap.SetTile(pos, highlightFloorTile);
                }
                else
                {
                    floorTilemap.SetTile(pos, floorTile);
                }

            }
        }
    }

    void SpawnEnemiesAlongPath()
    {
        // destroy old enemies
        if (enemyContainer != null)
        {
            Destroy(enemyContainer);
        }
        enemyContainer = new GameObject("EnemyContainer"); // recreate container

        // have one enemy spawn for each unit of distance along the intended path
        int step = Random.Range(8, 16);

        for (int i = step; i < correctPath.Count; i += step)
        {
            step = Random.Range(8, 16);
            grid[correctPath[i].x][correctPath[i].y] = 3;

            Vector3Int gridPos = new Vector3Int(correctPath[i].x, correctPath[i].y, 0);
            Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

            // spawn a random enemy from the enemy bank for this level
            //Instantiate(enemy, pos, Quaternion.identity, enemyContainer.transform);
            GameObject enemyObject = Instantiate(enemy, pos, Quaternion.identity, enemyContainer.transform);
            enemyObject.GetComponent<OverworldEnemy>().UpdateEnemy(enemyBank.GetRandomEnemy());
        }
    }

    void SpawnLootDeadEnds()
    {
        if (interactableContainer != null)
        {
            Destroy(interactableContainer);
        }
        interactableContainer = new GameObject("InteractableContainer"); // recreate container

        int[] dy = { -1, 1, 0, 0 }; // up, down
        int[] dx = { 0, 0, -1, 1 }; // left, right

        // search through maze matrix and look for dead ends to place loot
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
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
                    if (ny >= 0 && ny < gridSize && nx >= 0 && nx < gridSize)
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
                    grid[y][x] = 4; // add a 4 to represent this interactable instance

                    Vector3Int gridPos = new Vector3Int(y, x, 0);
                    Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

                    Instantiate(interactable, pos, Quaternion.identity, interactableContainer.transform);
                }
            }
        }
    }

    void SpawnEnemiesBranchingPaths()
    {
        List<Vector2Int> openFloorTiles = new List<Vector2Int>();

        // create a list of all of the open floor tiles (0s) and place a random range of enemies in random positions from that list
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
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
        
        // place enemies randomly on branching paths
        for (int i = 0; i < numEnemies; i++)
        {
            int randPos = Random.Range(0, openFloorTiles.Count);
            grid[openFloorTiles[randPos].x][openFloorTiles[randPos].y] = 3;

            Vector3Int gridPos = new Vector3Int(openFloorTiles[randPos].x, openFloorTiles[randPos].y, 0);
            Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

            Instantiate(enemy, pos, Quaternion.identity, enemyContainer.transform);
        }
    }
}