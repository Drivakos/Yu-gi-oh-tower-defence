using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class LevelEditorUI : MonoBehaviour
    {
        [Header("Editor UI")]
        [SerializeField] private GameObject editorPanel;
        [SerializeField] private GameObject playtestPanel;
        [SerializeField] private GameObject saveLoadPanel;
        
        [Header("Level Info")]
        [SerializeField] private TMP_InputField levelNameInput;
        [SerializeField] private TMP_InputField levelDescriptionInput;
        [SerializeField] private Slider difficultySlider;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_InputField startingDuelPointsInput;
        [SerializeField] private TMP_InputField startingLifePointsInput;
        
        [Header("Object Selection")]
        [SerializeField] private GameObject enemySelectionPanel;
        [SerializeField] private GameObject monsterSelectionPanel;
        [SerializeField] private GameObject cardSelectionPanel;
        [SerializeField] private Button[] enemyButtons;
        [SerializeField] private Button[] monsterButtons;
        [SerializeField] private Button[] cardButtons;
        
        [Header("Wave Management")]
        [SerializeField] private GameObject waveListPanel;
        [SerializeField] private GameObject waveItemPrefab;
        [SerializeField] private Transform waveListContent;
        [SerializeField] private Button addWaveButton;
        
        [Header("Playtest UI")]
        [SerializeField] private Button startPlaytestButton;
        [SerializeField] private Button stopPlaytestButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button publishButton;
        
        [Header("Save/Load UI")]
        [SerializeField] private GameObject customLevelListPanel;
        [SerializeField] private GameObject customLevelItemPrefab;
        [SerializeField] private Transform customLevelListContent;
        [SerializeField] private Button closeSaveLoadButton;
        
        private LevelEditor levelEditor;
        private List<GameObject> waveItems = new List<GameObject>();
        private List<GameObject> customLevelItems = new List<GameObject>();
        
        private void Start()
        {
            levelEditor = FindObjectOfType<LevelEditor>();
            
            if (levelEditor == null)
            {
                Debug.LogError("LevelEditor not found!");
                return;
            }
            
            // Set up button listeners
            addWaveButton.onClick.AddListener(OnAddWaveButtonClicked);
            startPlaytestButton.onClick.AddListener(OnStartPlaytestButtonClicked);
            stopPlaytestButton.onClick.AddListener(OnStopPlaytestButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            publishButton.onClick.AddListener(OnPublishButtonClicked);
            closeSaveLoadButton.onClick.AddListener(OnCloseSaveLoadButtonClicked);
            
            // Set up input field listeners
            levelNameInput.onValueChanged.AddListener(OnLevelNameChanged);
            levelDescriptionInput.onValueChanged.AddListener(OnLevelDescriptionChanged);
            difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
            startingDuelPointsInput.onValueChanged.AddListener(OnStartingDuelPointsChanged);
            startingLifePointsInput.onValueChanged.AddListener(OnStartingLifePointsChanged);
            
            // Set up enemy/monster/card button listeners
            for (int i = 0; i < enemyButtons.Length; i++)
            {
                int index = i;
                enemyButtons[i].onClick.AddListener(() => OnEnemyButtonClicked(index));
            }
            
            for (int i = 0; i < monsterButtons.Length; i++)
            {
                int index = i;
                monsterButtons[i].onClick.AddListener(() => OnMonsterButtonClicked(index));
            }
            
            for (int i = 0; i < cardButtons.Length; i++)
            {
                int index = i;
                cardButtons[i].onClick.AddListener(() => OnCardButtonClicked(index));
            }
            
            // Initialize UI state
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            if (levelEditor == null)
            {
                return;
            }
            
            LevelEditor.CustomLevel currentLevel = levelEditor.GetCurrentLevel();
            if (currentLevel == null)
            {
                return;
            }
            
            // Update level info
            levelNameInput.text = currentLevel.levelName;
            levelDescriptionInput.text = currentLevel.description;
            difficultySlider.value = currentLevel.difficulty;
            difficultyText.text = "Difficulty: " + currentLevel.difficulty;
            startingDuelPointsInput.text = currentLevel.startingDuelPoints.ToString();
            startingLifePointsInput.text = currentLevel.startingLifePoints.ToString();
            
            // Update wave list
            UpdateWaveList();
            
            // Update UI panels based on editor state
            editorPanel.SetActive(levelEditor.IsEditing());
            playtestPanel.SetActive(levelEditor.IsPlaytesting());
            saveLoadPanel.SetActive(false);
            
            // Update button states
            startPlaytestButton.gameObject.SetActive(levelEditor.IsEditing());
            stopPlaytestButton.gameObject.SetActive(levelEditor.IsPlaytesting());
        }
        
        private void UpdateWaveList()
        {
            // Clear existing wave items
            foreach (var item in waveItems)
            {
                Destroy(item);
            }
            waveItems.Clear();
            
            // Create new wave items
            LevelEditor.CustomLevel currentLevel = levelEditor.GetCurrentLevel();
            if (currentLevel == null)
            {
                return;
            }
            
            for (int i = 0; i < currentLevel.waves.Count; i++)
            {
                int waveIndex = i;
                WaveManager.Wave wave = currentLevel.waves[i];
                
                GameObject waveItem = Instantiate(waveItemPrefab, waveListContent);
                waveItems.Add(waveItem);
                
                // Set up wave item UI
                TMP_Text waveNameText = waveItem.GetComponentInChildren<TMP_Text>();
                if (waveNameText != null)
                {
                    waveNameText.text = wave.waveName;
                }
                
                Button editButton = waveItem.GetComponentInChildren<Button>();
                if (editButton != null)
                {
                    editButton.onClick.AddListener(() => OnEditWaveButtonClicked(waveIndex));
                }
            }
        }
        
        private void UpdateCustomLevelList()
        {
            // Clear existing items
            foreach (var item in customLevelItems)
            {
                Destroy(item);
            }
            customLevelItems.Clear();
            
            // Load custom levels from disk
            string directory = System.IO.Path.Combine(Application.persistentDataPath, "CustomLevels");
            if (!System.IO.Directory.Exists(directory))
            {
                return;
            }
            
            string[] files = System.IO.Directory.GetFiles(directory, "*.json");
            foreach (string file in files)
            {
                string json = System.IO.File.ReadAllText(file);
                LevelEditor.CustomLevel level = JsonUtility.FromJson<LevelEditor.CustomLevel>(json);
                
                GameObject levelItem = Instantiate(customLevelItemPrefab, customLevelListContent);
                customLevelItems.Add(levelItem);
                
                // Set up level item UI
                TMP_Text levelNameText = levelItem.GetComponentInChildren<TMP_Text>();
                if (levelNameText != null)
                {
                    levelNameText.text = level.levelName;
                }
                
                Button loadButton = levelItem.GetComponentInChildren<Button>();
                if (loadButton != null)
                {
                    loadButton.onClick.AddListener(() => OnLoadCustomLevelButtonClicked(level.levelId));
                }
            }
        }
        
        private void OnAddWaveButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.AddNewWave();
                UpdateWaveList();
            }
        }
        
        private void OnEditWaveButtonClicked(int waveIndex)
        {
            // TODO: Implement wave editing UI
            Debug.Log("Edit wave " + waveIndex);
        }
        
        private void OnStartPlaytestButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.StartPlaytesting();
                UpdateUI();
            }
        }
        
        private void OnStopPlaytestButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.StopPlaytesting();
                UpdateUI();
            }
        }
        
        private void OnSaveButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.SaveCustomLevel();
            }
        }
        
        private void OnLoadButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.ShowSaveLoadUI();
                UpdateCustomLevelList();
            }
        }
        
        private void OnPublishButtonClicked()
        {
            // TODO: Implement level publishing to server
            Debug.Log("Publish level");
        }
        
        private void OnCloseSaveLoadButtonClicked()
        {
            if (levelEditor != null)
            {
                levelEditor.HideSaveLoadUI();
            }
        }
        
        private void OnLoadCustomLevelButtonClicked(string levelId)
        {
            if (levelEditor != null)
            {
                levelEditor.LoadCustomLevel(levelId);
                levelEditor.HideSaveLoadUI();
                UpdateUI();
            }
        }
        
        private void OnLevelNameChanged(string value)
        {
            if (levelEditor != null)
            {
                levelEditor.UpdateLevelName(value);
            }
        }
        
        private void OnLevelDescriptionChanged(string value)
        {
            if (levelEditor != null)
            {
                levelEditor.UpdateLevelDescription(value);
            }
        }
        
        private void OnDifficultyChanged(float value)
        {
            if (levelEditor != null)
            {
                levelEditor.UpdateLevelDifficulty(Mathf.RoundToInt(value));
                difficultyText.text = "Difficulty: " + value;
            }
        }
        
        private void OnStartingDuelPointsChanged(string value)
        {
            if (levelEditor != null && int.TryParse(value, out int points))
            {
                levelEditor.UpdateStartingDuelPoints(points);
            }
        }
        
        private void OnStartingLifePointsChanged(string value)
        {
            if (levelEditor != null && int.TryParse(value, out int points))
            {
                levelEditor.UpdateStartingLifePoints(points);
            }
        }
        
        private void OnEnemyButtonClicked(int index)
        {
            if (levelEditor != null)
            {
                levelEditor.SelectEnemy(index);
            }
        }
        
        private void OnMonsterButtonClicked(int index)
        {
            if (levelEditor != null)
            {
                levelEditor.SelectMonster(index);
            }
        }
        
        private void OnCardButtonClicked(int index)
        {
            if (levelEditor != null)
            {
                levelEditor.SelectCard(index);
            }
        }
    }
} 