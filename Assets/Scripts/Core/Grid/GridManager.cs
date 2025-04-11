using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core.Grid
{
    /// <summary>
    /// Manages the grid system for card placement and enemy movement
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 originPosition = Vector3.zero;
        
        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material occupiedMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material invalidMaterial;
        
        private Tile[,] grid;
        private List<Tile> occupiedTiles = new List<Tile>();
        
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Vector3 OriginPosition => originPosition;
        
        private void Awake()
        {
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            grid = new Tile[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = GetWorldPosition(x, y);
                    GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    tileObject.name = $"Tile_{x}_{y}";
                    
                    Tile tile = tileObject.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tile.Initialize(x, y, defaultMaterial, occupiedMaterial, selectedMaterial, invalidMaterial);
                        grid[x, y] = tile;
                    }
                    else
                    {
                        Debug.LogError($"Tile component missing on prefab at position ({x}, {y})");
                    }
                }
            }
        }
        
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) * cellSize + originPosition;
        }
        
        public void GetGridPosition(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FlrotToInt((worldPosition - originPosition).z / cellSize);
        }
        
        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return grid[x, y];
            }
            return null;
        }
        
        public Tile GetTile(Vector3 worldPosition)
        {
            GetGridPosition(worldPosition, out int x, out int y);
            return GetTile(x, y);
        }
        
        public bool IsTileOccupied(int x, int y)
        {
            Tile tile = GetTile(x, y);
            return tile != null && tile.IsOccupied;
        }
        
        public bool IsTileOccupied(Vector3 worldPosition)
        {
            Tile tile = GetTile(worldPosition);
            return tile != null && tile.IsOccupied;
        }
        
        public void SetTileOccupied(int x, int y, bool occupied)
        {
            Tile tile = GetTile(x, y);
            if (tile != null)
            {
                tile.SetOccupied(occupied);
                if (occupied)
                {
                    occupiedTiles.Add(tile);
                }
                else
                {
                    occupiedTiles.Remove(tile);
                }
            }
        }
        
        public void SetTileSelected(int x, int y, bool selected)
        {
            Tile tile = GetTile(x, y);
            if (tile != null)
            {
                tile.SetSelected(selected);
            }
        }
        
        public void ClearAllSelections()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetTileSelected(x, y, false);
                }
            }
        }
        
        public List<Tile> GetOccupiedTiles()
        {
            return new List<Tile>(occupiedTiles);
        }
        
        public List<Tile> GetEmptyTiles()
        {
            List<Tile> emptyTiles = new List<Tile>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = grid[x, y];
                    if (tile != null && !tile.IsOccupied)
                    {
                        emptyTiles.Add(tile);
                    }
                }
            }
            return emptyTiles;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = GetWorldPosition(x, y);
                    Gizmos.DrawWireCube(position + new Vector3(cellSize, 0, cellSize) * 0.5f, 
                                       new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }
    }
} 