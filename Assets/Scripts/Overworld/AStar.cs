using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public class Node
    {
        public Vector2Int pos;
        public Node parent;
        public float gCost; // distance from start
        public float hCost; // heuristic distance to end
        public float fCost { get { return gCost + hCost; } }

        public Node(Vector2Int position, Node parentNode, float g, float h)
        {
            pos = position;
            parent = parentNode;
            gCost = g;
            hCost = h;
        }
    }

    public static List<Vector2Int> AStarFindPath(List<List<int>> grid, int gridSize, Vector2Int startPos, Vector2Int endPos)
    {
        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        // start node
        Node startNode = new Node(startPos, null, 0, Heuristic(startPos, endPos));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // find node with lowest fCost
            Node currentNode = openList[0];
            for (int i = 1; i<openList.Count; i++)
            {
                if (openList[i].fCost<currentNode.fCost || 
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost<currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.pos);

            // check if we reached the end
            if (currentNode.pos == endPos)
            {
                return RetracePath(currentNode);
            }

            // check all neighbors (4-directional movement)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),  // up
                new Vector2Int(0, -1), // down
                new Vector2Int(1, 0),  // right
                new Vector2Int(-1, 0)  // left
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.pos + dir;

                // check if neighbor is walkable and not in closed list
                if (!IsWalkable(grid, gridSize, neighborPos) || closedList.Contains(neighborPos))
                    continue;

                float newGCost = currentNode.gCost + 1;

                // check if neighbor is in open list
                Node neighborNode = openList.Find(n => n.pos == neighborPos);

                if (neighborNode == null)
                {
                    // add new node to open list
                    neighborNode = new Node(neighborPos, currentNode, newGCost, Heuristic(neighborPos, endPos));
                    openList.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    // update existing node with better path
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        // no path found
        return null;
    }

    private static bool IsWalkable(List<List<int>> grid, int gridSize, Vector2Int pos)
    {
        // check bounds
        if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize)
            return false;

        // check if it's a floor tile (0 = floor, 1 = wall)
        return grid[pos.x][pos.y] == 0;
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan Distance (for 4-directional movement)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.pos);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
