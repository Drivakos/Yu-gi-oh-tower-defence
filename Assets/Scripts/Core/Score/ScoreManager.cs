using UnityEngine;
using System;
using YuGiOhTowerDefense.Core.Enemy;
using YuGiOhTowerDefense.Core.UI;

namespace YuGiOhTowerDefense.Core.Score
{
    /// <summary>
    /// Manages the game's scoring system
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static event Action<int> OnScoreChanged;
        
        [Header("Score Settings")]
        [SerializeField] private int baseEnemyKillScore = 100;
        [SerializeField] private int waveCompletionBonus = 1000;
        [SerializeField] private float comboMultiplier = 1.1f;
        [SerializeField] private float comboDecayTime = 3f;
        
        private GameStateUI gameStateUI;
        private int currentScore;
        private int currentCombo;
        private float lastKillTime;
        private int highScore;
        
        private const string HighScoreKey = "HighScore";
        
        public int CurrentScore => currentScore;
        public int CurrentCombo => currentCombo;
        
        private void Awake()
        {
            currentScore = 0;
            currentCombo = 0;
        }
        
        private void Start()
        {
            LoadHighScore();
        }
        
        public void Initialize(GameStateUI ui)
        {
            gameStateUI = ui;
            
            // Subscribe to events
            EnemyController.OnEnemyDefeated += HandleEnemyDefeated;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EnemyController.OnEnemyDefeated -= HandleEnemyDefeated;
        }
        
        private void Update()
        {
            // Decay combo if no kills for a while
            if (currentCombo > 0 && Time.time - lastKillTime > comboDecayTime)
            {
                currentCombo = 0;
            }
        }
        
        private void HandleEnemyDefeated(EnemyController enemy)
        {
            // Calculate score based on enemy type and combo
            int killScore = CalculateKillScore(enemy);
            AddScore(killScore);
            
            // Update combo
            UpdateCombo();
        }
        
        private int CalculateKillScore(EnemyController enemy)
        {
            // Base score for killing enemy
            int score = baseEnemyKillScore;
            
            // Apply combo multiplier
            if (currentCombo > 0)
            {
                score = Mathf.RoundToInt(score * Mathf.Pow(comboMultiplier, currentCombo));
            }
            
            return score;
        }
        
        private void UpdateCombo()
        {
            currentCombo++;
            lastKillTime = Time.time;
        }
        
        public void AddScore(int points)
        {
            if (points <= 0) return;
            
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
            }
            
            gameStateUI.AddScore(points);
        }
        
        public void AddWaveCompletionBonus(int wave)
        {
            int bonus = waveCompletionBonus * wave;
            AddScore(bonus);
        }
        
        public void ResetScore()
        {
            currentScore = 0;
            currentCombo = 0;
            OnScoreChanged?.Invoke(currentScore);
            gameStateUI.AddScore(0);
        }
        
        public int GetCurrentScore()
        {
            return currentScore;
        }
        
        public int GetHighScore()
        {
            return highScore;
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }
} 