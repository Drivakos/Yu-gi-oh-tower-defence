using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Managers
{
    public class TileManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 3;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float tileSpacing = 0.1f;

        private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private Dictionary<Vector2Int, MonsterCard> placedMonsters = new Dictionary<Vector2Int, MonsterCard>();

        private void Start()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Vector3 worldPosition = GetWorldPosition(position);
                    
                    GameObject tileObj = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                    tileObj.transform.SetParent(transform);
                    
                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tile.Initialize(position);
                        tiles[position] = tile;
                    }
                }
            }
        }

        private Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            float x = gridPosition.x * (tileSize + tileSpacing);
            float z = gridPosition.y * (tileSize + tileSpacing);
            return new Vector3(x, 0, z);
        }

        public bool PlaceMonster(Vector2Int position, MonsterCard monster)
        {
            if (!tiles.ContainsKey(position))
            {
                Debug.Log("Invalid tile position!");
                return false;
            }

            if (placedMonsters.ContainsKey(position))
            {
                Debug.Log("Tile already occupied!");
                return false;
            }

            Tile tile = tiles[position];
            if (tile.IsOccupied)
            {
                Debug.Log("Tile is already occupied!");
                return false;
            }

            monster.transform.position = GetWorldPosition(position);
            placedMonsters[position] = monster;
            tile.SetOccupied(true);

            return true;
        }

        public void RemoveMonster(Vector2Int position)
        {
            if (placedMonsters.ContainsKey(position))
            {
                placedMonsters.Remove(position);
                tiles[position].SetOccupied(false);
            }
        }

        public MonsterCard GetMonsterAtPosition(Vector2Int position)
        {
            return placedMonsters.ContainsKey(position) ? placedMonsters[position] : null;
        }

        public List<MonsterCard> GetMonstersInRange(Vector2Int center, float range)
        {
            List<MonsterCard> monstersInRange = new List<MonsterCard>();

            foreach (var kvp in placedMonsters)
            {
                float distance = Vector2Int.Distance(center, kvp.Key);
                if (distance <= range)
                {
                    monstersInRange.Add(kvp.Value);
                }
            }

            return monstersInRange;
        }

        public Vector2Int? GetNearestEmptyTile(Vector2Int position)
        {
            float minDistance = float.MaxValue;
            Vector2Int? nearestTile = null;

            foreach (var kvp in tiles)
            {
                if (!kvp.Value.IsOccupied)
                {
                    float distance = Vector2Int.Distance(position, kvp.Key);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestTile = kvp.Key;
                    }
                }
            }

            return nearestTile;
        }
    }
} 