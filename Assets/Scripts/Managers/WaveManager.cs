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
        [SerializeField] private float waveStartDelay = 5f;
        [SerializeField] private float waveEndDelay = 3f;
        
        [Header("Enemy Settings")]
        [SerializeField] private List<MonsterCard> enemyPrefabs;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform objectivePoint;
        [SerializeField] private AnimationCurve enemyHealthCurve = AnimationCurve.Linear(0, 1, 100, 5);
        [SerializeField] private AnimationCurve enemySpeedCurve = AnimationCurve.Linear(0, 1, 100, 2);
        [SerializeField] private AnimationCurve enemyCountCurve = AnimationCurve.Linear(0, 1, 100, 3);
        
        private bool isWaveActive = false;
        private int enemiesRemaining = 0;
        private int enemiesSpawned = 0;
        private GameManager gameManager;
        
        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        public event System.Action<int> OnEnemySpawned;
        public event System.Action<int> OnEnemyDefeated;
        
        protected override void Awake()
        {
            base.Awake();
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("WaveManager: GameManager not found!");
            }
        }
        
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
            
            OnWaveStarted?.Invoke(currentWave);
            StartCoroutine(SpawnEnemies());
        }
        
        private int CalculateWaveSize()
        {
            return Mathf.RoundToInt(enemyCountCurve.Evaluate(currentWave));
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
            
            // Calculate enemy stats based on wave number
            float healthMultiplier = enemyHealthCurve.Evaluate(currentWave);
            float speedMultiplier = enemySpeedCurve.Evaluate(currentWave);
            
            // Spawn the enemy
            MonsterCard enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.SetAsEnemy(true);
            enemy.ModifyStats(healthMultiplier, speedMultiplier);
            
            // Set objective if available
            if (objectivePoint != null)
            {
                PathFollower pathFollower = enemy.GetComponent<PathFollower>();
                if (pathFollower != null)
                {
                    pathFollower.SetObjective(objectivePoint);
                }
            }
            
            OnEnemySpawned?.Invoke(enemiesRemaining);
        }
        
        public void OnEnemyDefeated()
        {
            enemiesRemaining--;
            OnEnemyDefeated?.Invoke(enemiesRemaining);
            
            if (enemiesRemaining <= 0)
            {
                isWaveActive = false;
                OnWaveCompleted?.Invoke(currentWave);
                
                // Award DP for completing the wave
                int waveReward = CalculateWaveReward();
                if (gameManager != null)
                {
                    gameManager.AddDP(waveReward);
                }
            }
        }
        
        private int CalculateWaveReward()
        {
            return 100 + (currentWave * 50);
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
        
        // New methods for spawn and objective points
        public void SetSpawnPoint(Transform point)
        {
            spawnPoint = point;
        }
        
        public void SetObjectivePoint(Transform point)
        {
            objectivePoint = point;
        }
        
        public void SetGameManager(GameManager manager)
        {
            gameManager = manager;
        }
        
        public void AddEnemyPrefab(MonsterCard prefab)
        {
            if (!enemyPrefabs.Contains(prefab))
            {
                enemyPrefabs.Add(prefab);
            }
        }
        
        public void ClearEnemyPrefabs()
        {
            enemyPrefabs.Clear();
        }
    }
} 