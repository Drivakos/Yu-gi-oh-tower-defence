using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOh.Cards;

namespace YuGiOh.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private float timeBetweenEnemySpawns = 2f;
        [SerializeField] private int startingWave = 1;
        
        [Header("Wave Progression")]
        [SerializeField] private AnimationCurve enemyHealthCurve = AnimationCurve.Linear(0, 1, 100, 5);
        [SerializeField] private AnimationCurve enemySpeedCurve = AnimationCurve.Linear(0, 1, 100, 2);
        [SerializeField] private AnimationCurve enemyCountCurve = AnimationCurve.Linear(0, 1, 100, 3);
        
        [Header("Enemy Types")]
        [SerializeField] private List<MonsterCard> enemyPrefabs;
        [SerializeField] private List<float> enemyTypeWeights;
        
        private int currentWave;
        private bool isWaveInProgress;
        private int enemiesRemaining;
        private GameManager gameManager;
        
        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        public event System.Action<int> OnEnemySpawned;
        public event System.Action<int> OnEnemyDefeated;
        
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            currentWave = startingWave - 1;
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            if (isWaveInProgress) return;
            
            currentWave++;
            isWaveInProgress = true;
            
            // Calculate wave parameters
            int enemyCount = Mathf.RoundToInt(enemyCountCurve.Evaluate(currentWave));
            enemiesRemaining = enemyCount;
            
            OnWaveStarted?.Invoke(currentWave);
            StartCoroutine(SpawnWave(enemyCount));
        }
        
        private IEnumerator SpawnWave(int enemyCount)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(timeBetweenEnemySpawns);
            }
        }
        
        private void SpawnEnemy()
        {
            // Select random enemy type based on weights
            int selectedIndex = SelectRandomEnemyType();
            MonsterCard enemyPrefab = enemyPrefabs[selectedIndex];
            
            // Calculate enemy stats based on wave number
            float healthMultiplier = enemyHealthCurve.Evaluate(currentWave);
            float speedMultiplier = enemySpeedCurve.Evaluate(currentWave);
            
            // Spawn enemy at random position along the spawn line
            Vector3 spawnPosition = GetRandomSpawnPosition();
            MonsterCard enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Apply wave-based stat modifications
            enemy.ModifyStats(healthMultiplier, speedMultiplier);
            
            OnEnemySpawned?.Invoke(enemiesRemaining);
        }
        
        private int SelectRandomEnemyType()
        {
            float totalWeight = 0f;
            foreach (float weight in enemyTypeWeights)
            {
                totalWeight += weight;
            }
            
            float random = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            for (int i = 0; i < enemyTypeWeights.Count; i++)
            {
                currentWeight += enemyTypeWeights[i];
                if (random <= currentWeight)
                {
                    return i;
                }
            }
            
            return 0;
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            // TODO: Implement proper spawn position logic based on game map
            return new Vector3(Random.Range(-10f, 10f), 0, 20f);
        }
        
        public void OnEnemyDefeated()
        {
            enemiesRemaining--;
            OnEnemyDefeated?.Invoke(enemiesRemaining);
            
            if (enemiesRemaining <= 0)
            {
                CompleteWave();
            }
        }
        
        private void CompleteWave()
        {
            isWaveInProgress = false;
            OnWaveCompleted?.Invoke(currentWave);
            
            // Award DP for completing the wave
            int waveReward = CalculateWaveReward();
            gameManager.AddDP(waveReward);
            
            // Start next wave after delay
            StartCoroutine(StartNextWaveAfterDelay());
        }
        
        private int CalculateWaveReward()
        {
            // Base reward + bonus for higher waves
            return 100 + (currentWave * 50);
        }
        
        private IEnumerator StartNextWaveAfterDelay()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNextWave();
        }
        
        public int GetCurrentWave() => currentWave;
        public bool IsWaveInProgress() => isWaveInProgress;
        public int GetEnemiesRemaining() => enemiesRemaining;
    }
} 