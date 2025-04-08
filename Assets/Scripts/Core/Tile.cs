using UnityEngine;

namespace YuGiOhTowerDefense.Core
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color occupiedColor = Color.gray;
        
        private int gridX;
        private int gridZ;
        private bool isOccupied;
        private Monster placedMonster;
        
        public void Initialize(int x, int z)
        {
            gridX = x;
            gridZ = z;
            isOccupied = false;
            placedMonster = null;
            
            // Set default color
            if (meshRenderer != null)
            {
                meshRenderer.material.color = defaultColor;
            }
        }
        
        public void Highlight(bool highlight)
        {
            if (meshRenderer != null && !isOccupied)
            {
                meshRenderer.material.color = highlight ? highlightColor : defaultColor;
            }
        }
        
        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
            
            if (meshRenderer != null)
            {
                meshRenderer.material.color = occupied ? occupiedColor : defaultColor;
            }
        }
        
        public void SetPlacedMonster(Monster monster)
        {
            placedMonster = monster;
            SetOccupied(monster != null);
        }
        
        public bool IsOccupied()
        {
            return isOccupied;
        }
        
        public Monster GetPlacedMonster()
        {
            return placedMonster;
        }
        
        public int GetGridX()
        {
            return gridX;
        }
        
        public int GetGridZ()
        {
            return gridZ;
        }
        
        private void OnMouseEnter()
        {
            Highlight(true);
        }
        
        private void OnMouseExit()
        {
            Highlight(false);
        }
    }
} 