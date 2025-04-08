using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.Core
{
    public class LevelManager : MonoBehaviour
    {
        [System.Serializable]
        public class LevelData
        {
            public string levelName;
            public string description;
            public Sprite levelImage;
            public int startingDuelPoints = 1000;
            public int startingLifePoints = 8000;
            public List<WaveManager.Wave> waves;
            public List<string> availableMonsterIds;
            public List<string> availableCardIds;
            public bool isUnlocked = false;
        }

        [Header("Level Settings")]
        [SerializeField] private List<LevelData> levels;
        [SerializeField] private int currentLevelIndex = -1;
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private TileManager tileManager;
        [SerializeField] private LevelRatingSystem ratingSystem;
        [SerializeField] private LevelRatingUI ratingUI;
        
        [Header("Level Transition")]
        [SerializeField] private float levelTransitionDelay = 1.5f;
        [SerializeField] private GameObject levelTransitionPanel;
        [SerializeField] private TMPro.TextMeshProUGUI levelTransitionText;
        
        private LevelData currentLevel;
        private float levelStartTime;
        private int monstersPlaced = 0;
        private int cardsUsed = 0;
        private bool isLevelActive = false;
        
        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
            
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
            
            if (tileManager == null)
            {
                tileManager = FindObjectOfType<TileManager>();
            }
            
            if (ratingSystem == null)
            {
                ratingSystem = FindObjectOfType<LevelRatingSystem>();
            }
            
            if (ratingUI == null)
            {
                ratingUI = FindObjectOfType<LevelRatingUI>();
            }
            
            if (levelTransitionPanel != null)
            {
                levelTransitionPanel.SetActive(false);
            }
        }
        
        private void Start()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted += OnWaveCompleted;
            }
            
            // Unlock first level by default
            if (levels.Count > 0)
            {
                levels[0].isUnlocked = true;
            }
        }
        
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                Debug.LogError("Invalid level index: " + levelIndex);
                return;
            }
            
            currentLevelIndex = levelIndex;
            currentLevel = levels[levelIndex];
            
            // Reset level stats
            levelStartTime = Time.time;
            monstersPlaced = 0;
            cardsUsed = 0;
            isLevelActive = true;
            
            // Load level data
            if (gameManager != null)
            {
                gameManager.SetDuelPoints(currentLevel.startingDuelPoints);
                gameManager.SetLifePoints(currentLevel.startingLifePoints);
            }
            
            if (waveManager != null)
            {
                waveManager.SetWaves(currentLevel.waves);
            }
            
            if (tileManager != null)
            {
                tileManager.ResetTiles();
            }
            
            // Show level transition
            StartCoroutine(ShowLevelTransition());
        }
        
        private IEnumerator ShowLevelTransition()
        {
            if (levelTransitionPanel != null)
            {
                levelTransitionPanel.SetActive(true);
                
                if (levelTransitionText != null)
                {
                    levelTransitionText.text = "Level " + (currentLevelIndex + 1) + ": " + currentLevel.levelName;
                }
                
                yield return new WaitForSeconds(levelTransitionDelay);
                
                levelTransitionPanel.SetActive(false);
            }
            
            // Start the level
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
        
        public void LoadNextLevel()
        {
            if (currentLevelIndex < levels.Count - 1)
            {
                LoadLevel(currentLevelIndex + 1);
            }
            else
            {
                // Game completed
                if (gameManager != null)
                {
                    gameManager.OnGameCompleted();
                }
            }
        }
        
        public void RetryCurrentLevel()
        {
            if (currentLevelIndex >= 0)
            {
                LoadLevel(currentLevelIndex);
            }
        }
        
        public void ReturnToLevelSelect()
        {
            if (gameManager != null)
            {
                gameManager.ReturnToMainMenu();
            }
        }
        
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.GameOver && isLevelActive)
            {
                isLevelActive = false;
                
                // Level failed
                if (ratingUI != null)
                {
                    ratingUI.ShowRatingResults(
                        currentLevel.levelName,
                        0, // No stars for failure
                        0, // No reward for failure
                        false,
                        Time.time - levelStartTime,
                        monstersPlaced,
                        cardsUsed,
                        gameManager != null ? gameManager.GetDuelPoints() : 0
                    );
                }
            }
        }
        
        private void OnWaveCompleted(int waveIndex)
        {
            if (waveManager != null && waveIndex == waveManager.GetTotalWaves() - 1)
            {
                // All waves completed, level succeeded
                isLevelActive = false;
                
                float completionTime = Time.time - levelStartTime;
                int duelPointsEarned = gameManager != null ? gameManager.GetDuelPoints() : 0;
                
                // Calculate rating and reward
                if (ratingSystem != null)
                {
                    ratingSystem.OnLevelCompleted(
                        currentLevel.levelName,
                        completionTime,
                        monstersPlaced,
                        cardsUsed,
                        duelPointsEarned
                    );
                }
            }
        }
        
        public void OnMonsterPlaced()
        {
            if (isLevelActive)
            {
                monstersPlaced++;
            }
        }
        
        public void OnCardUsed()
        {
            if (isLevelActive)
            {
                cardsUsed++;
            }
        }
        
        public void ShowLevelRatingResults(string levelName, int stars, int reward, bool isNewRecord)
        {
            if (ratingUI != null)
            {
                ratingUI.ShowRatingResults(
                    levelName,
                    currentLevel.levelName,
                    stars,
                    reward,
                    isNewRecord,
                    Time.time - levelStartTime,
                    monstersPlaced,
                    cardsUsed,
                    gameManager != null ? gameManager.GetDuelPoints() : 0
                );
            }
        }
        
        public LevelData GetCurrentLevel()
        {
            return currentLevel;
        }
        
        public int GetCurrentLevelIndex()
        {
            return currentLevelIndex;
        }
        
        public List<LevelData> GetAllLevels()
        {
            return levels;
        }
        
        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < levels.Count)
            {
                return levels[levelIndex].isUnlocked;
            }
            return false;
        }
        
        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= OnGameStateChanged;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted -= OnWaveCompleted;
            }
        }
    }
} 