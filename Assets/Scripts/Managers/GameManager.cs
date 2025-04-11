using UnityEngine;
using System.Collections.Generic;
using YuGiOh.Cards;
using YuGiOh.Gameplay;

namespace YuGiOh.Managers
{
    public class GameManager : BaseManager
    {
        [Header("Game Settings")]
        [SerializeField] private int startingDuelPoints = 1000;
        [SerializeField] private float duelPointsPerSecond = 10f;
        [SerializeField] private int startingLifePoints = 8000;
        
        [Header("References")]
        [SerializeField] private Transform monsterContainer;
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private WaveManager waveManager;
        
        private float currentDuelPoints;
        private int currentLifePoints;
        private List<MonsterCard> activeMonsters = new List<MonsterCard>();
        private List<Card> activeCards = new List<Card>();
        
        public float CurrentDuelPoints => currentDuelPoints;
        public int CurrentLifePoints => currentLifePoints;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
                if (waveManager == null)
                {
                    Debug.LogError("GameManager: WaveManager not found!");
                }
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void Update()
        {
            GenerateDuelPoints();
        }
        
        private void InitializeGame()
        {
            currentDuelPoints = startingDuelPoints;
            currentLifePoints = startingLifePoints;
        }
        
        private void GenerateDuelPoints()
        {
            currentDuelPoints += duelPointsPerSecond * Time.deltaTime;
        }
        
        public void AddDP(int amount)
        {
            currentDuelPoints += amount;
        }
        
        public bool SpendDP(int amount)
        {
            if (currentDuelPoints >= amount)
            {
                currentDuelPoints -= amount;
                return true;
            }
            return false;
        }
        
        public void TakeDamage(int damage)
        {
            currentLifePoints = Mathf.Max(0, currentLifePoints - damage);
            
            if (currentLifePoints <= 0)
            {
                GameOver();
            }
        }
        
        private void GameOver()
        {
            // Stop all game systems
            StopAllCoroutines();
            
            // Disable player input
            isGameActive = false;
            
            // Show game over UI
            if (gameOverMenu != null)
            {
                gameOverMenu.ShowMainPanel();
                gameOverMenu.UpdateGameOverInfo(currentWave, CalculateFinalScore());
                gameOverMenu.FadeBackground(true);
            }
            
            // Save player progress
            SavePlayerProgress();
            
            // Play game over sound
            if (audioManager != null)
            {
                audioManager.PlayGameOverSound();
            }
            
            // Trigger game over event
            OnGameOver?.Invoke();
        }
        
        private int CalculateFinalScore()
        {
            int score = 0;
            
            // Base score from waves completed
            score += currentWave * 1000;
            
            // Bonus for remaining life points
            score += currentLifePoints * 100;
            
            // Bonus for remaining duel points
            score += currentDuelPoints * 10;
            
            // Bonus for cards in deck
            score += playerDeck.Count * 50;
            
            return score;
        }
        
        private void SavePlayerProgress()
        {
            if (saveManager == null) return;
            
            // Create save data
            PlayerProgress progress = new PlayerProgress
            {
                highestWave = Mathf.Max(saveManager.GetHighestWave(), currentWave),
                totalScore = saveManager.GetTotalScore() + CalculateFinalScore(),
                unlockedCards = saveManager.GetUnlockedCards(),
                unlockedItems = saveManager.GetUnlockedItems()
            };
            
            // Save progress
            saveManager.SaveProgress(progress);
        }
        
        public void RegisterMonster(MonsterCard monster)
        {
            if (!activeMonsters.Contains(monster))
            {
                activeMonsters.Add(monster);
            }
        }
        
        public void UnregisterMonster(MonsterCard monster)
        {
            activeMonsters.Remove(monster);
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
        
        public bool CanAffordMonster(int cost)
        {
            return currentDuelPoints >= cost;
        }

        public void SpendDuelPoints(int amount)
        {
            currentDuelPoints = Mathf.Max(0, currentDuelPoints - amount);
        }

        public void AddDuelPoints(int amount)
        {
            currentDuelPoints = Mathf.Min(9999, currentDuelPoints + amount);
        }

        public int GetCurrentDuelPoints()
        {
            return (int)currentDuelPoints;
        }

        public List<Card> GetActiveCards()
        {
            return activeCards;
        }
        
        // New methods for container setup
        public void SetMonsterContainer(Transform container)
        {
            monsterContainer = container;
        }
        
        public void SetEnemyContainer(Transform container)
        {
            enemyContainer = container;
        }
        
        public void SetWaveManager(WaveManager manager)
        {
            waveManager = manager;
        }
        
        public Transform GetMonsterContainer()
        {
            return monsterContainer;
        }
        
        public Transform GetEnemyContainer()
        {
            return enemyContainer;
        }
    }
} 