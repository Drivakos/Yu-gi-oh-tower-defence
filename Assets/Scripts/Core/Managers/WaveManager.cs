using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Monsters;

namespace YuGiOhTowerDefense.Managers
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private int currentWave = 0;
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private float timeBetweenEnemies = 1f;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private GameObject enemyPrefab;

        private float waveTimer;
        private float enemyTimer;
        private int enemiesSpawned;
        private bool isWaveActive;
        private List<Enemy> activeEnemies = new List<Enemy>();

        private void Start()
        {
            waveTimer = timeBetweenWaves;
            enemyTimer = timeBetweenEnemies;
        }

        private void Update()
        {
            if (!isWaveActive)
            {
                waveTimer -= Time.deltaTime;
                if (waveTimer <= 0)
                {
                    StartWave();
                }
            }
            else
            {
                if (enemiesSpawned < enemiesPerWave)
                {
                    enemyTimer -= Time.deltaTime;
                    if (enemyTimer <= 0)
                    {
                        SpawnEnemy();
                        enemyTimer = timeBetweenEnemies;
                    }
                }
                else if (activeEnemies.Count == 0)
                {
                    EndWave();
                }
            }
        }

        private void StartWave()
        {
            currentWave++;
            enemiesSpawned = 0;
            isWaveActive = true;
            Debug.Log($"Starting Wave {currentWave}");
        }

        private void SpawnEnemy()
        {
            if (spawnPoints.Length == 0) return;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                enemy.Initialize(waypoints);
                enemy.OnDeath += HandleEnemyDeath;
                activeEnemies.Add(enemy);
                enemiesSpawned++;
            }
        }

        private void HandleEnemyDeath(Enemy enemy)
        {
            enemy.OnDeath -= HandleEnemyDeath;
            activeEnemies.Remove(enemy);
        }

        private void EndWave()
        {
            isWaveActive = false;
            waveTimer = timeBetweenWaves;
            Debug.Log($"Wave {currentWave} completed!");
            
            // Notify GameManager to draw a card
            GameManager.Instance.DrawCard();
        }

        public void IncreaseWaveDifficulty()
        {
            enemiesPerWave += 2;
            timeBetweenEnemies = Mathf.Max(0.5f, timeBetweenEnemies - 0.1f);
        }
    }
} 