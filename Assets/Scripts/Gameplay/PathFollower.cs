using UnityEngine;
using UnityEngine.AI;

namespace YuGiOh.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PathFollower : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Transform currentObjective;
        
        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        private void Update()
        {
            if (currentObjective != null)
            {
                agent.SetDestination(currentObjective.position);
            }
        }
        
        public void SetObjective(Transform objective)
        {
            currentObjective = objective;
        }
        
        public void SetMoveSpeed(float speed)
        {
            if (agent != null)
            {
                agent.speed = speed;
            }
        }
        
        public void Stop()
        {
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }
        
        public void Resume()
        {
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
    }
} 