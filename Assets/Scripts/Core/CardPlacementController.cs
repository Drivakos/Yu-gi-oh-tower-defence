using UnityEngine;
using UnityEngine.EventSystems;

namespace YuGiOhTowerDefense.Core
{
    public class CardPlacementController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TileSystem tileSystem;
        [SerializeField] private LayerMask tileLayer;
        
        private Card selectedCard;
        private Vector3 dragOffset;
        private bool isDragging;
        
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
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                selectedCard = eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>();
                if (selectedCard != null)
                {
                    isDragging = true;
                    dragOffset = selectedCard.transform.position - GetMouseWorldPosition();
                    tileSystem.HighlightValidPlacements(selectedCard);
                }
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging && selectedCard != null)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                selectedCard.transform.position = mousePosition + dragOffset;
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDragging && selectedCard != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayer))
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null)
                    {
                        Vector2Int gridPosition = tile.GridPosition;
                        if (tileSystem.PlaceCard(gridPosition, selectedCard))
                        {
                            // Card was successfully placed
                            selectedCard.OnPlaced();
                        }
                        else
                        {
                            // Return card to hand or original position
                            selectedCard.ReturnToHand();
                        }
                    }
                }
                else
                {
                    // Return card to hand or original position
                    selectedCard.ReturnToHand();
                }
                
                tileSystem.ResetTileHighlights();
                selectedCard = null;
                isDragging = false;
            }
        }
        
        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayer))
            {
                return hit.point;
            }
            
            return Vector3.zero;
        }
    }
} 