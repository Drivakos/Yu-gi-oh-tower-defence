using UnityEngine;
using System.Collections.Generic;
using YuGiOh.Pathfinding;

namespace YuGiOh.Gameplay
{
    public class PathFollower : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 5f;
        [SerializeField] private float pathUpdateInterval = 0.5f;
        [SerializeField] private float waypointThreshold = 0.1f;
        
        [Header("Path Settings")]
        [SerializeField] private Transform objective;
        [SerializeField] private bool loopPath = false;
        
        private PathManager pathManager;
        private List<Vector3> path;
        private int currentWaypointIndex;
        private float nextPathUpdate;
        
        private void Start()
        {
            pathManager = FindObjectOfType<PathManager>();
            if (pathManager == null)
            {
                Debug.LogError("PathManager not found in scene!");
                enabled = false;
                return;
            }
            
            UpdatePath();
        }
        
        private void Update()
        {
            if (path == null || path.Count == 0)
            {
                return;
            }
            
            if (Time.time >= nextPathUpdate)
            {
                UpdatePath();
            }
            
            MoveAlongPath();
        }
        
        private void UpdatePath()
        {
            if (objective == null)
            {
                return;
            }
            
            path = pathManager.FindPath(transform.position, objective.position);
            currentWaypointIndex = 0;
            nextPathUpdate = Time.time + pathUpdateInterval;
        }
        
        private void MoveAlongPath()
        {
            if (currentWaypointIndex >= path.Count)
            {
                if (loopPath)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    return;
                }
            }
            
            Vector3 targetPosition = path[currentWaypointIndex];
            targetPosition.y = transform.position.y; // Keep y position constant
            
            // Calculate direction to target
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // Rotate towards target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            
            // Move towards target
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Check if reached waypoint
            if (Vector3.Distance(transform.position, targetPosition) <= waypointThreshold)
            {
                currentWaypointIndex++;
            }
        }
        
        public void SetObjective(Transform newObjective)
        {
            objective = newObjective;
            UpdatePath();
        }
        
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        public void SetLoopPath(bool loop)
        {
            loopPath = loop;
        }
        
        private void OnDrawGizmos()
        {
            if (path != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
} 