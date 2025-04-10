using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core.Enemy
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private float baseEnemyHealth = 100f;
        [SerializeField] private float baseEnemyDamage = 10f;
        [SerializeField] private float healthScaling = 1.2f;
        [SerializeField] private float damageScaling = 1.1f;
        [SerializeField] private float enemyCountScaling = 1.1f;
        
        [Header("Wave Types")]
        [SerializeField] private List<EnemyType> availableEnemyTypes;
        
        private int currentWave = 0;
        private List<EnemySpawner> spawners = new List<EnemySpawner>();
        
        public int CurrentWave => currentWave;
        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        
        private void Start()
        {
            // Find all enemy spawners in the scene
            spawners.AddRange(FindObjectsOfType<EnemySpawner>());
            
            // Start the first wave
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            currentWave++;
            Debug.Log($"Starting Wave {currentWave}");
            
            // Calculate wave difficulty
            float waveHealthMultiplier = Mathf.Pow(healthScaling, currentWave - 1);
            float waveDamageMultiplier = Mathf.Pow(damageScaling, currentWave - 1);
            int waveEnemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyCountScaling, currentWave - 1));
            
            // Configure spawners for this wave
            foreach (EnemySpawner spawner in spawners)
            {
                spawner.SetWaveSettings(waveEnemyCount, waveHealthMultiplier, waveDamageMultiplier);
                spawner.StartWave();
            }
            
            OnWaveStarted?.Invoke(currentWave);
        }
        
        public void OnEnemySpawnerWaveComplete(EnemySpawner spawner)
        {
            // Check if all spawners have completed their waves
            bool allWavesComplete = true;
            foreach (EnemySpawner s in spawners)
            {
                if (s.IsWaveInProgress)
                {
                    allWavesComplete = false;
                    break;
                }
            }
            
            if (allWavesComplete)
            {
                OnWaveCompleted?.Invoke(currentWave);
            }
        }
    }
    
    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
        public float spawnWeight = 1f;
    }
} 