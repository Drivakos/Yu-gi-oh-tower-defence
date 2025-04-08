using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemySpawnInfo> enemies;
        public float timeBetweenSpawns = 1f;
        public float timeAfterWave = 5f;
    }
    
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public float spawnDelay = 0f;
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public int duelPointsRewardMultiplier = 1;
    }
    
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private List<Wave> waves;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float timeBetweenWaves = 10f;
        
        [Header("UI References")]
        [SerializeField] private GameObject waveAnnouncementPanel;
        [SerializeField] private TMPro.TextMeshProUGUI waveAnnouncementText;
        [SerializeField] private TMPro.TextMeshProUGUI waveCountText;
        [SerializeField] private GameObject waveCompletePanel;
        [SerializeField] private TMPro.TextMeshProUGUI waveCompleteText;
        
        [Header("Animation Settings")]
        [SerializeField] private float announcementDuration = 3f;
        [SerializeField] private float announcementFadeSpeed = 2f;
        
        private int currentWaveIndex = -1;
        private int enemiesRemaining = 0;
        private bool isWaveActive = false;
        private Coroutine waveCoroutine;
        private CardManager cardManager;
        
        private void Awake()
        {
            cardManager = FindObjectOfType<CardManager>();
            if (cardManager == null)
            {
                Debug.LogError("WaveManager: CardManager not found!");
            }
            
            // Hide UI panels
            if (waveAnnouncementPanel != null)
            {
                waveAnnouncementPanel.SetActive(false);
            }
            
            if (waveCompletePanel != null)
            {
                waveCompletePanel.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Update wave count display
            UpdateWaveCountDisplay();
            
            // Start first wave after delay
            StartCoroutine(StartFirstWave());
        }
        
        private IEnumerator StartFirstWave()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            if (isWaveActive)
            {
                return;
            }
            
            currentWaveIndex++;
            
            // Check if all waves are complete
            if (currentWaveIndex >= waves.Count)
            {
                // Game complete
                Debug.Log("All waves complete! Game won!");
                return;
            }
            
            // Start the wave
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
            }
            
            waveCoroutine = StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        
        private IEnumerator SpawnWave(Wave wave)
        {
            isWaveActive = true;
            
            // Show wave announcement
            ShowWaveAnnouncement(wave.waveName);
            
            // Wait for announcement to finish
            yield return new WaitForSeconds(announcementDuration);
            
            // Update wave count display
            UpdateWaveCountDisplay();
            
            // Calculate total enemies
            enemiesRemaining = 0;
            foreach (EnemySpawnInfo enemyInfo in wave.enemies)
            {
                enemiesRemaining += enemyInfo.count;
            }
            
            // Spawn enemies
            foreach (EnemySpawnInfo enemyInfo in wave.enemies)
            {
                for (int i = 0; i < enemyInfo.count; i++)
                {
                    // Wait for spawn delay
                    yield return new WaitForSeconds(enemyInfo.spawnDelay);
                    
                    // Spawn enemy
                    SpawnEnemy(enemyInfo);
                    
                    // Wait between spawns
                    yield return new WaitForSeconds(wave.timeBetweenSpawns);
                }
            }
            
            // Wait for all enemies to be defeated
            while (enemiesRemaining > 0)
            {
                yield return new WaitForSeconds(1f);
            }
            
            // Wave complete
            isWaveActive = false;
            
            // Show wave complete announcement
            ShowWaveComplete();
            
            // Wait after wave
            yield return new WaitForSeconds(wave.timeAfterWave);
            
            // Start next wave
            StartNextWave();
        }
        
        private void SpawnEnemy(EnemySpawnInfo enemyInfo)
        {
            if (enemyInfo.enemyPrefab == null || spawnPoints.Length == 0)
            {
                return;
            }
            
            // Select random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Instantiate enemy
            GameObject enemyObject = Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Set up enemy
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Set waypoints
                enemy.SetWaypoints(waypoints);
                
                // Apply multipliers
                enemy.SetHealthMultiplier(enemyInfo.healthMultiplier);
                enemy.SetSpeedMultiplier(enemyInfo.speedMultiplier);
                enemy.SetDuelPointsRewardMultiplier(enemyInfo.duelPointsRewardMultiplier);
                
                // Subscribe to enemy death event
                enemy.OnEnemyDeath += HandleEnemyDeath;
            }
        }
        
        private void HandleEnemyDeath()
        {
            enemiesRemaining--;
        }
        
        private void ShowWaveAnnouncement(string waveName)
        {
            if (waveAnnouncementPanel == null || waveAnnouncementText == null)
            {
                return;
            }
            
            // Set wave name
            waveAnnouncementText.text = $"Wave {currentWaveIndex + 1}: {waveName}";
            
            // Show panel
            waveAnnouncementPanel.SetActive(true);
            
            // Start fade out coroutine
            StartCoroutine(FadeOutAnnouncement());
        }
        
        private IEnumerator FadeOutAnnouncement()
        {
            yield return new WaitForSeconds(announcementDuration - 1f);
            
            // Get canvas group
            CanvasGroup canvasGroup = waveAnnouncementPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = waveAnnouncementPanel.AddComponent<CanvasGroup>();
            }
            
            // Fade out
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= announcementFadeSpeed * Time.deltaTime;
                canvasGroup.alpha = alpha;
                yield return null;
            }
            
            // Hide panel
            waveAnnouncementPanel.SetActive(false);
            canvasGroup.alpha = 1f;
        }
        
        private void ShowWaveComplete()
        {
            if (waveCompletePanel == null || waveCompleteText == null)
            {
                return;
            }
            
            // Set wave complete text
            waveCompleteText.text = $"Wave {currentWaveIndex + 1} Complete!";
            
            // Show panel
            waveCompletePanel.SetActive(true);
            
            // Hide panel after delay
            StartCoroutine(HideWaveComplete());
        }
        
        private IEnumerator HideWaveComplete()
        {
            yield return new WaitForSeconds(3f);
            
            // Hide panel
            waveCompletePanel.SetActive(false);
        }
        
        private void UpdateWaveCountDisplay()
        {
            if (waveCountText == null)
            {
                return;
            }
            
            // Update wave count text
            waveCountText.text = $"Wave: {currentWaveIndex + 1}/{waves.Count}";
        }
        
        public int GetCurrentWaveIndex()
        {
            return currentWaveIndex;
        }
        
        public int GetTotalWaves()
        {
            return waves.Count;
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