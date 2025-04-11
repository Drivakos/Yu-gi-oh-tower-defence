using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core.Enemy;
using YuGiOhTowerDefense.Core.UI;
using YuGiOhTowerDefense.Core.Score;

namespace YuGiOhTowerDefense.Core.Wave
{
    /// <summary>
    /// Manages the game's wave system
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static event Action<int> OnWaveChanged;
        public static event Action OnWaveComplete;
        public static event Action OnAllWavesComplete;
        
        [Header("Wave Settings")]
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private float timeBetweenEnemies = 1f;
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        
        private int currentWaveIndex = -1;
        private bool isWaveActive;
        private Coroutine waveCoroutine;
        
        private GameStateUI gameStateUI;
        private ScoreManager scoreManager;
        
        private void Start()
        {
            StartNextWave();
        }
        
        public void Initialize(GameStateUI ui, ScoreManager score)
        {
            gameStateUI = ui;
            scoreManager = score;
        }
        
        public void StartNextWave()
        {
            if (isWaveActive || currentWaveIndex >= waves.Count - 1) return;
            
            currentWaveIndex++;
            isWaveActive = true;
            OnWaveChanged?.Invoke(currentWaveIndex + 1);
            
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
            }
            
            waveCoroutine = StartCoroutine(SpawnWave());
        }
        
        private IEnumerator SpawnWave()
        {
            var currentWave = waves[currentWaveIndex];
            
            foreach (var enemyGroup in currentWave.enemyGroups)
            {
                for (int i = 0; i < enemyGroup.count; i++)
                {
                    SpawnEnemy(enemyGroup.enemyPrefab);
                    yield return new WaitForSeconds(timeBetweenEnemies);
                }
            }
            
            isWaveActive = false;
            OnWaveComplete?.Invoke();
            
            if (currentWaveIndex >= waves.Count - 1)
            {
                OnAllWavesComplete?.Invoke();
            }
            else
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                StartNextWave();
            }
        }
        
        private void SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null) return;
            
            var spawnPoint = GetRandomSpawnPoint();
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
        
        private Transform GetRandomSpawnPoint()
        {
            var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points found in the scene!");
                return transform;
            }
            
            return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform;
        }
        
        public int GetCurrentWave()
        {
            return currentWaveIndex + 1;
        }
        
        public int GetTotalWaves()
        {
            return waves.Count;
        }
        
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        private void EndWave()
        {
            // Add wave completion bonus
            scoreManager.AddWaveCompletionBonus(currentWaveIndex + 1);
            
            // Update UI
            gameStateUI.HideWaveStartPanel();
        }
        
        public void ResetWaves()
        {
            currentWaveIndex = -1;
            isWaveActive = false;
            
            // Stop all coroutines
            StopAllCoroutines();
            
            // Start new countdown
            StartNextWave();
        }
    }
    
    [Serializable]
    public class WaveData
    {
        public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    }
    
    [Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count;
    }
} 