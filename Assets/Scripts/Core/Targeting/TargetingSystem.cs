using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using YuGiOhTowerDefense.Utils;

namespace YuGiOhTowerDefense.Targeting
{
    public enum TargetPriority
    {
        First,
        Last,
        Strongest,
        Weakest,
        Closest,
        Farthest
    }

    public interface ITargetable
    {
        Transform Transform { get; }
        float Health { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }
    }

    public class TargetingSystem
    {
        private readonly SpatialPartition spatialPartition;
        private readonly TargetPriority priority;
        private readonly float range;

        public TargetingSystem(SpatialPartition spatialPartition, TargetPriority priority, float range)
        {
            this.spatialPartition = spatialPartition;
            this.priority = priority;
            this.range = range;
        }

        public Transform FindTarget(Vector3 position, LayerMask targetLayer)
        {
            List<Transform> targets = spatialPartition.FindInRange(position, range);
            if (targets.Count == 0)
            {
                return null;
            }

            List<ITargetable> targetables = targets
                .Select(t => t.GetComponent<ITargetable>())
                .Where(t => t != null && t.IsAlive)
                .ToList();

            if (targetables.Count == 0)
            {
                return null;
            }

            return priority switch
            {
                TargetPriority.First => targetables[0].Transform,
                TargetPriority.Last => targetables[^1].Transform,
                TargetPriority.Strongest => targetables.OrderByDescending(t => t.MaxHealth).First().Transform,
                TargetPriority.Weakest => targetables.OrderBy(t => t.Health).First().Transform,
                TargetPriority.Closest => targetables.OrderBy(t => Vector3.Distance(position, t.Transform.position)).First().Transform,
                TargetPriority.Farthest => targetables.OrderByDescending(t => Vector3.Distance(position, t.Transform.position)).First().Transform,
                _ => targetables[0].Transform
            };
        }

        public List<Transform> FindTargetsInRange(Vector3 position, LayerMask targetLayer)
        {
            return spatialPartition.FindInRange(position, range);
        }

        public void UpdateTarget(Transform target, Vector3 oldPosition)
        {
            spatialPartition.UpdateObject(target, oldPosition);
        }

        public void AddTarget(Transform target)
        {
            spatialPartition.AddObject(target);
        }

        public void RemoveTarget(Transform target)
        {
            spatialPartition.RemoveObject(target);
        }
    }
} 