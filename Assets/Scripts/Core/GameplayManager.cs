using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.Core
{
    public class GameplayManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float gameStartDelay = 2f;
        [SerializeField] private float waveStartDelay = 5f;
        [SerializeField] private float waveEndDelay = 3f;
        [SerializeField] private int startingWave = 1;
        [SerializeField] private int maxWaves = 10;
        
        [Header("Resource Settings")]
        [SerializeField] private int startingCurrency = 1000;
        [SerializeField] private int currencyPerWave = 100;
        [SerializeField] private float currencyMultiplier = 1.1f;
        
        [Header("Card Settings")]
        [SerializeField] private int startingHandSize = 5;
        [SerializeField] private float cardDrawInterval = 10f;
        [SerializeField] private int maxHandSize = 7;
        
        [Header("References")]
        [SerializeField] private CardPoolManager cardPoolManager;
        [SerializeField] private BossSpawner bossSpawner;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private BossRewardUI bossRewardUI;
        [SerializeField] private MobileHandUI handUI;
        
        private int currentWave;
        private int currentCurrency;
        private bool isGameActive;
        private bool isWaveActive;
        private List<YuGiOhCard> playerDeck = new List<YuGiOhCard>();
        
        private void Awake()
        {
            // Validate references
            if (cardPoolManager == null)
            {
                cardPoolManager = GetComponent<CardPoolManager>();
                if (cardPoolManager == null)
                {
                    Debug.LogError("CardPoolManager not found!");
                }
            }
            
            if (bossSpawner == null)
            {
                bossSpawner = GetComponent<BossSpawner>();
                if (bossSpawner == null)
                {
                    Debug.LogError("BossSpawner not found!");
                }
            }
            
            if (waveManager == null)
            {
                waveManager = GetComponent<WaveManager>();
                if (waveManager == null)
                {
                    Debug.LogError("WaveManager not found!");
                }
            }
            
            if (bossRewardUI == null)
            {
                bossRewardUI = FindObjectOfType<BossRewardUI>();
                if (bossRewardUI == null)
                {
                    Debug.LogError("BossRewardUI not found!");
                }
            }
            
            if (handUI == null)
            {
                handUI = FindObjectOfType<MobileHandUI>();
                if (handUI == null)
                {
                    Debug.LogError("MobileHandUI not found!");
                }
            }
        }
        
        private void Start()
        {
            // Initialize game state
            currentWave = startingWave - 1;
            currentCurrency = startingCurrency;
            isGameActive = false;
            isWaveActive = false;
            
            // Initialize card pools
            if (cardPoolManager != null)
            {
                cardPoolManager.InitializeCardPools();
            }
            
            // Start game after delay
            StartCoroutine(StartGame());
        }
        
        private IEnumerator StartGame()
        {
            // Wait for initial delay
            yield return new WaitForSeconds(gameStartDelay);
            
            // Set game as active
            isGameActive = true;
            
            // Initialize player deck
            InitializePlayerDeck();
            
            // Start first wave
            StartNextWave();
            
            // Start card drawing
            StartCoroutine(DrawCardsRoutine());
        }
        
        private void InitializePlayerDeck()
        {
            if (cardPoolManager == null)
            {
                return;
            }
            
            // Generate starting cards
            var startingCards = cardPoolManager.GeneratePlayerCards(startingHandSize);
            playerDeck.AddRange(startingCards);
            
            // Update hand UI
            if (handUI != null)
            {
                handUI.UpdateHand(playerDeck);
            }
        }
        
        private IEnumerator DrawCardsRoutine()
        {
            while (isGameActive)
            {
                // Wait for draw interval
                yield return new WaitForSeconds(cardDrawInterval);
                
                // Draw a card if hand is not full
                if (playerDeck.Count < maxHandSize)
                {
                    var newCards = cardPoolManager.GeneratePlayerCards(1);
                    if (newCards.Count > 0)
                    {
                        playerDeck.Add(newCards[0]);
                        
                        // Update hand UI
                        if (handUI != null)
                        {
                            handUI.UpdateHand(playerDeck);
                        }
                    }
                }
            }
        }
        
        public void StartNextWave()
        {
            if (!isGameActive || isWaveActive)
            {
                return;
            }
            
            // Increment wave number
            currentWave++;
            
            // Check if game is complete
            if (currentWave > maxWaves)
            {
                EndGame(true);
                return;
            }
            
            // Start wave
            StartCoroutine(StartWave());
        }
        
        private IEnumerator StartWave()
        {
            isWaveActive = true;
            
            // Wait for wave start delay
            yield return new WaitForSeconds(waveStartDelay);
            
            // Check if this is a boss wave
            if (bossSpawner != null && bossSpawner.IsBossWave(currentWave))
            {
                // Start boss wave
                bossSpawner.StartBossWave(currentWave);
            }
            else
            {
                // Start regular wave
                if (waveManager != null)
                {
                    waveManager.StartWave(currentWave);
                }
            }
        }
        
        public void EndWave()
        {
            if (!isWaveActive)
            {
                return;
            }
            
            isWaveActive = false;
            
            // Award currency
            int waveReward = Mathf.RoundToInt(currencyPerWave * Mathf.Pow(currencyMultiplier, currentWave - 1));
            AddCurrency(waveReward);
            
            // Start next wave after delay
            StartCoroutine(EndWaveSequence());
        }
        
        private IEnumerator EndWaveSequence()
        {
            // Wait for wave end delay
            yield return new WaitForSeconds(waveEndDelay);
            
            // Start next wave
            StartNextWave();
        }
        
        public void OnBossDefeated(string bossId)
        {
            // Show boss rewards
            if (bossRewardUI != null)
            {
                bossRewardUI.ShowBossRewards(bossId);
            }
            
            // End wave
            EndWave();
        }
        
        public void AddCurrency(int amount)
        {
            currentCurrency += amount;
            
            // Update UI
            // TODO: Update currency UI
        }
        
        public bool SpendCurrency(int amount)
        {
            if (currentCurrency < amount)
            {
                return false;
            }
            
            currentCurrency -= amount;
            
            // Update UI
            // TODO: Update currency UI
            
            return true;
        }
        
        private void EndGame(bool victory)
        {
            isGameActive = false;
            
            // Stop all coroutines
            StopAllCoroutines();
            
            // Show end game UI
            // TODO: Show victory/defeat UI
            
            // Save player progress
            SavePlayerProgress();
        }
        
        private void SavePlayerProgress()
        {
            // TODO: Save player deck, currency, and wave progress
        }
        
        public void OnGameOver()
        {
            EndGame(false);
        }
        
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        public int GetCurrentWave()
        {
            return currentWave;
        }
        
        public int GetCurrentCurrency()
        {
            return currentCurrency;
        }
        
        public List<YuGiOhCard> GetPlayerDeck()
        {
            return playerDeck;
        }
    }
} 