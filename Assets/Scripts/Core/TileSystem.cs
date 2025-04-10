using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core
{
    public class TileSystem : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private LayerMask tileLayer;
        
        [Header("Visual Settings")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private Material defaultMaterial;
        
        private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private List<Card> placedCards = new List<Card>();
        
        private void Awake()
        {
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Vector3 worldPosition = GridToWorldPosition(position);
                    
                    GameObject tileObject = new GameObject($"Tile_{x}_{y}");
                    tileObject.transform.position = worldPosition;
                    tileObject.transform.parent = transform;
                    
                    Tile tile = tileObject.AddComponent<Tile>();
                    tile.Initialize(position, defaultMaterial);
                    
                    tiles[position] = tile;
                }
            }
        }
        
        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * tileSize,
                0f,
                gridPosition.y * tileSize
            );
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / tileSize),
                Mathf.RoundToInt(worldPosition.z / tileSize)
            );
        }
        
        public bool IsValidPlacement(Vector2Int gridPosition, Card card)
        {
            // Check if position is within grid bounds
            if (!IsWithinGrid(gridPosition))
            {
                return false;
            }
            
            // Check if tile is already occupied
            if (tiles.TryGetValue(gridPosition, out Tile tile) && tile.IsOccupied)
            {
                return false;
            }
            
            // Check if player has enough resources
            if (!GameManager.Instance.SpendDP(card.Cost))
            {
                return false;
            }
            
            return true;
        }
        
        public bool PlaceCard(Vector2Int gridPosition, Card card)
        {
            if (!IsValidPlacement(gridPosition, card))
            {
                return false;
            }
            
            if (tiles.TryGetValue(gridPosition, out Tile tile))
            {
                card.transform.position = GridToWorldPosition(gridPosition);
                tile.Occupy(card);
                placedCards.Add(card);
                return true;
            }
            
            return false;
        }
        
        public void RemoveCard(Card card)
        {
            if (placedCards.Contains(card))
            {
                Vector2Int gridPosition = WorldToGridPosition(card.transform.position);
                if (tiles.TryGetValue(gridPosition, out Tile tile))
                {
                    tile.Vacate();
                    placedCards.Remove(card);
                }
            }
        }
        
        private bool IsWithinGrid(Vector2Int position)
        {
            return position.x >= 0 && position.x < gridWidth &&
                   position.y >= 0 && position.y < gridHeight;
        }
        
        public void HighlightValidPlacements(Card card)
        {
            foreach (var tile in tiles.Values)
            {
                if (IsValidPlacement(tile.GridPosition, card))
                {
                    tile.SetMaterial(validPlacementMaterial);
                }
                else
                {
                    tile.SetMaterial(invalidPlacementMaterial);
                }
            }
        }
        
        public void ResetTileHighlights()
        {
            foreach (var tile in tiles.Values)
            {
                tile.SetMaterial(defaultMaterial);
            }
        }
    }
} 