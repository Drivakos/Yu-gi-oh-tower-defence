using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOh.Gameplay;

namespace YuGiOh.UI
{
    public class WaveUI : MonoBehaviour
    {
        [Header("Wave Info")]
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI enemiesRemainingText;
        [SerializeField] private TextMeshProUGUI nextWaveTimerText;
        
        [Header("Wave Progress")]
        [SerializeField] private Slider waveProgressSlider;
        [SerializeField] private Image waveProgressFill;
        [SerializeField] private Gradient progressGradient;
        
        [Header("Wave Announcement")]
        [SerializeField] private GameObject waveAnnouncementPanel;
        [SerializeField] private TextMeshProUGUI waveAnnouncementText;
        [SerializeField] private float announcementDuration = 3f;
        
        private WaveManager waveManager;
        private float nextWaveTimer;
        
        private void Start()
        {
            waveManager = FindObjectOfType<WaveManager>();
            
            // Subscribe to wave events
            waveManager.OnWaveStarted += HandleWaveStarted;
            waveManager.OnWaveCompleted += HandleWaveCompleted;
            waveManager.OnEnemySpawned += HandleEnemySpawned;
            waveManager.OnEnemyDefeated += HandleEnemyDefeated;
            
            // Initialize UI
            UpdateWaveInfo();
            waveAnnouncementPanel.SetActive(false);
        }
        
        private void Update()
        {
            if (!waveManager.IsWaveInProgress())
            {
                nextWaveTimer -= Time.deltaTime;
                nextWaveTimerText.text = $"Next Wave: {Mathf.CeilToInt(nextWaveTimer)}s";
            }
        }
        
        private void HandleWaveStarted(int waveNumber)
        {
            UpdateWaveInfo();
            ShowWaveAnnouncement(waveNumber);
        }
        
        private void HandleWaveCompleted(int waveNumber)
        {
            nextWaveTimer = waveManager.GetTimeBetweenWaves();
            UpdateWaveInfo();
        }
        
        private void HandleEnemySpawned(int enemiesRemaining)
        {
            UpdateWaveInfo();
        }
        
        private void HandleEnemyDefeated(int enemiesRemaining)
        {
            UpdateWaveInfo();
        }
        
        private void UpdateWaveInfo()
        {
            // Update wave number
            waveNumberText.text = $"Wave {waveManager.GetCurrentWave()}";
            
            // Update enemies remaining
            enemiesRemainingText.text = $"Enemies: {waveManager.GetEnemiesRemaining()}";
            
            // Update progress bar
            float progress = 1f - (float)waveManager.GetEnemiesRemaining() / waveManager.GetTotalEnemiesInWave();
            waveProgressSlider.value = progress;
            waveProgressFill.color = progressGradient.Evaluate(progress);
        }
        
        private void ShowWaveAnnouncement(int waveNumber)
        {
            waveAnnouncementText.text = $"Wave {waveNumber}";
            waveAnnouncementPanel.SetActive(true);
            
            // Hide announcement after duration
            Invoke(nameof(HideWaveAnnouncement), announcementDuration);
        }
        
        private void HideWaveAnnouncement()
        {
            waveAnnouncementPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= HandleWaveStarted;
                waveManager.OnWaveCompleted -= HandleWaveCompleted;
                waveManager.OnEnemySpawned -= HandleEnemySpawned;
                waveManager.OnEnemyDefeated -= HandleEnemyDefeated;
            }
        }
    }
} 