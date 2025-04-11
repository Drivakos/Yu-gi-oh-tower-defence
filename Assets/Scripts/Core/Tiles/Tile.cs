using UnityEngine;

namespace YuGiOhTowerDefense.Tiles
{
    public class Tile : MonoBehaviour
    {
        private Vector2Int gridPosition;
        private bool isOccupied;

        public Vector2Int GridPosition => gridPosition;
        public bool IsOccupied => isOccupied;

        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            isOccupied = false;
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
            // TODO: Update visual state of tile
        }

        private void OnMouseDown()
        {
            // TODO: Handle tile selection for monster placement
        }
    }
} 