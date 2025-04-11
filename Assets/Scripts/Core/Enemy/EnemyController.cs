using UnityEngine;
using System.Collections;
using YuGiOhTowerDefense.Core.UI;
using YuGiOhTowerDefense.Core.Score;

namespace YuGiOhTowerDefense.Core.Enemy
{
    /// <summary>
    /// Controls enemy behavior and attributes
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseSpeed = 2f;
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int baseScoreValue = 100;
        
        [Header("References")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private GameObject deathEffect;
        
        private float currentHealth;
        private float currentSpeed;
        private int currentDamage;
        private int currentScoreValue;
        private int currentWaypointIndex;
        
        public static event System.Action<EnemyController> OnEnemyDefeated;
        public static event System.Action<EnemyController> OnEnemyReachedEnd;
        
        public float Health => currentHealth;
        public float MaxHealth => baseHealth;
        public float Speed => currentSpeed;
        public int Damage => currentDamage;
        
        private void Awake()
        {
            currentWaypointIndex = 0;
        }
        
        public void Initialize(float healthMultiplier = 1f)
        {
            currentHealth = baseHealth * healthMultiplier;
            currentSpeed = baseSpeed;
            currentDamage = baseDamage;
            currentScoreValue = Mathf.RoundToInt(baseScoreValue * healthMultiplier);
            
            // Start moving along path
            StartCoroutine(MoveAlongPath());
        }
        
        private IEnumerator MoveAlongPath()
        {
            while (currentWaypointIndex < waypoints.Length)
            {
                Vector3 targetPosition = waypoints[currentWaypointIndex].position;
                Vector3 direction = (targetPosition - transform.position).normalized;
                
                transform.position += direction * currentSpeed * Time.deltaTime;
                
                // Check if reached waypoint
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    currentWaypointIndex++;
                }
                
                yield return null;
            }
            
            // Reached the end of the path
            OnEnemyReachedEnd?.Invoke(this);
            Destroy(gameObject);
        }
        
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            // Trigger defeat event
            OnEnemyDefeated?.Invoke(this);
            
            // Spawn death effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayerBase"))
            {
                // Deal damage to player base
                PlayerBase playerBase = other.GetComponent<PlayerBase>();
                if (playerBase != null)
                {
                    playerBase.TakeDamage(currentDamage);
                }
                
                Destroy(gameObject);
            }
        }
    }
} 