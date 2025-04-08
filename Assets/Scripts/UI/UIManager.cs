using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Wave Information")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI enemiesRemainingText;
        [SerializeField] private Slider waveProgressBar;
        [SerializeField] private GameObject waveStartButton;
        
        [Header("Player Resources")]
        [SerializeField] private TextMeshProUGUI duelPointsText;
        [SerializeField] private TextMeshProUGUI lifePointsText;
        
        [Header("Monster Selection")]
        [SerializeField] private Transform monsterContainer;
        [SerializeField] private GameObject monsterButtonPrefab;
        [SerializeField] private List<Monster> availableMonsters;
        
        [Header("Card Selection")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private List<Card> availableCards;
        
        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button restartButton;
        
        private GameManager gameManager;
        private WaveManager waveManager;
        private Dictionary<Monster, Button> monsterButtons;
        private Dictionary<Card, Button> cardButtons;
        
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            waveManager = FindObjectOfType<WaveManager>();
            
            monsterButtons = new Dictionary<Monster, Button>();
            cardButtons = new Dictionary<Card, Button>();
            
            InitializeMonsterButtons();
            InitializeCardButtons();
            
            // Subscribe to events
            gameManager.OnDuelPointsChanged += UpdateDuelPoints;
            gameManager.OnLifePointsChanged += UpdateLifePoints;
            gameManager.OnGameOver += ShowGameOver;
            gameManager.OnGameCompleted += ShowGameCompleted;
            
            waveManager.OnWaveStarted += UpdateWaveInfo;
            waveManager.OnEnemyCountChanged += UpdateEnemyCount;
            
            // Initial UI update
            UpdateDuelPoints(gameManager.GetDuelPoints());
            UpdateLifePoints(gameManager.GetLifePoints());
            UpdateWaveInfo(waveManager.GetCurrentWaveIndex());
        }
        
        private void InitializeMonsterButtons()
        {
            foreach (var monster in availableMonsters)
            {
                GameObject buttonObj = Instantiate(monsterButtonPrefab, monsterContainer);
                Button button = buttonObj.GetComponent<Button>();
                Image icon = buttonObj.GetComponentInChildren<Image>();
                TextMeshProUGUI costText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                
                // Set button properties
                icon.sprite = monster.GetIcon();
                costText.text = monster.GetStats().cost.ToString();
                
                // Add click handler
                button.onClick.AddListener(() => OnMonsterSelected(monster));
                
                monsterButtons.Add(monster, button);
            }
        }
        
        private void InitializeCardButtons()
        {
            foreach (var card in availableCards)
            {
                GameObject buttonObj = Instantiate(cardButtonPrefab, cardContainer);
                Button button = buttonObj.GetComponent<Button>();
                Image icon = buttonObj.GetComponentInChildren<Image>();
                TextMeshProUGUI costText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                
                // Set button properties
                icon.sprite = card.GetIcon();
                costText.text = card.GetStats().cost.ToString();
                
                // Add click handler
                button.onClick.AddListener(() => OnCardSelected(card));
                
                cardButtons.Add(card, button);
            }
        }
        
        private void OnMonsterSelected(Monster monster)
        {
            if (gameManager.CanAffordMonster(monster.GetStats().cost))
            {
                gameManager.StartMonsterPlacement(monster);
            }
        }
        
        private void OnCardSelected(Card card)
        {
            if (gameManager.CanAffordCard(card.GetStats().cost))
            {
                gameManager.SelectCard(card);
            }
        }
        
        private void UpdateDuelPoints(int points)
        {
            duelPointsText.text = $"DP: {points}";
            
            // Update button interactability
            foreach (var kvp in monsterButtons)
            {
                kvp.Value.interactable = points >= kvp.Key.GetStats().cost;
            }
            
            foreach (var kvp in cardButtons)
            {
                kvp.Value.interactable = points >= kvp.Key.GetStats().cost;
            }
        }
        
        private void UpdateLifePoints(int points)
        {
            lifePointsText.text = $"LP: {points}";
        }
        
        private void UpdateWaveInfo(int waveIndex)
        {
            waveText.text = $"Wave {waveIndex + 1}";
            waveStartButton.SetActive(!waveManager.IsWaveActive());
        }
        
        private void UpdateEnemyCount(int remaining, int total)
        {
            enemiesRemainingText.text = $"Enemies: {remaining}/{total}";
            waveProgressBar.value = 1f - ((float)remaining / total);
        }
        
        private void ShowGameOver()
        {
            gameOverPanel.SetActive(true);
            gameOverText.text = "Game Over";
            restartButton.onClick.AddListener(() => gameManager.RestartGame());
        }
        
        private void ShowGameCompleted()
        {
            gameOverPanel.SetActive(true);
            gameOverText.text = "Victory!";
            restartButton.onClick.AddListener(() => gameManager.RestartGame());
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnDuelPointsChanged -= UpdateDuelPoints;
                gameManager.OnLifePointsChanged -= UpdateLifePoints;
                gameManager.OnGameOver -= ShowGameOver;
                gameManager.OnGameCompleted -= ShowGameCompleted;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= UpdateWaveInfo;
                waveManager.OnEnemyCountChanged -= UpdateEnemyCount;
            }
        }
    }
} 