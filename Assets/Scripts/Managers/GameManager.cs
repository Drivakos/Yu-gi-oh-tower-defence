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
            // TODO: Implement game over logic
            Debug.Log("Game Over!");
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