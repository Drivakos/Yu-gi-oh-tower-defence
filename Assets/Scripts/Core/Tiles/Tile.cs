using UnityEngine;
using System.Collections;

namespace YuGiOhTowerDefense.Tiles
{
    public class Tile : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material occupiedMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material invalidMaterial;
        
        [Header("References")]
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] private GameObject highlightObject;
        
        private Vector2Int gridPosition;
        private bool isOccupied;
        private bool isSelected;
        
        public Vector2Int GridPosition => gridPosition;
        public bool IsOccupied => isOccupied;
        public bool IsSelected => isSelected;
        
        private void Awake()
        {
            if (tileRenderer == null)
            {
                tileRenderer = GetComponent<Renderer>();
            }
            
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }
        
        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            isOccupied = false;
            isSelected = false;
            UpdateVisualState();
        }
        
        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
            UpdateVisualState();
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (tileRenderer == null) return;
            
            if (isSelected)
            {
                tileRenderer.material = selectedMaterial;
                if (highlightObject != null)
                {
                    highlightObject.SetActive(true);
                }
            }
            else if (isOccupied)
            {
                tileRenderer.material = occupiedMaterial;
                if (highlightObject != null)
                {
                    highlightObject.SetActive(false);
                }
            }
            else
            {
                tileRenderer.material = defaultMaterial;
                if (highlightObject != null)
                {
                    highlightObject.SetActive(false);
                }
            }
        }
        
        private void OnMouseDown()
        {
            if (GameManager.Instance == null) return;
            
            // Check if we can place a monster here
            if (!isOccupied)
            {
                // Notify GameManager of tile selection
                GameManager.Instance.OnTileSelected(this);
            }
            else
            {
                // Show invalid placement feedback
                StartCoroutine(ShowInvalidPlacementFeedback());
            }
        }
        
        private System.Collections.IEnumerator ShowInvalidPlacementFeedback()
        {
            if (tileRenderer != null)
            {
                Material originalMaterial = tileRenderer.material;
                tileRenderer.material = invalidMaterial;
                
                yield return new WaitForSeconds(0.5f);
                
                if (tileRenderer != null)
                {
                    tileRenderer.material = originalMaterial;
                }
            }
        }
    }
} 