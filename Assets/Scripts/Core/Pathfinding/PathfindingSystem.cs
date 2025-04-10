using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Core.Pathfinding
{
    public class PathfindingSystem : MonoBehaviour
    {
        [SerializeField] private TileSystem tileSystem;
        [SerializeField] private Vector2Int startPosition;
        [SerializeField] private Vector2Int endPosition;
        
        private void Awake()
        {
            if (tileSystem == null)
            {
                tileSystem = FindObjectOfType<TileSystem>();
                if (tileSystem == null)
                {
                    Debug.LogError("TileSystem not found!");
                }
            }
        }
        
        public List<Vector3> FindPath(Vector2Int start, Vector2Int end)
        {
            List<PathNode> openSet = new List<PathNode>();
            HashSet<PathNode> closedSet = new HashSet<PathNode>();
            
            PathNode startNode = new PathNode(start);
            PathNode endNode = new PathNode(end);
            
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                // Get node with lowest F cost
                PathNode currentNode = openSet.OrderBy(node => node.FCost).First();
                
                if (currentNode.Position == endNode.Position)
                {
                    return RetracePath(startNode, currentNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                foreach (PathNode neighbor in GetNeighbors(currentNode))
                {
                    if (closedSet.Contains(neighbor) || !IsWalkable(neighbor.Position))
                    {
                        continue;
                    }
                    
                    float newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, endNode);
                        neighbor.Parent = currentNode;
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            
            return null; // No path found
        }
        
        private List<Vector3> RetracePath(PathNode startNode, PathNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathNode currentNode = endNode;
            
            while (currentNode != startNode)
            {
                path.Add(tileSystem.GridToWorldPosition(currentNode.Position));
                currentNode = currentNode.Parent;
            }
            
            path.Reverse();
            return path;
        }
        
        private List<PathNode> GetNeighbors(PathNode node)
        {
            List<PathNode> neighbors = new List<PathNode>();
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    
                    Vector2Int neighborPos = new Vector2Int(node.Position.x + x, node.Position.y + y);
                    if (IsWithinGrid(neighborPos))
                    {
                        neighbors.Add(new PathNode(neighborPos));
                    }
                }
            }
            
            return neighbors;
        }
        
        private float GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int distX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
            int distY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
            
            if (distX > distY)
            {
                return 14 * distY + 10 * (distX - distY);
            }
            return 14 * distX + 10 * (distY - distX);
        }
        
        private bool IsWalkable(Vector2Int position)
        {
            // Check if the position is within grid bounds and not occupied
            return IsWithinGrid(position) && !tileSystem.IsTileOccupied(position);
        }
        
        private bool IsWithinGrid(Vector2Int position)
        {
            return position.x >= 0 && position.x < tileSystem.GridWidth &&
                   position.y >= 0 && position.y < tileSystem.GridHeight;
        }
    }
    
    public class PathNode
    {
        public Vector2Int Position { get; private set; }
        public PathNode Parent { get; set; }
        public float GCost { get; set; } // Distance from start
        public float HCost { get; set; } // Distance to end
        public float FCost => GCost + HCost; // Total cost
        
        public PathNode(Vector2Int position)
        {
            Position = position;
        }
    }
} 