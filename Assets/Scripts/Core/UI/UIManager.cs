using UnityEngine;
using TMPro;
using UnityEngine.UI;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI goldText;
        
        [Header("Health Bar")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalWaveText;
        [SerializeField] private Button restartButton;
        
        private GameManager gameManager;
        private Player player;
        
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            player = FindObjectOfType<Player>();
            
            if (gameManager == null || player == null)
            {
                Debug.LogError("GameManager or Player not found!");
                return;
            }
            
            // Subscribe to events
            gameManager.OnLivesChanged += UpdateLives;
            gameManager.OnGoldChanged += UpdateGold;
            gameManager.OnGameOver += ShowGameOver;
            player.OnHealthChanged += UpdateHealth;
            
            // Initialize UI
            UpdateLives(gameManager.CurrentLives);
            UpdateGold(gameManager.CurrentGold);
            UpdateHealth(player.GetHealthPercentage() * 100);
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            // Set up restart button
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }
        
        private void Update()
        {
            // Update wave text
            if (waveText != null)
            {
                waveText.text = $"Wave: {gameManager.CurrentWave}";
            }
        }
        
        private void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }
        
        private void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }
        
        private void UpdateHealth(int health)
        {
            if (healthBar != null)
            {
                healthBar.value = (float)health / 100f;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{health}%";
            }
        }
        
        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                if (finalWaveText != null)
                {
                    finalWaveText.text = $"Final Wave: {gameManager.CurrentWave}";
                }
            }
        }
        
        private void RestartGame()
        {
            // Reset time scale
            Time.timeScale = 1f;
            
            // Reload the current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnLivesChanged -= UpdateLives;
                gameManager.OnGoldChanged -= UpdateGold;
                gameManager.OnGameOver -= ShowGameOver;
            }
            
            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealth;
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }
        }
    }
} 