using UnityEngine;
using System;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Enemies
{
    public enum EnemyType
    {
        Normal,
        Elite,
        Boss
    }

    [Serializable]
    public class EnemyStats
    {
        public float health;
        public float speed;
        public float damage;
        public int rewardPoints;
        public EnemyType type;
    }

    public class Enemy : MonoBehaviour
    {
        [SerializeField] protected EnemyStats stats;
        [SerializeField] protected float rotationSpeed = 5f;
        [SerializeField] protected GameObject deathEffectPrefab;
        
        protected float currentHealth;
        protected Transform[] waypoints;
        protected int currentWaypointIndex;
        protected bool isActive;
        protected bool isStunned;
        protected float stunDuration;
        protected float stunTimer;

        public event Action<Enemy> OnEnemyDestroyed;
        public event Action<float> OnHealthChanged;
        public event Action<Enemy> OnEnemyReachedEnd;

        protected virtual void Start()
        {
            currentHealth = stats.health;
            isActive = true;
            isStunned = false;
            stunTimer = 0f;
            currentWaypointIndex = 0;
        }

        protected virtual void Update()
        {
            if (!isActive || isStunned) return;

            if (stunTimer > 0)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    isStunned = false;
                }
                return;
            }

            Move();
        }

        protected virtual void Move()
        {
            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachEnd();
                return;
            }

            Transform targetWaypoint = waypoints[currentWaypointIndex];
            Vector3 direction = (targetWaypoint.position - transform.position).normalized;
            
            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Move towards waypoint
            transform.position += direction * stats.speed * Time.deltaTime;

            // Check if reached current waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);
            if (distanceToWaypoint < 0.1f)
            {
                currentWaypointIndex++;
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (!isActive) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isActive = false;
            
            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Reward player
            GameManager.Instance.SpendDuelPoints(-stats.rewardPoints); // Negative to add points
            
            OnEnemyDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        protected virtual void ReachEnd()
        {
            isActive = false;
            OnEnemyReachedEnd?.Invoke(this);
            
            // Deal damage to player
            GameManager.Instance.TakeDamage((int)stats.damage);
            
            Destroy(gameObject);
        }

        public virtual void Stun(float duration)
        {
            isStunned = true;
            stunTimer = duration;
        }

        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
        }

        public EnemyStats GetStats()
        {
            return stats;
        }

        public float GetHealthPercentage()
        {
            return currentHealth / stats.health;
        }

        public bool IsStunned()
        {
            return isStunned;
        }
    }
} 