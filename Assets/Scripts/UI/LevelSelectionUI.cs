using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class LevelSelectionUI : MonoBehaviour
    {
        [Header("Level Selection")]
        [SerializeField] private GameObject levelSelectionPanel;
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Button backButton;
        
        [Header("Level Details")]
        [SerializeField] private GameObject levelDetailsPanel;
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI levelDescriptionText;
        [SerializeField] private Image levelImage;
        [SerializeField] private TextMeshProUGUI startingDuelPointsText;
        [SerializeField] private TextMeshProUGUI startingLifePointsText;
        [SerializeField] private Button startLevelButton;
        [SerializeField] private Button closeDetailsButton;
        
        private LevelManager levelManager;
        private GameManager gameManager;
        private List<GameObject> levelButtons = new List<GameObject>();
        private int selectedLevelIndex = -1;
        
        private void Start()
        {
            levelManager = FindObjectOfType<LevelManager>();
            gameManager = FindObjectOfType<GameManager>();
            
            // Set up button listeners
            backButton.onClick.AddListener(OnBackButtonClicked);
            startLevelButton.onClick.AddListener(OnStartLevelButtonClicked);
            closeDetailsButton.onClick.AddListener(OnCloseDetailsButtonClicked);
            
            // Hide panels initially
            levelSelectionPanel.SetActive(false);
            levelDetailsPanel.SetActive(false);
        }
        
        public void ShowLevelSelection()
        {
            levelSelectionPanel.SetActive(true);
            levelDetailsPanel.SetActive(false);
            PopulateLevelButtons();
        }
        
        private void PopulateLevelButtons()
        {
            // Clear existing buttons
            foreach (var button in levelButtons)
            {
                Destroy(button);
            }
            levelButtons.Clear();
            
            // Create buttons for each level
            List<LevelManager.LevelData> levels = levelManager.GetAllLevels();
            for (int i = 0; i < levels.Count; i++)
            {
                int levelIndex = i; // Capture for lambda
                LevelManager.LevelData levelData = levels[i];
                
                GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Image buttonImage = buttonObj.GetComponent<Image>();
                
                // Set button properties
                buttonText.text = levelData.levelName;
                button.interactable = levelData.isUnlocked;
                
                // Add click handler
                button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
                
                levelButtons.Add(buttonObj);
            }
        }
        
        private void OnLevelButtonClicked(int levelIndex)
        {
            selectedLevelIndex = levelIndex;
            ShowLevelDetails(levelIndex);
        }
        
        private void ShowLevelDetails(int levelIndex)
        {
            LevelManager.LevelData levelData = levelManager.GetAllLevels()[levelIndex];
            
            levelNameText.text = levelData.levelName;
            levelDescriptionText.text = levelData.description;
            levelImage.sprite = levelData.levelImage;
            startingDuelPointsText.text = $"Starting DP: {levelData.startingDuelPoints}";
            startingLifePointsText.text = $"Starting LP: {levelData.startingLifePoints}";
            
            startLevelButton.interactable = levelData.isUnlocked;
            
            levelDetailsPanel.SetActive(true);
        }
        
        private void OnStartLevelButtonClicked()
        {
            if (selectedLevelIndex >= 0)
            {
                levelManager.LoadLevel(selectedLevelIndex);
                gameManager.StartGame();
                HideAllPanels();
            }
        }
        
        private void OnBackButtonClicked()
        {
            HideAllPanels();
            // Return to main menu or previous screen
        }
        
        private void OnCloseDetailsButtonClicked()
        {
            levelDetailsPanel.SetActive(false);
        }
        
        private void HideAllPanels()
        {
            levelSelectionPanel.SetActive(false);
            levelDetailsPanel.SetActive(false);
        }
    }
} 