using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class CreateLevel : MonoBehaviour
{
    [Header("Debug")]
    public bool enemies = true;
    public bool interactables = true;

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

    // grid parameter
    private int gridSize;
    private List<List<int>> grid;
    private List<int> topEdge;
    private List<int> bottomEdge;

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
        if (Input.GetKeyDown(KeyCode.G))
        {
            DrawLevel();
        }
    }

    public void DrawLevel()
    {
        ProcGen.GenerateLevel();
        grid = ProcGen.GetLevel();
        topEdge = ProcGen.GetTopEdge();
        bottomEdge = ProcGen.GetBottomEdge();
        gridSize = grid.Count;

        DrawMaze();

        if (interactables)
            AddInteractables();

        if (enemies)
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
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);
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
                    Instantiate(wallCube, cubePos, Quaternion.identity, wallsContainer.transform);
                }
            }

            // place the player at the entrance
            player.transform.position = edgeTilemap.GetCellCenterWorld(playerPos);

            // place walls
            for (int y = 0; y < gridSize; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (grid[x][y] == 1)
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

    private void AddEnemies()
    {
        // destroy old enemies
        if (enemyContainer != null)
        {
            Destroy(enemyContainer);
        }
        enemyContainer = new GameObject("EnemyContainer"); // recreate container

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x][y] == 3)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, 0);
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

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x][y] == 4)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, 0);
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