using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class CreateLevel : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap wallTilemap;
    public Tilemap edgeTilemap;
    public Tilemap floorTilemap;
    public TileBase wallTile;
    public TileBase floorTile;
    public TileBase edgeTile;
    public TileBase highlightFloorTile;

    [Header("Entity References")]
    public GameObject player;
    public GameObject enemy;    
    public GameObject lootBox;
    public GameObject campfire;

    [Header("3D Elements")]
    public GameObject edge; 
    public GameObject wallCube; 
    public GameObject torch;

    // grid parameters
    private int gridSize;
    private List<List<int>> grid;
    private List<int> topEdge;
    private List<int> bottomEdge;

    // containers for different prefabs
    private GameObject wallsContainer; 
    private GameObject enemyContainer;
    private GameObject interactableContainer;
    private GameObject torchContainer;

    // enemy bank
    private EnemyBank enemyBank;
    
    void OnEnable()
    {
        DrawLevel();
    }

    void OnDisable()
    {
        // destroy old containers generating new level
        Destroy(wallsContainer);
        Destroy(enemyContainer);
        Destroy(interactableContainer);
        Destroy(torchContainer);
    }

    void Update()
    {
        // generate new random level
        if (Input.GetKeyDown(KeyCode.G))
        {
            DrawLevel();
        }

        // set current level to the format of Tutorial.csv
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     TextAsset lvlFile = Resources.Load<TextAsset>("Levels/Tutorial");
        //     DrawLevel(lvlFile);
        // }

        // // set current level to the format of FinalBoss.csv
        // if (Input.GetKeyDown(KeyCode.B))
        // {
        //     TextAsset lvlFile = Resources.Load<TextAsset>("Levels/FinalBoss");
        //     DrawLevel(lvlFile);
        // }

        if (LevelSystem.Instance.enemies)
            enemyContainer.SetActive(true);
        else
            enemyContainer.SetActive(false);

        if (LevelSystem.Instance.interactables)
            interactableContainer.SetActive(true);
        else
            interactableContainer.SetActive(false);
    }

    public void DrawLevel(TextAsset csv = null)
    {
        if (csv != null)
        {
            LevelParser.GenerateLevelFromCsv(csv);

            grid = LevelParser.GetLevel();
            topEdge = LevelParser.GetTopEdge();
            bottomEdge = LevelParser.GetBottomEdge();
        }
        else
        {
            ProcGen.GenerateLevel();

            grid = ProcGen.GetLevel();
            topEdge = ProcGen.GetTopEdge();
            bottomEdge = ProcGen.GetBottomEdge();
        }
        
        gridSize = grid.Count;

        DrawMaze();

        AddInteractables();

        AddEnemies();

        AddModels();
    }

    private void DrawMaze()
    {
        // destroy old cubes before generating new level
        if (wallsContainer != null)
        {
            Destroy(wallsContainer);
        }
        wallsContainer = new GameObject("WallsContainer"); // recreate container for new level

        Vector3Int playerPos = new Vector3Int(0, 0, 0);

        // draw left/right edges
        for (int y = -9; y <= 25; y++)
        {
            Vector3Int leftEdgeWorldPos = new Vector3Int(0, y, 0);
            Vector3Int rightEdgeWorldPos = new Vector3Int(16, y, 0);

            Vector3 leftCubePos = edgeTilemap.GetCellCenterWorld(leftEdgeWorldPos);
            leftCubePos.z = -0.5f;
            Instantiate(wallCube, leftCubePos, Quaternion.Euler(-90f, 0f, 0f), wallsContainer.transform);

            Vector3 rightCubePos = edgeTilemap.GetCellCenterWorld(rightEdgeWorldPos);
            rightCubePos.z = -0.5f;
            Instantiate(wallCube, rightCubePos, Quaternion.Euler(-90f, 0f, 0f), wallsContainer.transform);
        }

        // create thick walls on edges (limits number of cubes instantiated on left/right edges)
        Instantiate(edge, new Vector3(13.5f, 0f, -0.5f), Quaternion.identity, wallsContainer.transform);
        Instantiate(edge, new Vector3(-13.5f, 0f, -0.5f), Quaternion.identity, wallsContainer.transform);

        // draw the top edge
        for (int x = 0; x < gridSize; x++)
        {
            Vector3Int topEdgeWorldPos = new Vector3Int(x + 1, gridSize + 1, 0);

            for (int y = gridSize + 1; y <= gridSize + 10; y++)
            {
                topEdgeWorldPos.y = y;
                
                if (topEdge[x] == 0)
                {
                    // this for loop carves a line after the exit tile
                    edgeTilemap.SetTile(topEdgeWorldPos, null);
                }
                else
                {
                    edgeTilemap.SetTile(topEdgeWorldPos, edgeTile);

                    Vector3 cubePos = edgeTilemap.GetCellCenterWorld(topEdgeWorldPos);
                    cubePos.z = -0.5f;
                    Instantiate(wallCube, cubePos, Quaternion.Euler(-90f, 0f, 0f), wallsContainer.transform);
                }
            }
        }

        // draw the bottom edge
        for (int x = 0; x < gridSize; x++)
        {
            // put the player in the entrance tile
            if (bottomEdge[x] == 0)
                playerPos = new Vector3Int(x + 1, 0, 0);

            Vector3Int bottomEdgeWorldPos = new Vector3Int(x + 1, 0, 0);

            for (int y = 0; y >= -9; y--)
            {
                bottomEdgeWorldPos.y = y;
                
                if (bottomEdge[x] == 0)
                {
                    // this for loop carves a line after the exit tile
                    edgeTilemap.SetTile(bottomEdgeWorldPos, null);
                }
                else
                {
                    edgeTilemap.SetTile(bottomEdgeWorldPos, edgeTile);

                    Vector3 cubePos = edgeTilemap.GetCellCenterWorld(bottomEdgeWorldPos);
                    cubePos.z = -0.5f;
                    Instantiate(wallCube, cubePos, Quaternion.Euler(-90f, 0f, 0f), wallsContainer.transform);
                }
            }
        }

        // draw the grid, making sure to account for 4th quadrant vs 1st quadrant coordinates
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int flippedY = (gridSize - 1) - y; // convert from 4th quadrant -> 1st quadrant
                Vector3Int pos = new Vector3Int(x, flippedY, 0);

                if (grid[y][x] == 1)
                {
                    wallTilemap.SetTile(pos, wallTile);

                    // add 3D cube on top of wall tiles
                    Vector3 cubePos = wallTilemap.GetCellCenterWorld(pos);
                    cubePos.z = -0.5f;
                    Instantiate(wallCube, cubePos, Quaternion.Euler(-90f, 0f, 0f), wallsContainer.transform);
                }
                else
                {
                    wallTilemap.SetTile(pos, null);
                }

                if (grid[y][x] == 2)
                {
                    floorTilemap.SetTile(pos, highlightFloorTile);
                }
                else
                {
                    floorTilemap.SetTile(pos, floorTile);
                }
            }
        }

        // place the player at the entrance
        player.transform.position = edgeTilemap.GetCellCenterWorld(playerPos);
    }

    private void AddEnemies()
    {
        // destroy old enemies
        if (enemyContainer != null)
        {
            Destroy(enemyContainer);
        }
        enemyContainer = new GameObject("EnemyContainer"); // recreate container

        // get the currently active enemy bank for the current area (A1, A2, A3)
        enemyBank = FindFirstObjectByType<EnemyBank>(FindObjectsInactive.Exclude);

        // set up spawn weights for enemies like Wizard Lizard
        enemyBank.SetupSpawnWeights();

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (grid[y][x] == 3)
                {
                    int flippedY = (gridSize - 1) - y; // convert from 4th quadrant -> 1st quadrant
                    Vector3Int gridPos = new Vector3Int(x, flippedY, 0);
                    Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

                    GameObject enemyObject = Instantiate(enemy, pos, Quaternion.identity, enemyContainer.transform);
                    enemyObject.GetComponent<OverworldEnemy>().UpdateEnemy(enemyBank.GetRandomEnemy());
                }
            }
        }
    }

    private void AddInteractables()
    {
        if (interactableContainer != null)
        {
            Destroy(interactableContainer);
        }
        interactableContainer = new GameObject("InteractableContainer"); // recreate container

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (grid[y][x] == 4)
                {
                    int flippedY = (gridSize - 1) - y; // convert from 4th quadrant -> 1st quadrant
                    Vector3Int gridPos = new Vector3Int(x, flippedY, 0);
                    Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

                    GameObject lootBoxObject = Instantiate(lootBox, pos, Quaternion.identity, interactableContainer.transform);
                    lootBoxObject.name = $"Interactable_{x}_{y}";
                }

                if (grid[y][x] == 5)
                {
                    int flippedY = (gridSize - 1) - y; // convert from 4th quadrant -> 1st quadrant
                    Vector3Int gridPos = new Vector3Int(x, flippedY, 0);
                    Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

                    GameObject campfireObject = Instantiate(campfire, pos, Quaternion.identity, interactableContainer.transform);
                    campfireObject.name = $"Interactable_{x}_{y}";
                }
            }
        }
    }

    private void AddModels()
    {
        if (torchContainer != null)
        {
            Destroy(torchContainer);
        }
        torchContainer = new GameObject("TorchContainer"); // recreate container

        Dictionary<Vector2Int, List<bool>> availableTorchPositions = new Dictionary<Vector2Int, List<bool>>();

        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // ignore cells with walls, the enemies, and interactables
                if (grid[y][x] == 1 || grid[y][x] == 3 || grid[y][x] == 4)
                    continue;
                
                // check what adjacent tiles have walls where we can place torches, in this order: up, down, left, right
                List<bool> neighbors = new List<bool>();
                for (int i = 0; i < 4; i++)
                {
                    int dy = y + directions[i].y;
                    int dx = x + directions[i].x;

                    // bounds check to avoid errors
                    if (dy >= 0 && dy < gridSize && dx >= 0 && dx < gridSize)
                    {
                        if (grid[dy][dx] == 1)
                            neighbors.Add(true);
                        else
                            neighbors.Add(false);
                    }
                    else
                    {
                        neighbors.Add(true);
                    }
                }
                
                // ignore if there are no adjacent walls
                if (!neighbors.Contains(true))
                    continue;

                // ignore if this is a correct path tile on the top/bottom edge to avoid floating torched spawning at start/end of level
                if ((y == 0 || y == gridSize - 1) && grid[y][x] == 2)
                    continue;

                // add this position, with its neighboring tiles, as a valid place to put a torch
                availableTorchPositions.Add(new Vector2Int(y, x), neighbors);
            }
        }

        int numTorches = 30;
        for (int i = 0; i < numTorches; i++)
        {
            int randPos = Random.Range(0, availableTorchPositions.Count);
            var torchLocation = availableTorchPositions.ElementAt(randPos);
            availableTorchPositions.Remove(torchLocation.Key); // remove it so torches can't be placed on top of each other

            int flippedY = (gridSize - 1) - torchLocation.Key.x; // convert from 4th quadrant -> 1st quadrant
            Vector3Int gridPos = new Vector3Int(torchLocation.Key.y, flippedY, 0);
            Vector3 pos = floorTilemap.GetCellCenterWorld(gridPos);

            // pick a random wall from the walls neighboring this flooring tile to place the torch
            List<int> availableWalls = new List<int>();
            for (int k = 0; k < 4; k++)
            {
                if (torchLocation.Value[k])
                    availableWalls.Add(k);
            }
            
            int randWall = Random.Range(0, availableWalls.Count);
            switch (availableWalls[randWall])
            {
                case 0: // place torch on top wall
                    Instantiate(
                        torch, 
                        new Vector3(pos.x, pos.y, pos.z-0.5f), 
                        Quaternion.Euler(270f, 90f, -90f), 
                        torchContainer.transform
                    ).transform.localScale = Vector3.one * 0.0275f;
                    
                    break;
                case 1: // place torch on bottom wall
                    Instantiate(
                        torch, 
                        new Vector3(pos.x, pos.y, pos.z-0.5f), 
                        Quaternion.Euler(90f, 90f, -90f), 
                        torchContainer.transform
                    ).transform.localScale = Vector3.one * 0.0275f;
                    break;
                case 2: // place torch on left wall
                    Instantiate(
                        torch, 
                        new Vector3(pos.x, pos.y, pos.z-0.5f), 
                        Quaternion.Euler(180f, 90f, -90f), 
                        torchContainer.transform
                    ).transform.localScale = Vector3.one * 0.0275f;
                    break;
                case 3: // place torch on right wall
                    Instantiate(
                        torch, 
                        new Vector3(pos.x, pos.y, pos.z-0.5f),
                        Quaternion.Euler(0f, 90f, -90f), 
                        torchContainer.transform
                    ).transform.localScale = Vector3.one * 0.0275f;
                    break;
            }
        }
    }
}