using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [Header("Wave UI")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI waveTimerText;
        [SerializeField] private GameObject waveStartPanel;
        [SerializeField] private GameObject waveEndPanel;
        [SerializeField] private float waveStartDuration = 3f;
        [SerializeField] private float waveEndDuration = 2f;
        
        [Header("Resource UI")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private Image healthIcon;
        [SerializeField] private float resourceUpdateDuration = 0.3f;
        
        [Header("Card UI")]
        [SerializeField] private MobileHandUI handUI;
        [SerializeField] private float cardDrawDuration = 0.5f;
        [SerializeField] private AnimationCurve cardDrawCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private float gameOverDelay = 2f;
        [SerializeField] private float gameOverFadeDuration = 1f;
        
        private GameplayManager gameplayManager;
        private CanvasGroup gameOverCanvasGroup;
        private bool isUpdatingResources;
        
        private void Awake()
        {
            gameplayManager = GetComponent<GameplayManager>();
            if (gameplayManager == null)
            {
                gameplayManager = FindObjectOfType<GameplayManager>();
                if (gameplayManager == null)
                {
                    Debug.LogError("GameplayManager not found!");
                }
            }
            
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
            
            // Hide panels initially
            if (waveStartPanel != null)
            {
                waveStartPanel.SetActive(false);
            }
            
            if (waveEndPanel != null)
            {
                waveEndPanel.SetActive(false);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            // Set up buttons
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }
        
        private void Start()
        {
            // Start resource update routine
            StartCoroutine(UpdateResourcesRoutine());
        }
        
        public void ShowWaveStart(int waveNumber)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave {waveNumber}";
            }
            
            if (waveStartPanel != null)
            {
                waveStartPanel.SetActive(true);
            }
            
            StartCoroutine(HideWaveStartPanel());
        }
        
        private IEnumerator HideWaveStartPanel()
        {
            yield return new WaitForSeconds(waveStartDuration);
            
            if (waveStartPanel != null)
            {
                waveStartPanel.SetActive(false);
            }
        }
        
        public void ShowWaveEnd(int waveNumber, int reward)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave {waveNumber} Complete!";
            }
            
            if (waveTimerText != null)
            {
                waveTimerText.text = $"+{reward} Currency";
            }
            
            if (waveEndPanel != null)
            {
                waveEndPanel.SetActive(true);
            }
            
            StartCoroutine(HideWaveEndPanel());
        }
        
        private IEnumerator HideWaveEndPanel()
        {
            yield return new WaitForSeconds(waveEndDuration);
            
            if (waveEndPanel != null)
            {
                waveEndPanel.SetActive(false);
            }
        }
        
        private IEnumerator UpdateResourcesRoutine()
        {
            while (true)
            {
                if (gameplayManager != null)
                {
                    UpdateCurrencyText(gameplayManager.GetCurrentCurrency());
                    // TODO: Update health text
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void UpdateCurrencyText(int amount)
        {
            if (currencyText != null)
            {
                currencyText.text = amount.ToString();
            }
            
            // Animate currency icon
            if (currencyIcon != null && !isUpdatingResources)
            {
                StartCoroutine(AnimateResourceIcon(currencyIcon));
            }
        }
        
        private IEnumerator AnimateResourceIcon(Image icon)
        {
            isUpdatingResources = true;
            
            float elapsedTime = 0f;
            Vector3 startScale = icon.transform.localScale;
            Vector3 targetScale = startScale * 1.2f;
            
            while (elapsedTime < resourceUpdateDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / resourceUpdateDuration;
                
                icon.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            elapsedTime = 0f;
            
            while (elapsedTime < resourceUpdateDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / resourceUpdateDuration;
                
                icon.transform.localScale = Vector3.Lerp(targetScale, startScale, t);
                
                yield return null;
            }
            
            icon.transform.localScale = startScale;
            isUpdatingResources = false;
        }
        
        public void ShowGameOver(bool victory)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            if (gameOverText != null)
            {
                gameOverText.text = victory ? "Victory!" : "Game Over";
            }
            
            StartCoroutine(ShowGameOverSequence());
        }
        
        private IEnumerator ShowGameOverSequence()
        {
            // Wait for delay
            yield return new WaitForSeconds(gameOverDelay);
            
            // Fade in
            float elapsedTime = 0f;
            
            while (elapsedTime < gameOverFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / gameOverFadeDuration;
                
                gameOverCanvasGroup.alpha = t;
                
                yield return null;
            }
            
            gameOverCanvasGroup.alpha = 1f;
        }
        
        private void OnRestartClicked()
        {
            // TODO: Restart game
        }
        
        private void OnMainMenuClicked()
        {
            // TODO: Return to main menu
        }
    }
} 