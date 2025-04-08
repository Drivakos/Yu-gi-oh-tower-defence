using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    public class CardPlacementManager : MonoBehaviour
    {
        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private float placementHeight = 0f;
        [SerializeField] private float placementRadius = 1f;
        [SerializeField] private GameObject placementIndicator;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        
        [Header("Grid Settings")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
        
        private Dictionary<Vector2Int, bool> occupiedPositions;
        private YuGiOhCard currentCard;
        private bool isPlacing;
        private Vector3 currentPlacementPosition;
        private bool isValidPlacement;
        
        private void Awake()
        {
            occupiedPositions = new Dictionary<Vector2Int, bool>();
            
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isPlacing || currentCard == null)
            {
                return;
            }
            
            // Update placement position
            UpdatePlacementPosition();
            
            // Check if placement is valid
            isValidPlacement = CheckValidPlacement(currentPlacementPosition);
            
            // Update placement indicator
            UpdatePlacementIndicator();
        }
        
        private void UpdatePlacementPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayer))
            {
                // Snap to grid
                Vector3 snappedPosition = SnapToGrid(hit.point);
                currentPlacementPosition = new Vector3(snappedPosition.x, placementHeight, snappedPosition.z);
            }
        }
        
        private Vector3 SnapToGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / cellSize) * cellSize;
            float z = Mathf.Round(position.z / cellSize) * cellSize;
            return new Vector3(x, position.y, z);
        }
        
        private bool CheckValidPlacement(Vector3 position)
        {
            // Check if position is within grid bounds
            Vector2Int gridPosition = WorldToGridPosition(position);
            if (!IsWithinGridBounds(gridPosition))
            {
                return false;
            }
            
            // Check if position is already occupied
            if (occupiedPositions.ContainsKey(gridPosition))
            {
                return false;
            }
            
            // Check for nearby cards
            Collider[] colliders = Physics.OverlapSphere(position, placementRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject != placementIndicator)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void UpdatePlacementIndicator()
        {
            if (placementIndicator == null)
            {
                return;
            }
            
            placementIndicator.SetActive(isPlacing);
            placementIndicator.transform.position = currentPlacementPosition;
            
            // Update material based on validity
            Renderer renderer = placementIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isValidPlacement ? validPlacementMaterial : invalidPlacementMaterial;
            }
        }
        
        public void StartPlacement(YuGiOhCard card)
        {
            if (card == null || !card.IsMonster())
            {
                return;
            }
            
            currentCard = card;
            isPlacing = true;
            
            // Show placement indicator
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
            }
        }
        
        public void CancelPlacement()
        {
            currentCard = null;
            isPlacing = false;
            
            // Hide placement indicator
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
        
        public bool TryPlaceCard(Vector3 position)
        {
            if (!isPlacing || currentCard == null || !isValidPlacement)
            {
                return false;
            }
            
            // Get grid position
            Vector2Int gridPosition = WorldToGridPosition(position);
            
            // Mark position as occupied
            occupiedPositions[gridPosition] = true;
            
            // Instantiate card
            GameObject cardObject = Instantiate(currentCard.Prefab, position, Quaternion.identity);
            
            // Reset placement state
            CancelPlacement();
            
            return true;
        }
        
        private Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / cellSize);
            int z = Mathf.RoundToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, z);
        }
        
        private bool IsWithinGridBounds(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < gridSize.x &&
                   gridPosition.y >= 0 && gridPosition.y < gridSize.y;
        }
        
        public void RemoveCard(Vector3 position)
        {
            Vector2Int gridPosition = WorldToGridPosition(position);
            if (occupiedPositions.ContainsKey(gridPosition))
            {
                occupiedPositions.Remove(gridPosition);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showGrid)
            {
                return;
            }
            
            Gizmos.color = gridColor;
            
            // Draw vertical lines
            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 start = new Vector3(x * cellSize, 0f, 0f);
                Vector3 end = new Vector3(x * cellSize, 0f, gridSize.y * cellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw horizontal lines
            for (int z = 0; z <= gridSize.y; z++)
            {
                Vector3 start = new Vector3(0f, 0f, z * cellSize);
                Vector3 end = new Vector3(gridSize.x * cellSize, 0f, z * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
} 