using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOh.Cards;
using YuGiOh.Gameplay;

namespace YuGiOh.Managers
{
    public class WaveManager : BaseManager
    {
        [Header("Wave Settings")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private float spawnInterval = 2f;
        
        [Header("Enemy Settings")]
        [SerializeField] private List<MonsterCard> enemyPrefabs;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform objectivePoint;
        
        private bool isWaveActive = false;
        private int enemiesRemaining = 0;
        private int enemiesSpawned = 0;
        
        private void Start()
        {
            StartCoroutine(WaveSpawner());
        }
        
        private IEnumerator WaveSpawner()
        {
            while (true)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                
                if (!isWaveActive)
                {
                    StartWave();
                }
            }
        }
        
        private void StartWave()
        {
            currentWave++;
            isWaveActive = true;
            enemiesRemaining = CalculateWaveSize();
            enemiesSpawned = 0;
            
            StartCoroutine(SpawnEnemies());
        }
        
        private int CalculateWaveSize()
        {
            // Basic wave size calculation - can be made more complex
            return 5 + (currentWave * 2);
        }
        
        private IEnumerator SpawnEnemies()
        {
            while (enemiesSpawned < enemiesRemaining)
            {
                SpawnEnemy();
                enemiesSpawned++;
                
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        private void SpawnEnemy()
        {
            if (enemyPrefabs.Count == 0 || spawnPoint == null)
            {
                Debug.LogError("Enemy prefabs or spawn point not set!");
                return;
            }
            
            // Randomly select an enemy prefab
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            MonsterCard enemyPrefab = enemyPrefabs[randomIndex];
            
            // Spawn the enemy
            MonsterCard enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.SetAsEnemy(true);
            
            // Set objective if available
            if (objectivePoint != null)
            {
                PathFollower pathFollower = enemy.GetComponent<PathFollower>();
                if (pathFollower != null)
                {
                    pathFollower.SetObjective(objectivePoint);
                }
            }
        }
        
        public void OnEnemyDefeated()
        {
            enemiesRemaining--;
            
            if (enemiesRemaining <= 0)
            {
                isWaveActive = false;
                // Wave completed logic here
            }
        }
        
        public int GetCurrentWave()
        {
            return currentWave;
        }
        
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        public int GetEnemiesRemaining()
        {
            return enemiesRemaining;
        }
    }
} 