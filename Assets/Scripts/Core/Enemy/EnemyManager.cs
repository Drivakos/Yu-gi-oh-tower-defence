using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core.Grid;

namespace YuGiOhTowerDefense.Core.Enemy
{
    /// <summary>
    /// Manages enemy spawning and behavior
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxEnemiesPerWave = 10;
        
        [Header("Wave Settings")]
        [SerializeField] private int startingWave = 1;
        [SerializeField] private float waveInterval = 30f;
        [SerializeField] private int enemiesPerWaveIncrease = 2;
        
        private List<EnemyController> activeEnemies = new List<EnemyController>();
        private int currentWave;
        private int enemiesRemainingInWave;
        private float nextSpawnTime;
        private float nextWaveTime;
        private bool isWaveActive;
        
        public int CurrentWave => currentWave;
        public int EnemiesRemainingInWave => enemiesRemainingInWave;
        public bool IsWaveActive => isWaveActive;
        
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action<EnemyController> OnEnemySpawned;
        public event Action<EnemyController> OnEnemyDefeated;
        
        private void Start()
        {
            InitializeWaveSystem();
        }
        
        private void Update()
        {
            if (!isWaveActive) return;
            
            HandleEnemySpawning();
            CheckWaveCompletion();
        }
        
        private void InitializeWaveSystem()
        {
            currentWave = startingWave;
            enemiesRemainingInWave = CalculateEnemiesForWave(currentWave);
            isWaveActive = false;
            nextWaveTime = Time.time + waveInterval;
        }
        
        private int CalculateEnemiesForWave(int wave)
        {
            return maxEnemiesPerWave + (wave - 1) * enemiesPerWaveIncrease;
        }
        
        private void HandleEnemySpawning()
        {
            if (Time.time >= nextSpawnTime && enemiesRemainingInWave > 0)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
        
        private void SpawnEnemy()
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points defined");
                return;
            }
            
            // Select random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Select random enemy prefab
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // Instantiate enemy
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            EnemyController enemy = enemyObject.GetComponent<EnemyController>();
            
            if (enemy != null)
            {
                enemy.Initialize(currentWave);
                activeEnemies.Add(enemy);
                enemiesRemainingInWave--;
                
                enemy.OnDefeated += HandleEnemyDefeated;
                enemy.OnReachedEnd += HandleEnemyReachedEnd;
                
                OnEnemySpawned?.Invoke(enemy);
            }
            else
            {
                Debug.LogError("Enemy prefab missing EnemyController component");
                Destroy(enemyObject);
            }
        }
        
        private void HandleEnemyDefeated(EnemyController enemy)
        {
            activeEnemies.Remove(enemy);
            OnEnemyDefeated?.Invoke(enemy);
            
            // Clean up event subscriptions
            enemy.OnDefeated -= HandleEnemyDefeated;
            enemy.OnReachedEnd -= HandleEnemyReachedEnd;
        }
        
        private void HandleEnemyReachedEnd(EnemyController enemy)
        {
            // Handle enemy reaching the end (damage player, etc.)
            // This will be implemented in the GameManager
            
            HandleEnemyDefeated(enemy);
        }
        
        private void CheckWaveCompletion()
        {
            if (enemiesRemainingInWave <= 0 && activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }
        
        private void CompleteWave()
        {
            isWaveActive = false;
            OnWaveCompleted?.Invoke(currentWave);
            
            // Prepare for next wave
            currentWave++;
            enemiesRemainingInWave = CalculateEnemiesForWave(currentWave);
            nextWaveTime = Time.time + waveInterval;
        }
        
        public void StartWave()
        {
            if (isWaveActive)
            {
                Debug.LogWarning("Wave already in progress");
                return;
            }
            
            isWaveActive = true;
            nextSpawnTime = Time.time;
            OnWaveStarted?.Invoke(currentWave);
        }
        
        public void StopWave()
        {
            if (!isWaveActive) return;
            
            isWaveActive = false;
            
            // Clean up remaining enemies
            foreach (EnemyController enemy in activeEnemies)
            {
                enemy.OnDefeated -= HandleEnemyDefeated;
                enemy.OnReachedEnd -= HandleEnemyReachedEnd;
                Destroy(enemy.gameObject);
            }
            
            activeEnemies.Clear();
        }
    }
} 