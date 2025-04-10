using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core.Enemy;
using YuGiOhTowerDefense.Core.Player;

namespace YuGiOhTowerDefense.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int startingDuelPoints = 1000;
        [SerializeField] private float duelPointsPerSecond = 10f;
        [SerializeField] private int startingLifePoints = 8000;
        [SerializeField] private int startingLives = 3;
        [SerializeField] private int startingGold = 100;
        
        [Header("Wave Settings")]
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private int currentWave = 0;
        
        [Header("Card Settings")]
        [SerializeField] private int maxDeckSize = 40;
        [SerializeField] private int maxHandSize = 5;
        
        [Header("References")]
        [SerializeField] private Transform monsterContainer;
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private Player player;
        [SerializeField] private WaveManager waveManager;
        
        private float currentDuelPoints;
        private int currentLifePoints;
        private float waveTimer;
        private bool isWaveInProgress;
        private List<Monster> activeMonsters = new List<Monster>();
        private List<Card> activeCards = new List<Card>();
        private int currentLives;
        private int currentGold;
        private bool isGameOver;
        
        private List<YuGiOhCard> playerDeck = new List<YuGiOhCard>();
        private List<YuGiOhCard> playerHand = new List<YuGiOhCard>();
        private List<YuGiOhCard> playerGraveyard = new List<YuGiOhCard>();
        
        public int CurrentWave => currentWave;
        public float CurrentDuelPoints => currentDuelPoints;
        public int CurrentLifePoints => currentLifePoints;
        public bool IsWaveInProgress => isWaveInProgress;
        public int CurrentLives => currentLives;
        public int CurrentGold => currentGold;
        public bool IsGameOver => isGameOver;
        
        public event System.Action<int> OnLivesChanged;
        public event System.Action<int> OnGoldChanged;
        public event System.Action OnGameOver;
        public event System.Action<YuGiOhCard> OnCardAddedToDeck;
        public event System.Action<YuGiOhCard> OnCardAddedToHand;
        public event System.Action<YuGiOhCard> OnCardPlayed;
        public event System.Action<YuGiOhCard> OnCardSentToGraveyard;

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

            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
            
            InitializeGame();
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
            currentLives = startingLives;
            currentGold = startingGold;
            isGameOver = false;
            
            // Subscribe to events
            if (player != null)
            {
                player.OnPlayerDeath += HandlePlayerDeath;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted += HandleWaveCompleted;
            }
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

        private void HandlePlayerDeath()
        {
            currentLives--;
            OnLivesChanged?.Invoke(currentLives);
            
            if (currentLives <= 0)
            {
                GameOver();
            }
            else
            {
                // Handle player respawn or other logic
                player.Heal(player.GetHealthPercentage() * 100); // Full heal
            }
        }
        
        private void HandleWaveCompleted(int waveNumber)
        {
            // Reward player for completing wave
            int waveReward = 50 + (waveNumber * 10);
            AddGold(waveReward);
            
            // Start next wave after a delay
            StartCoroutine(StartNextWaveAfterDelay(5f));
        }
        
        private System.Collections.IEnumerator StartNextWaveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            waveManager.StartNextWave();
        }
        
        public void AddGold(int amount)
        {
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }
        
        public bool SpendGold(int amount)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                return true;
            }
            return false;
        }
        
        private void GameOver()
        {
            isGameOver = true;
            OnGameOver?.Invoke();
            
            // Handle game over state
            Debug.Log("Game Over!");
            Time.timeScale = 0f; // Pause the game
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

        public void AddCardToDeck(YuGiOhCard card)
        {
            if (playerDeck.Count < maxDeckSize)
            {
                playerDeck.Add(card);
                OnCardAddedToDeck?.Invoke(card);
            }
            else
            {
                Debug.Log("Deck is full!");
            }
        }
        
        public void DrawCard()
        {
            if (playerDeck.Count > 0 && playerHand.Count < maxHandSize)
            {
                YuGiOhCard card = playerDeck[0];
                playerDeck.RemoveAt(0);
                playerHand.Add(card);
                OnCardAddedToHand?.Invoke(card);
            }
        }
        
        public void PlayCard(YuGiOhCard card)
        {
            if (playerHand.Contains(card))
            {
                playerHand.Remove(card);
                OnCardPlayed?.Invoke(card);
                
                // Handle card effect
                HandleCardEffect(card);
            }
        }
        
        private void HandleCardEffect(YuGiOhCard card)
        {
            // TODO: Implement card effect handling
            // This will depend on the specific card type and effect
            
            // For now, just send the card to graveyard
            SendCardToGraveyard(card);
        }
        
        public void SendCardToGraveyard(YuGiOhCard card)
        {
            playerGraveyard.Add(card);
            OnCardSentToGraveyard?.Invoke(card);
        }
        
        public void ShuffleDeck()
        {
            // Fisher-Yates shuffle algorithm
            for (int i = playerDeck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                YuGiOhCard temp = playerDeck[i];
                playerDeck[i] = playerDeck[j];
                playerDeck[j] = temp;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (player != null)
            {
                player.OnPlayerDeath -= HandlePlayerDeath;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted -= HandleWaveCompleted;
            }
        }
    }
} 