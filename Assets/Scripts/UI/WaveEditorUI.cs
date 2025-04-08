using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class WaveEditorUI : MonoBehaviour
    {
        [Header("Wave Editor Panel")]
        [SerializeField] private GameObject waveEditorPanel;
        [SerializeField] private TMP_InputField waveNameInput;
        [SerializeField] private TMP_InputField spawnDelayInput;
        
        [Header("Enemy List")]
        [SerializeField] private GameObject enemyListPanel;
        [SerializeField] private GameObject enemyItemPrefab;
        [SerializeField] private Transform enemyListContent;
        [SerializeField] private Button addEnemyButton;
        
        [Header("Enemy Selection")]
        [SerializeField] private GameObject enemySelectionPanel;
        [SerializeField] private TMP_Dropdown enemyTypeDropdown;
        [SerializeField] private TMP_InputField enemyCountInput;
        [SerializeField] private TMP_InputField spawnIntervalInput;
        [SerializeField] private Button confirmEnemyButton;
        [SerializeField] private Button cancelEnemyButton;
        
        [Header("Navigation")]
        [SerializeField] private Button saveWaveButton;
        [SerializeField] private Button cancelWaveButton;
        
        private LevelEditor levelEditor;
        private int currentWaveIndex = -1;
        private List<GameObject> enemyItems = new List<GameObject>();
        private WaveManager.Wave currentWave;
        
        private void Start()
        {
            levelEditor = FindObjectOfType<LevelEditor>();
            
            if (levelEditor == null)
            {
                Debug.LogError("LevelEditor not found!");
                return;
            }
            
            // Set up button listeners
            addEnemyButton.onClick.AddListener(OnAddEnemyButtonClicked);
            confirmEnemyButton.onClick.AddListener(OnConfirmEnemyButtonClicked);
            cancelEnemyButton.onClick.AddListener(OnCancelEnemyButtonClicked);
            saveWaveButton.onClick.AddListener(OnSaveWaveButtonClicked);
            cancelWaveButton.onClick.AddListener(OnCancelWaveButtonClicked);
            
            // Set up input field listeners
            waveNameInput.onValueChanged.AddListener(OnWaveNameChanged);
            spawnDelayInput.onValueChanged.AddListener(OnSpawnDelayChanged);
            
            // Initialize enemy type dropdown
            InitializeEnemyTypeDropdown();
            
            // Hide panels initially
            waveEditorPanel.SetActive(false);
            enemySelectionPanel.SetActive(false);
        }
        
        private void InitializeEnemyTypeDropdown()
        {
            enemyTypeDropdown.ClearOptions();
            
            // Add enemy types to dropdown
            List<string> enemyTypes = new List<string>
            {
                "Kuriboh",
                "BlueEyesWhiteDragon",
                "DarkMagician",
                "Exodia",
                "SliferTheSkyDragon",
                "ObeliskTheTormentor",
                "TheWingedDragonOfRa"
            };
            
            enemyTypeDropdown.AddOptions(enemyTypes);
        }
        
        public void ShowWaveEditor(int waveIndex)
        {
            if (levelEditor == null)
            {
                return;
            }
            
            LevelEditor.CustomLevel currentLevel = levelEditor.GetCurrentLevel();
            if (currentLevel == null || waveIndex < 0 || waveIndex >= currentLevel.waves.Count)
            {
                return;
            }
            
            currentWaveIndex = waveIndex;
            currentWave = currentLevel.waves[waveIndex];
            
            // Update UI with wave data
            waveNameInput.text = currentWave.waveName;
            spawnDelayInput.text = currentWave.spawnDelay.ToString();
            
            // Update enemy list
            UpdateEnemyList();
            
            // Show wave editor panel
            waveEditorPanel.SetActive(true);
            enemySelectionPanel.SetActive(false);
        }
        
        private void UpdateEnemyList()
        {
            // Clear existing enemy items
            foreach (var item in enemyItems)
            {
                Destroy(item);
            }
            enemyItems.Clear();
            
            if (currentWave == null)
            {
                return;
            }
            
            // Create new enemy items
            for (int i = 0; i < currentWave.enemies.Count; i++)
            {
                int enemyIndex = i;
                WaveManager.EnemySpawnInfo enemy = currentWave.enemies[i];
                
                GameObject enemyItem = Instantiate(enemyItemPrefab, enemyListContent);
                enemyItems.Add(enemyItem);
                
                // Set up enemy item UI
                TMP_Text enemyInfoText = enemyItem.GetComponentInChildren<TMP_Text>();
                if (enemyInfoText != null)
                {
                    enemyInfoText.text = $"{enemy.enemyId} x{enemy.count} (Every {enemy.spawnInterval}s)";
                }
                
                // Set up edit button
                Button editButton = enemyItem.transform.Find("EditButton")?.GetComponent<Button>();
                if (editButton != null)
                {
                    editButton.onClick.AddListener(() => OnEditEnemyButtonClicked(enemyIndex));
                }
                
                // Set up remove button
                Button removeButton = enemyItem.transform.Find("RemoveButton")?.GetComponent<Button>();
                if (removeButton != null)
                {
                    removeButton.onClick.AddListener(() => OnRemoveEnemyButtonClicked(enemyIndex));
                }
            }
        }
        
        private void OnAddEnemyButtonClicked()
        {
            // Show enemy selection panel
            enemySelectionPanel.SetActive(true);
            
            // Reset enemy selection inputs
            enemyTypeDropdown.value = 0;
            enemyCountInput.text = "1";
            spawnIntervalInput.text = "1.0";
        }
        
        private void OnEditEnemyButtonClicked(int enemyIndex)
        {
            if (currentWave == null || enemyIndex < 0 || enemyIndex >= currentWave.enemies.Count)
            {
                return;
            }
            
            WaveManager.EnemySpawnInfo enemy = currentWave.enemies[enemyIndex];
            
            // Set enemy selection inputs
            enemyTypeDropdown.value = enemyTypeDropdown.options.FindIndex(option => option.text == enemy.enemyId);
            enemyCountInput.text = enemy.count.ToString();
            spawnIntervalInput.text = enemy.spawnInterval.ToString();
            
            // Show enemy selection panel
            enemySelectionPanel.SetActive(true);
        }
        
        private void OnRemoveEnemyButtonClicked(int enemyIndex)
        {
            if (currentWave == null || enemyIndex < 0 || enemyIndex >= currentWave.enemies.Count)
            {
                return;
            }
            
            // Remove enemy from wave
            currentWave.enemies.RemoveAt(enemyIndex);
            
            // Update enemy list
            UpdateEnemyList();
        }
        
        private void OnConfirmEnemyButtonClicked()
        {
            if (currentWave == null)
            {
                return;
            }
            
            // Get enemy selection values
            string enemyId = enemyTypeDropdown.options[enemyTypeDropdown.value].text;
            int count = int.Parse(enemyCountInput.text);
            float spawnInterval = float.Parse(spawnIntervalInput.text);
            
            // Create new enemy spawn info
            WaveManager.EnemySpawnInfo newEnemy = new WaveManager.EnemySpawnInfo
            {
                enemyId = enemyId,
                count = count,
                spawnInterval = spawnInterval
            };
            
            // Add or update enemy in wave
            if (enemySelectionPanel.activeSelf)
            {
                currentWave.enemies.Add(newEnemy);
            }
            
            // Update enemy list
            UpdateEnemyList();
            
            // Hide enemy selection panel
            enemySelectionPanel.SetActive(false);
        }
        
        private void OnCancelEnemyButtonClicked()
        {
            // Hide enemy selection panel
            enemySelectionPanel.SetActive(false);
        }
        
        private void OnSaveWaveButtonClicked()
        {
            if (currentWave == null)
            {
                return;
            }
            
            // Update wave data
            currentWave.waveName = waveNameInput.text;
            currentWave.spawnDelay = float.Parse(spawnDelayInput.text);
            
            // Hide wave editor panel
            waveEditorPanel.SetActive(false);
        }
        
        private void OnCancelWaveButtonClicked()
        {
            // Hide wave editor panel
            waveEditorPanel.SetActive(false);
        }
        
        private void OnWaveNameChanged(string value)
        {
            if (currentWave != null)
            {
                currentWave.waveName = value;
            }
        }
        
        private void OnSpawnDelayChanged(string value)
        {
            if (currentWave != null && float.TryParse(value, out float delay))
            {
                currentWave.spawnDelay = delay;
            }
        }
    }
} 