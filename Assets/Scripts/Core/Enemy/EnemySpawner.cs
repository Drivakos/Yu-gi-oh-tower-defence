using UnityEngine;
using System.Collections;
using YuGiOhTowerDefense.Core.Pathfinding;

namespace YuGiOhTowerDefense.Core.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxEnemies = 10;
        [SerializeField] private Vector2Int spawnPosition;
        [SerializeField] private Vector2Int targetPosition;
        
        [Header("Wave Settings")]
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float waveInterval = 10f;
        
        private int currentEnemies;
        private bool isWaveInProgress;
        private float currentHealthMultiplier = 1f;
        private float currentDamageMultiplier = 1f;
        
        public bool IsWaveInProgress => isWaveInProgress;
        
        private void Start()
        {
            // Wait for WaveManager to start waves
        }
        
        public void SetWaveSettings(int enemiesCount, float healthMultiplier, float damageMultiplier)
        {
            enemiesPerWave = enemiesCount;
            currentHealthMultiplier = healthMultiplier;
            currentDamageMultiplier = damageMultiplier;
        }
        
        public void StartWave()
        {
            if (!isWaveInProgress)
            {
                StartCoroutine(SpawnWave());
            }
        }
        
        private IEnumerator SpawnWave()
        {
            isWaveInProgress = true;
            
            for (int i = 0; i < enemiesPerWave; i++)
            {
                if (currentEnemies < maxEnemies)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(spawnInterval);
                }
                else
                {
                    yield return new WaitUntil(() => currentEnemies < maxEnemies);
                }
            }
            
            // Wait for all enemies to be defeated
            yield return new WaitUntil(() => currentEnemies == 0);
            
            isWaveInProgress = false;
            
            // Notify WaveManager that this spawner's wave is complete
            WaveManager waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.OnEnemySpawnerWaveComplete(this);
            }
        }
        
        private void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab not assigned!");
                return;
            }
            
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(spawnPosition.x, 0, spawnPosition.y), Quaternion.identity);
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            
            if (enemyComponent != null)
            {
                // Apply wave multipliers to enemy stats
                enemyComponent.SetStatsMultipliers(currentHealthMultiplier, currentDamageMultiplier);
                
                enemyComponent.Initialize(spawnPosition, targetPosition);
                currentEnemies++;
                
                // Subscribe to enemy death event
                enemyComponent.OnDeath += HandleEnemyDeath;
            }
            else
            {
                Debug.LogError("Enemy prefab missing Enemy component!");
                Destroy(enemy);
            }
        }
        
        private void HandleEnemyDeath(Enemy enemy)
        {
            currentEnemies--;
            enemy.OnDeath -= HandleEnemyDeath;
        }
    }
} 