using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int startingDuelPoints = 1000;
        [SerializeField] private float duelPointsPerSecond = 10f;
        [SerializeField] private int startingLifePoints = 8000;
        
        [Header("Wave Settings")]
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private int currentWave = 0;
        
        [Header("References")]
        [SerializeField] private Transform monsterContainer;
        [SerializeField] private Transform enemyContainer;
        
        private float currentDuelPoints;
        private int currentLifePoints;
        private float waveTimer;
        private bool isWaveInProgress;
        private List<Monster> activeMonsters = new List<Monster>();
        private List<Card> activeCards = new List<Card>();

        public int CurrentWave => currentWave;
        public float CurrentDuelPoints => currentDuelPoints;
        public int CurrentLifePoints => currentLifePoints;
        public bool IsWaveInProgress => isWaveInProgress;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (isWaveInProgress)
            {
                UpdateWave();
            }
            else
            {
                UpdateWaveTimer();
            }

            GenerateDuelPoints();
        }

        private void InitializeGame()
        {
            currentDuelPoints = startingDuelPoints;
            currentLifePoints = startingLifePoints;
            waveTimer = timeBetweenWaves;
            isWaveInProgress = false;
        }

        private void GenerateDuelPoints()
        {
            currentDuelPoints += duelPointsPerSecond * Time.deltaTime;
        }

        private void UpdateWaveTimer()
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
            {
                StartNextWave();
            }
        }

        private void UpdateWave()
        {
            // Check if all enemies are defeated
            if (enemyContainer.childCount == 0)
            {
                EndWave();
            }
        }

        public void StartNextWave()
        {
            currentWave++;
            isWaveInProgress = true;
            waveTimer = timeBetweenWaves;
            
            // TODO: Spawn wave enemies
            SpawnWaveEnemies();
        }

        private void EndWave()
        {
            isWaveInProgress = false;
            waveTimer = timeBetweenWaves;
            
            // Reward player for completing wave
            RewardWaveCompletion();
        }

        private void SpawnWaveEnemies()
        {
            // TODO: Implement enemy spawning logic based on wave number
        }

        private void RewardWaveCompletion()
        {
            // Base reward
            currentDuelPoints += 500;
            
            // Bonus for no damage taken
            if (currentLifePoints == startingLifePoints)
            {
                currentDuelPoints += 200;
            }
        }

        public bool CanAffordMonster(int cost)
        {
            return currentDuelPoints >= cost;
        }

        public void SpendDuelPoints(int amount)
        {
            if (currentDuelPoints >= amount)
            {
                currentDuelPoints -= amount;
            }
        }

        public void TakeDamage(int damage)
        {
            currentLifePoints -= damage;
            
            if (currentLifePoints <= 0)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            // TODO: Implement game over logic
            Debug.Log("Game Over!");
        }

        public void RegisterMonster(Monster monster)
        {
            if (!activeMonsters.Contains(monster))
            {
                activeMonsters.Add(monster);
                monster.OnMonsterDestroyed += UnregisterMonster;
            }
        }

        private void UnregisterMonster(Monster monster)
        {
            activeMonsters.Remove(monster);
            monster.OnMonsterDestroyed -= UnregisterMonster;
        }

        public void RegisterCard(Card card)
        {
            if (!activeCards.Contains(card))
            {
                activeCards.Add(card);
            }
        }

        public void UnregisterCard(Card card)
        {
            activeCards.Remove(card);
        }
    }
} 