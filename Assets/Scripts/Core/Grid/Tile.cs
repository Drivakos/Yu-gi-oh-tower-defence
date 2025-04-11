using UnityEngine;

namespace YuGiOhTowerDefense.Core.Grid
{
    /// <summary>
    /// Represents a single tile in the grid system
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [SerializeField] private GameObject highlightObject;
        
        private Renderer tileRenderer;
        private Material defaultMaterial;
        private Material occupiedMaterial;
        private Material selectedMaterial;
        private Material invalidMaterial;
        
        private Vector2Int gridPosition;
        private bool isOccupied;
        private bool isSelected;
        
        public Vector2Int GridPosition => gridPosition;
        public bool IsOccupied => isOccupied;
        public bool IsSelected => isSelected;
        
        private void Awake()
        {
            tileRenderer = GetComponent<Renderer>();
            if (tileRenderer == null)
            {
                Debug.LogError("Renderer component missing on tile prefab");
            }
            
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }
        
        public void Initialize(int x, int y, Material defaultMat, Material occupiedMat, Material selectedMat, Material invalidMat)
        {
            gridPosition = new Vector2Int(x, y);
            defaultMaterial = defaultMat;
            occupiedMaterial = occupiedMat;
            selectedMaterial = selectedMat;
            invalidMaterial = invalidMat;
            
            ResetState();
        }
        
        public void ResetState()
        {
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
            if (highlightObject != null)
            {
                highlightObject.SetActive(selected);
            }
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (tileRenderer == null) return;
            
            if (isSelected)
            {
                tileRenderer.material = selectedMaterial;
            }
            else if (isOccupied)
            {
                tileRenderer.material = occupiedMaterial;
            }
            else
            {
                tileRenderer.material = defaultMaterial;
            }
        }
        
        public void ShowInvalidPlacement()
        {
            if (tileRenderer == null) return;
            
            StartCoroutine(ShowInvalidPlacementFeedback());
        }
        
        private System.Collections.IEnumerator ShowInvalidPlacementFeedback()
        {
            if (tileRenderer == null) yield break;
            
            Material originalMaterial = tileRenderer.material;
            tileRenderer.material = invalidMaterial;
            
            yield return new WaitForSeconds(0.5f);
            
            if (tileRenderer != null)
            {
                tileRenderer.material = originalMaterial;
            }
        }
        
        private void OnMouseDown()
        {
            if (!isOccupied)
            {
                // Trigger tile selection event
                // This will be handled by the CardPlacementSystem
            }
            else
            {
                ShowInvalidPlacement();
            }
        }
    }
} 