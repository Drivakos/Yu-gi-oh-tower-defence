using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOh.Pathfinding
{
    public class PathManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Vector2Int gridDimensions = new Vector2Int(50, 50);
        [SerializeField] private LayerMask obstacleLayer;
        
        private Node[,] grid;
        private Vector3 gridOrigin;
        
        private void Start()
        {
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            grid = new Node[gridDimensions.x, gridDimensions.y];
            gridOrigin = transform.position - new Vector3(gridDimensions.x * gridSize * 0.5f, 0, gridDimensions.y * gridSize * 0.5f);
            
            // Create grid nodes
            for (int x = 0; x < gridDimensions.x; x++)
            {
                for (int y = 0; y < gridDimensions.y; y++)
                {
                    Vector3 worldPos = gridOrigin + new Vector3(x * gridSize, 0, y * gridSize);
                    bool walkable = !Physics.CheckBox(worldPos, Vector3.one * gridSize * 0.5f, Quaternion.identity, obstacleLayer);
                    grid[x, y] = new Node(walkable, worldPos, x, y);
                }
            }
        }
        
        public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
        {
            Node startNode = GetNodeFromWorldPoint(startPos);
            Node endNode = GetNodeFromWorldPoint(endPos);
            
            if (startNode == null || endNode == null || !startNode.walkable || !endNode.walkable)
            {
                return null;
            }
            
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.OrderBy(n => n.fCost).First();
                
                if (currentNode == endNode)
                {
                    return RetracePath(startNode, endNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                foreach (Node neighbor in GetNeighbors(currentNode))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, endNode);
                        neighbor.parent = currentNode;
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            
            return null;
        }
        
        private List<Vector3> RetracePath(Node startNode, Node endNode)
        {
            List<Vector3> path = new List<Vector3>();
            Node currentNode = endNode;
            
            while (currentNode != startNode)
            {
                path.Add(currentNode.worldPosition);
                currentNode = currentNode.parent;
            }
            
            path.Add(startNode.worldPosition);
            path.Reverse();
            
            return path;
        }
        
        private Node GetNodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x - gridOrigin.x) / (gridDimensions.x * gridSize);
            float percentY = (worldPosition.z - gridOrigin.z) / (gridDimensions.y * gridSize);
            
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);
            
            int x = Mathf.RoundToInt((gridDimensions.x - 1) * percentX);
            int y = Mathf.RoundToInt((gridDimensions.y - 1) * percentY);
            
            return grid[x, y];
        }
        
        private List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    
                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    
                    if (checkX >= 0 && checkX < gridDimensions.x && checkY >= 0 && checkY < gridDimensions.y)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }
            
            return neighbors;
        }
        
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            
            if (distX > distY)
            {
                return 14 * distY + 10 * (distX - distY);
            }
            
            return 14 * distX + 10 * (distY - distX);
        }
        
        private void OnDrawGizmos()
        {
            if (grid != null)
            {
                foreach (Node node in grid)
                {
                    Gizmos.color = node.walkable ? Color.white : Color.red;
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * gridSize * 0.5f);
                }
            }
        }
    }
    
    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        
        public int gCost; // Distance from start
        public int hCost; // Distance to end
        public Node parent;
        
        public int fCost => gCost + hCost;
        
        public Node(bool walkable, Vector3 worldPos, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }
} 