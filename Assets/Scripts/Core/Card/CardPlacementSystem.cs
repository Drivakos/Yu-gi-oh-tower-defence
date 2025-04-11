using UnityEngine;
using System;
using YuGiOhTowerDefense.Core.Grid;
using YuGiOhTowerDefense.Core.Resource;

namespace YuGiOhTowerDefense.Core.Card
{
    /// <summary>
    /// Handles card placement on the grid
    /// </summary>
    public class CardPlacementSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ResourceManager resourceManager;
        
        [Header("Placement Settings")]
        [SerializeField] private LayerMask tileLayerMask;
        [SerializeField] private float cardPlacementHeight = 0.5f;
        
        private CardData selectedCard;
        private bool isPlacingCard;
        
        public event Action<CardData, Vector2Int> OnCardPlaced;
        public event Action<CardData> OnCardPlacementCancelled;
        
        private void Update()
        {
            if (!isPlacingCard) return;
            
            HandleCardPlacement();
        }
        
        public void StartCardPlacement(CardData card)
        {
            if (card == null)
            {
                Debug.LogError("Cannot start card placement with null card data");
                return;
            }
            
            if (!resourceManager.CanAffordCard(card.Cost))
            {
                Debug.LogWarning("Not enough DP to place this card");
                return;
            }
            
            selectedCard = card;
            isPlacingCard = true;
            gridManager.ClearAllSelections();
        }
        
        public void CancelCardPlacement()
        {
            if (!isPlacingCard) return;
            
            OnCardPlacementCancelled?.Invoke(selectedCard);
            ResetPlacementState();
        }
        
        private void HandleCardPlacement()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    HandleTileHover(tile);
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        TryPlaceCard(tile);
                    }
                }
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                CancelCardPlacement();
            }
        }
        
        private void HandleTileHover(Tile tile)
        {
            gridManager.ClearAllSelections();
            
            if (!tile.IsOccupied)
            {
                gridManager.SetTileSelected(tile.GridPosition.x, tile.GridPosition.y, true);
            }
        }
        
        private void TryPlaceCard(Tile tile)
        {
            if (tile.IsOccupied)
            {
                tile.ShowInvalidPlacement();
                return;
            }
            
            if (!resourceManager.TrySpendDP(selectedCard.Cost))
            {
                tile.ShowInvalidPlacement();
                return;
            }
            
            PlaceCard(tile);
        }
        
        private void PlaceCard(Tile tile)
        {
            Vector3 worldPosition = gridManager.GetWorldPosition(tile.GridPosition.x, tile.GridPosition.y);
            worldPosition.y = cardPlacementHeight;
            
            // Instantiate card prefab
            GameObject cardObject = Instantiate(selectedCard.Prefab, worldPosition, Quaternion.identity);
            
            // Initialize card
            CardController cardController = cardObject.GetComponent<CardController>();
            if (cardController != null)
            {
                cardController.Initialize(selectedCard, tile.GridPosition);
            }
            
            // Update grid state
            gridManager.SetTileOccupied(tile.GridPosition.x, tile.GridPosition.y, true);
            
            // Trigger event
            OnCardPlaced?.Invoke(selectedCard, tile.GridPosition);
            
            ResetPlacementState();
        }
        
        private void ResetPlacementState()
        {
            selectedCard = null;
            isPlacingCard = false;
            gridManager.ClearAllSelections();
        }
    }
} 