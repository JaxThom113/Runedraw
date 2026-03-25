using System.Collections;
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
    public GameObject interactable;
    public EnemyBank enemyBank;

    [Header("3D Elements")]
    public GameObject edge; 
    public GameObject wallCube; 

    // grid parameters
    private int gridSize;
    private List<List<int>> grid;
    private List<int> topEdge;
    private List<int> bottomEdge;

    // custom levels
    private List<List<int>> tutorialGrid = new List<List<int>>();
    private List<List<int>> finalBossGrid;

    // containers for wall/enemy/interactable prefabs
    private GameObject wallsContainer; 
    private GameObject enemyContainer;
    private GameObject interactableContainer;

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
    }

    void Update()
    {
        // generate new random level
        if (Input.GetKeyDown(KeyCode.G))
        {
            DrawLevel();
        }

        // set current level to the format of Tutorial.csv
        if (Input.GetKeyDown(KeyCode.T))
        {
            TextAsset lvlFile = Resources.Load<TextAsset>("Levels/Tutorial");
            DrawLevel(lvlFile);
        }

        // set current level to the format of FinalBoss.csv
        if (Input.GetKeyDown(KeyCode.B))
        {
            TextAsset lvlFile = Resources.Load<TextAsset>("Levels/FinalBoss");
            DrawLevel(lvlFile);
        }

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

        // create thick walls on edges
        Instantiate(edge, new Vector3(13f, 0f, -0.5f), Quaternion.identity, wallsContainer.transform);
        Instantiate(edge, new Vector3(-13f, 0f, -0.5f), Quaternion.identity, wallsContainer.transform);

        // draw the top edge
        for (int x = 0; x < gridSize; x++)
        {
            Vector3Int topEdgeWorldPos = new Vector3Int(x + 1, gridSize + 1, 0);

            for (int y = gridSize + 1; y <= gridSize + 16; y++)
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
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);
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
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);

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
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);
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

                    Instantiate(interactable, pos, Quaternion.identity, interactableContainer.transform);
                }
            }
        }
    }

    private void AddModels()
    {
        // will be used to add torches, cobwebs, other decorations
    }
}