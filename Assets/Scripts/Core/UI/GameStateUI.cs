using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core.Wave;
using YuGiOhTowerDefense.Core.Score;

namespace YuGiOhTowerDefense.Core.UI
{
    /// <summary>
    /// Manages the game's UI elements and state
    /// </summary>
    public class GameStateUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI lifePointsText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        
        [Header("Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject waveStartPanel;
        
        [Header("Wave Start")]
        [SerializeField] private TextMeshProUGUI waveStartText;
        [SerializeField] private float waveStartDisplayTime = 2f;
        
        private WaveManager waveManager;
        private ScoreManager scoreManager;
        private int currentLifePoints;
        
        private void Awake()
        {
            // Subscribe to events
            WaveManager.OnWaveChanged += OnWaveChanged;
            WaveManager.OnWaveComplete += OnWaveComplete;
            WaveManager.OnAllWavesComplete += OnAllWavesComplete;
            ScoreManager.OnScoreChanged += OnScoreChanged;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            WaveManager.OnWaveChanged -= OnWaveChanged;
            WaveManager.OnWaveComplete -= OnWaveComplete;
            WaveManager.OnAllWavesComplete -= OnAllWavesComplete;
            ScoreManager.OnScoreChanged -= OnScoreChanged;
        }
        
        public void Initialize(WaveManager waveManager, ScoreManager scoreManager, int initialLifePoints)
        {
            this.waveManager = waveManager;
            this.scoreManager = scoreManager;
            currentLifePoints = initialLifePoints;
            
            UpdateLifePointsDisplay();
            UpdateWaveDisplay();
            UpdateScoreDisplay();
            UpdateHighScoreDisplay();
            
            // Hide panels
            gameOverPanel.SetActive(false);
            pausePanel.SetActive(false);
            waveStartPanel.SetActive(false);
        }
        
        public void UpdateLifePoints(int newLifePoints)
        {
            currentLifePoints = newLifePoints;
            UpdateLifePointsDisplay();
            
            if (currentLifePoints <= 0)
            {
                ShowGameOver();
            }
        }
        
        private void UpdateLifePointsDisplay()
        {
            if (lifePointsText != null)
            {
                lifePointsText.text = $"LP: {currentLifePoints}";
            }
        }
        
        private void UpdateWaveDisplay()
        {
            if (waveText != null && waveManager != null)
            {
                waveText.text = $"Wave: {waveManager.GetCurrentWave()}/{waveManager.GetTotalWaves()}";
            }
        }
        
        private void UpdateScoreDisplay()
        {
            if (scoreText != null && scoreManager != null)
            {
                scoreText.text = $"Score: {scoreManager.GetCurrentScore()}";
            }
        }
        
        private void UpdateHighScoreDisplay()
        {
            if (highScoreText != null && scoreManager != null)
            {
                highScoreText.text = $"High Score: {scoreManager.GetHighScore()}";
            }
        }
        
        private void OnWaveChanged(int waveNumber)
        {
            UpdateWaveDisplay();
            ShowWaveStart(waveNumber);
        }
        
        private void OnWaveComplete()
        {
            UpdateWaveDisplay();
        }
        
        private void OnAllWavesComplete()
        {
            // Handle game completion
            ShowGameOver();
        }
        
        private void OnScoreChanged(int newScore)
        {
            UpdateScoreDisplay();
            UpdateHighScoreDisplay();
        }
        
        private void ShowWaveStart(int waveNumber)
        {
            if (waveStartPanel != null && waveStartText != null)
            {
                waveStartText.text = $"Wave {waveNumber}";
                waveStartPanel.SetActive(true);
                Invoke(nameof(HideWaveStart), waveStartDisplayTime);
            }
        }
        
        private void HideWaveStart()
        {
            if (waveStartPanel != null)
            {
                waveStartPanel.SetActive(false);
            }
        }
        
        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        
        public void ShowPause()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        
        public void HidePause()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
} 