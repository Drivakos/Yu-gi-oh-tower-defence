using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core.Pathfinding;
using System.Collections;

namespace YuGiOhTowerDefense.Core.Enemy
{
    public class Enemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Combat Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackCooldown = 1f;
        
        private PathfindingSystem pathfindingSystem;
        private List<Vector3> path;
        private int currentPathIndex;
        private int currentHealth;
        private float attackTimer;
        private bool isAttacking;
        private float healthMultiplier = 1f;
        private float damageMultiplier = 1f;
        
        public event System.Action<Enemy> OnDeath;
        
        private void Awake()
        {
            pathfindingSystem = FindObjectOfType<PathfindingSystem>();
            if (pathfindingSystem == null)
            {
                Debug.LogError("PathfindingSystem not found!");
            }
            
            currentHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        }
        
        public void SetStatsMultipliers(float healthMult, float damageMult)
        {
            healthMultiplier = healthMult;
            damageMultiplier = damageMult;
            
            // Update current health with new multiplier
            currentHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        }
        
        private void Update()
        {
            if (path != null && currentPathIndex < path.Count)
            {
                MoveAlongPath();
            }
            
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }
        
        public void Initialize(Vector2Int startPosition, Vector2Int endPosition)
        {
            path = pathfindingSystem.FindPath(startPosition, endPosition);
            currentPathIndex = 0;
        }
        
        private void MoveAlongPath()
        {
            Vector3 targetPosition = path[currentPathIndex];
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // Rotate towards movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Move towards target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Check if reached current waypoint
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPathIndex++;
                
                // Check if reached end of path
                if (currentPathIndex >= path.Count)
                {
                    OnReachedDestination();
                }
            }
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && attackTimer <= 0)
            {
                Attack(other.GetComponent<Player>());
            }
        }
        
        private void Attack(Player player)
        {
            if (player != null)
            {
                int actualDamage = Mathf.RoundToInt(damage * damageMultiplier);
                player.TakeDamage(actualDamage);
                attackTimer = attackCooldown;
            }
        }
        
        private void OnReachedDestination()
        {
            // Handle reaching the destination (e.g., damage player, destroy self)
            Destroy(gameObject);
        }
        
        private void Die()
        {
            // Trigger death event before destroying
            OnDeath?.Invoke(this);
            
            // Handle enemy death (e.g., spawn effects, give rewards)
            Destroy(gameObject);
        }
        
        public void ApplyStun(float duration)
        {
            StartCoroutine(StunEffect(duration));
        }
        
        private IEnumerator StunEffect(float duration)
        {
            float originalSpeed = moveSpeed;
            moveSpeed = 0f;
            
            yield return new WaitForSeconds(duration);
            
            moveSpeed = originalSpeed;
        }
        
        public void ApplySlow(float slowAmount, float duration)
        {
            StartCoroutine(SlowEffect(slowAmount, duration));
        }
        
        private IEnumerator SlowEffect(float slowAmount, float duration)
        {
            float originalSpeed = moveSpeed;
            moveSpeed *= (1f - slowAmount);
            
            yield return new WaitForSeconds(duration);
            
            moveSpeed = originalSpeed;
        }
        
        public void ApplyBuff(float buffAmount, float duration)
        {
            StartCoroutine(BuffEffect(buffAmount, duration));
        }
        
        private IEnumerator BuffEffect(float buffAmount, float duration)
        {
            float originalDamage = damage;
            damage *= (1f + buffAmount);
            
            yield return new WaitForSeconds(duration);
            
            damage = originalDamage;
        }
    }
} 