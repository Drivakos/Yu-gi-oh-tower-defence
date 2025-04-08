using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    public class BossSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class BossWave
        {
            public string bossId;
            public string bossName;
            public int waveNumber;
            public float spawnDelay = 5f;
            public List<string> minionCardIds = new List<string>();
            public int minionCount = 3;
            public float minionSpawnInterval = 2f;
        }
        
        [Header("Boss Waves")]
        [SerializeField] private List<BossWave> bossWaves = new List<BossWave>();
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float bossSpawnHeight = 2f;
        
        [Header("Minion Settings")]
        [SerializeField] private GameObject minionPrefab;
        [SerializeField] private float minionSpawnRadius = 5f;
        
        private CardPoolManager cardPoolManager;
        private Dictionary<string, BossWave> bossWaveMap = new Dictionary<string, BossWave>();
        private bool isSpawning;
        
        private void Awake()
        {
            cardPoolManager = GetComponent<CardPoolManager>();
            if (cardPoolManager == null)
            {
                Debug.LogError("CardPoolManager not found!");
            }
            
            // Initialize boss wave map
            foreach (var wave in bossWaves)
            {
                bossWaveMap[wave.bossId] = wave;
            }
        }
        
        public void StartBossWave(int waveNumber)
        {
            var bossWave = bossWaves.Find(w => w.waveNumber == waveNumber);
            if (bossWave == null)
            {
                Debug.LogWarning($"No boss wave found for wave number: {waveNumber}");
                return;
            }
            
            StartCoroutine(SpawnBossWave(bossWave));
        }
        
        private IEnumerator SpawnBossWave(BossWave wave)
        {
            isSpawning = true;
            
            // Wait for spawn delay
            yield return new WaitForSeconds(wave.spawnDelay);
            
            // Spawn boss
            Vector3 spawnPosition = GetBossSpawnPosition();
            GameObject bossObj = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            
            // Set up boss
            BossEnemy boss = bossObj.GetComponent<BossEnemy>();
            if (boss != null)
            {
                // TODO: Set boss properties
            }
            
            // Spawn minions
            StartCoroutine(SpawnMinions(wave));
            
            isSpawning = false;
        }
        
        private IEnumerator SpawnMinions(BossWave wave)
        {
            for (int i = 0; i < wave.minionCount; i++)
            {
                // Wait for spawn interval
                yield return new WaitForSeconds(wave.minionSpawnInterval);
                
                // Spawn minion
                Vector3 spawnPosition = GetMinionSpawnPosition();
                GameObject minionObj = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
                
                // Set up minion with card
                if (cardPoolManager != null && wave.minionCardIds.Count > 0)
                {
                    // Select random card from minion pool
                    int randomIndex = Random.Range(0, wave.minionCardIds.Count);
                    string cardId = wave.minionCardIds[randomIndex];
                    
                    YuGiOhCard card = cardPoolManager.GetCardById(cardId);
                    if (card != null)
                    {
                        MonsterCard monsterCard = minionObj.GetComponent<MonsterCard>();
                        if (monsterCard != null)
                        {
                            monsterCard.Initialize(card);
                        }
                    }
                }
            }
        }
        
        private Vector3 GetBossSpawnPosition()
        {
            // TODO: Implement proper spawn position logic
            return new Vector3(0, bossSpawnHeight, 0);
        }
        
        private Vector3 GetMinionSpawnPosition()
        {
            // Generate random position within spawn radius
            float randomAngle = Random.Range(0f, 360f);
            float randomRadius = Random.Range(0f, minionSpawnRadius);
            
            Vector3 offset = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomRadius,
                0,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomRadius
            );
            
            return GetBossSpawnPosition() + offset;
        }
        
        public bool IsBossWave(int waveNumber)
        {
            return bossWaves.Exists(w => w.waveNumber == waveNumber);
        }
        
        public BossWave GetBossWave(int waveNumber)
        {
            return bossWaves.Find(w => w.waveNumber == waveNumber);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw minion spawn radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetBossSpawnPosition(), minionSpawnRadius);
        }
    }
} 