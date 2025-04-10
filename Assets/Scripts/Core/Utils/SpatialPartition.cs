using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Utils
{
    public class SpatialPartition
    {
        private readonly float cellSize;
        private readonly Dictionary<Vector2Int, List<Transform>> cells = new Dictionary<Vector2Int, List<Transform>>();
        private readonly LayerMask targetLayer;

        public SpatialPartition(float cellSize, LayerMask targetLayer)
        {
            this.cellSize = cellSize;
            this.targetLayer = targetLayer;
        }

        public void AddObject(Transform obj)
        {
            Vector2Int cellPos = GetCellPosition(obj.position);
            if (!cells.ContainsKey(cellPos))
            {
                cells[cellPos] = new List<Transform>();
            }
            cells[cellPos].Add(obj);
        }

        public void RemoveObject(Transform obj)
        {
            Vector2Int cellPos = GetCellPosition(obj.position);
            if (cells.ContainsKey(cellPos))
            {
                cells[cellPos].Remove(obj);
                if (cells[cellPos].Count == 0)
                {
                    cells.Remove(cellPos);
                }
            }
        }

        public void UpdateObject(Transform obj, Vector3 oldPosition)
        {
            Vector2Int oldCellPos = GetCellPosition(oldPosition);
            Vector2Int newCellPos = GetCellPosition(obj.position);

            if (oldCellPos != newCellPos)
            {
                if (cells.ContainsKey(oldCellPos))
                {
                    cells[oldCellPos].Remove(obj);
                    if (cells[oldCellPos].Count == 0)
                    {
                        cells.Remove(oldCellPos);
                    }
                }

                if (!cells.ContainsKey(newCellPos))
                {
                    cells[newCellPos] = new List<Transform>();
                }
                cells[newCellPos].Add(obj);
            }
        }

        public Transform FindNearest(Vector3 position, float maxDistance)
        {
            Vector2Int centerCell = GetCellPosition(position);
            int searchRadius = Mathf.CeilToInt(maxDistance / cellSize);

            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int cellPos = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    if (cells.TryGetValue(cellPos, out List<Transform> objectsInCell))
                    {
                        foreach (Transform obj in objectsInCell)
                        {
                            float distance = Vector3.Distance(position, obj.position);
                            if (distance <= maxDistance && distance < nearestDistance)
                            {
                                nearest = obj;
                                nearestDistance = distance;
                            }
                        }
                    }
                }
            }

            return nearest;
        }

        public List<Transform> FindInRange(Vector3 position, float range)
        {
            Vector2Int centerCell = GetCellPosition(position);
            int searchRadius = Mathf.CeilToInt(range / cellSize);
            List<Transform> results = new List<Transform>();

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int cellPos = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    if (cells.TryGetValue(cellPos, out List<Transform> objectsInCell))
                    {
                        results.AddRange(objectsInCell.Where(obj => 
                            Vector3.Distance(position, obj.position) <= range));
                    }
                }
            }

            return results;
        }

        private Vector2Int GetCellPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / cellSize),
                Mathf.FloorToInt(worldPosition.z / cellSize)
            );
        }

        public void Clear()
        {
            cells.Clear();
        }
    }
} 