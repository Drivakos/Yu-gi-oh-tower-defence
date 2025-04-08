using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using YuGiOhTowerDefense.Enemies;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.Core
{
    public class LevelEditor : MonoBehaviour
    {
        [System.Serializable]
        public class CustomLevel
        {
            public string levelId;
            public string levelName;
            public string description;
            public string author;
            public DateTime creationDate;
            public int difficulty;
            public int startingDuelPoints;
            public int startingLifePoints;
            public List<string> availableMonsterIds;
            public List<string> availableCardIds;
            public List<WaveManager.Wave> waves;
            public string thumbnailPath;
            public int downloads;
            public float averageRating;
            public int ratingCount;
        }
        
        [Header("Editor Settings")]
        [SerializeField] private GameObject editorUI;
        [SerializeField] private GameObject playtestUI;
        [SerializeField] private GameObject saveLoadUI;
        [SerializeField] private float gridSnapSize = 1f;
        [SerializeField] private LayerMask tileLayer;
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private TileManager tileManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private LevelValidator levelValidator;
        [SerializeField] private LevelValidationUI validationUI;
        [SerializeField] private WaveEditorUI waveEditorUI;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject enemyPreviewPrefab;
        [SerializeField] private GameObject monsterPreviewPrefab;
        [SerializeField] private GameObject cardPreviewPrefab;
        
        private CustomLevel currentLevel;
        private bool isEditing = false;
        private bool isPlaytesting = false;
        private GameObject currentPreview;
        private Vector3 lastValidPosition;
        private List<GameObject> placedObjects = new List<GameObject>();
        private int selectedEnemyIndex = 0;
        private int selectedMonsterIndex = 0;
        private int selectedCardIndex = 0;
        private Vector2Int gridDimensions;
        
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
            
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }
            
            if (levelValidator == null)
            {
                levelValidator = FindObjectOfType<LevelValidator>();
                if (levelValidator == null)
                {
                    Debug.LogError("LevelEditor: LevelValidator not found!");
                }
            }
            
            if (validationUI == null)
            {
                validationUI = FindObjectOfType<LevelValidationUI>();
                if (validationUI == null)
                {
                    Debug.LogError("LevelEditor: LevelValidationUI not found!");
                }
            }
            
            if (waveEditorUI == null)
            {
                waveEditorUI = FindObjectOfType<WaveEditorUI>();
                if (waveEditorUI == null)
                {
                    Debug.LogError("LevelEditor: WaveEditorUI not found!");
                }
            }
            
            // Hide editor UI initially
            if (editorUI != null)
            {
                editorUI.SetActive(false);
            }
            
            if (playtestUI != null)
            {
                playtestUI.SetActive(false);
            }
            
            if (saveLoadUI != null)
            {
                saveLoadUI.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Initialize with default values
            gridDimensions = new Vector2Int(5, 5);
            currentLevel = new CustomLevel
            {
                levelId = Guid.NewGuid().ToString(),
                levelName = "New Level",
                description = "A custom level",
                author = "Player",
                creationDate = DateTime.Now,
                difficulty = 1,
                startingDuelPoints = 1000,
                startingLifePoints = 8000,
                availableMonsterIds = new List<string>(),
                availableCardIds = new List<string>(),
                waves = new List<WaveManager.Wave>(),
                downloads = 0,
                averageRating = 0,
                ratingCount = 0
            };
            
            // Add default monsters and cards
            currentLevel.availableMonsterIds.Add("BlueEyesWhiteDragon");
            currentLevel.availableMonsterIds.Add("DarkMagician");
            currentLevel.availableMonsterIds.Add("Kuriboh");
            
            currentLevel.availableCardIds.Add("MirrorForce");
            currentLevel.availableCardIds.Add("Polymerization");
            
            // Add a default wave
            WaveManager.Wave defaultWave = new WaveManager.Wave
            {
                waveName = "Wave 1",
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo { enemyId = "Kuriboh", count = 5, spawnInterval = 2f }
                },
                spawnDelay = 5f
            };
            
            currentLevel.waves.Add(defaultWave);
            
            // Start editing
            StartEditing();
        }
        
        public void StartNewLevel()
        {
            currentLevel = new CustomLevel
            {
                levelId = Guid.NewGuid().ToString(),
                levelName = "New Level",
                description = "A custom level",
                author = "Player",
                creationDate = DateTime.Now,
                difficulty = 1,
                startingDuelPoints = 1000,
                startingLifePoints = 8000,
                availableMonsterIds = new List<string>(),
                availableCardIds = new List<string>(),
                waves = new List<WaveManager.Wave>(),
                downloads = 0,
                averageRating = 0,
                ratingCount = 0
            };
            
            // Add default monsters and cards
            currentLevel.availableMonsterIds.Add("BlueEyesWhiteDragon");
            currentLevel.availableMonsterIds.Add("DarkMagician");
            currentLevel.availableMonsterIds.Add("Kuriboh");
            
            currentLevel.availableCardIds.Add("MirrorForce");
            currentLevel.availableCardIds.Add("Polymerization");
            
            // Add a default wave
            WaveManager.Wave defaultWave = new WaveManager.Wave
            {
                waveName = "Wave 1",
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo { enemyId = "Kuriboh", count = 5, spawnInterval = 2f }
                },
                spawnDelay = 5f
            };
            
            currentLevel.waves.Add(defaultWave);
            
            // Start editing
            StartEditing();
        }
        
        public void LoadCustomLevel(string levelId)
        {
            string path = Path.Combine(Application.persistentDataPath, "CustomLevels", levelId + ".json");
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                currentLevel = JsonUtility.FromJson<CustomLevel>(json);
                
                // Update downloads count
                currentLevel.downloads++;
                SaveCustomLevel();
                
                // Start editing
                StartEditing();
            }
            else
            {
                Debug.LogError("Custom level not found: " + levelId);
            }
        }
        
        public void SaveCustomLevel()
        {
            if (currentLevel == null)
            {
                return;
            }
            
            // Create directory if it doesn't exist
            string directory = Path.Combine(Application.persistentDataPath, "CustomLevels");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Save level data
            string path = Path.Combine(directory, currentLevel.levelId + ".json");
            string json = JsonUtility.ToJson(currentLevel, true);
            File.WriteAllText(path, json);
            
            Debug.Log("Custom level saved: " + path);
        }
        
        public void StartEditing()
        {
            isEditing = true;
            isPlaytesting = false;
            
            // Show editor UI
            if (editorUI != null)
            {
                editorUI.SetActive(true);
            }
            
            if (playtestUI != null)
            {
                playtestUI.SetActive(false);
            }
            
            if (saveLoadUI != null)
            {
                saveLoadUI.SetActive(false);
            }
            
            // Reset game state
            if (gameManager != null)
            {
                gameManager.ResetGame();
            }
            
            // Clear placed objects
            foreach (var obj in placedObjects)
            {
                Destroy(obj);
            }
            placedObjects.Clear();
            
            // Reset tile manager
            if (tileManager != null)
            {
                tileManager.ResetTiles();
            }
        }
        
        public void StartPlaytesting()
        {
            if (currentLevel == null)
            {
                return;
            }
            
            isEditing = false;
            isPlaytesting = true;
            
            // Hide editor UI, show playtest UI
            if (editorUI != null)
            {
                editorUI.SetActive(false);
            }
            
            if (playtestUI != null)
            {
                playtestUI.SetActive(true);
            }
            
            // Configure game for playtesting
            if (gameManager != null)
            {
                gameManager.SetDuelPoints(currentLevel.startingDuelPoints);
                gameManager.SetLifePoints(currentLevel.startingLifePoints);
                
                // Set available monsters and cards
                gameManager.ClearAvailableMonsters();
                gameManager.ClearAvailableCards();
                
                foreach (string monsterId in currentLevel.availableMonsterIds)
                {
                    gameManager.UnlockMonster(monsterId);
                }
                
                foreach (string cardId in currentLevel.availableCardIds)
                {
                    gameManager.UnlockCard(cardId);
                }
            }
            
            // Configure waves
            if (waveManager != null)
            {
                waveManager.SetWaves(currentLevel.waves);
            }
            
            // Start the game
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
        
        public void StopPlaytesting()
        {
            isPlaytesting = false;
            
            // Stop the game
            if (gameManager != null)
            {
                gameManager.StopGame();
            }
            
            // Return to editing
            StartEditing();
        }
        
        public void ShowSaveLoadUI()
        {
            if (saveLoadUI != null)
            {
                saveLoadUI.SetActive(true);
            }
        }
        
        public void HideSaveLoadUI()
        {
            if (saveLoadUI != null)
            {
                saveLoadUI.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isEditing)
            {
                return;
            }
            
            // Handle mouse input for placing objects
            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                RemoveObject();
            }
            
            // Update preview position
            UpdatePreviewPosition();
        }
        
        private void UpdatePreviewPosition()
        {
            if (currentPreview == null)
            {
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f, tileLayer))
            {
                // Snap to grid
                Vector3 position = hit.point;
                position.x = Mathf.Round(position.x / gridSnapSize) * gridSnapSize;
                position.z = Mathf.Round(position.z / gridSnapSize) * gridSnapSize;
                
                currentPreview.transform.position = position;
                lastValidPosition = position;
            }
        }
        
        private void PlaceObject()
        {
            if (currentPreview == null)
            {
                return;
            }
            
            // Instantiate the actual object
            GameObject newObject = Instantiate(currentPreview, lastValidPosition, Quaternion.identity);
            placedObjects.Add(newObject);
            
            // Add to level data based on object type
            if (newObject.CompareTag("Enemy"))
            {
                // Add enemy to current wave
                if (currentLevel.waves.Count > 0)
                {
                    WaveManager.Wave currentWave = currentLevel.waves[currentLevel.waves.Count - 1];
                    currentWave.enemies.Add(new WaveManager.EnemySpawnInfo
                    {
                        enemyId = GetSelectedEnemyId(),
                        count = 1,
                        spawnInterval = 1f
                    });
                }
            }
            else if (newObject.CompareTag("Monster"))
            {
                // Add monster to available monsters
                string monsterId = GetSelectedMonsterId();
                if (!currentLevel.availableMonsterIds.Contains(monsterId))
                {
                    currentLevel.availableMonsterIds.Add(monsterId);
                }
            }
            else if (newObject.CompareTag("Card"))
            {
                // Add card to available cards
                string cardId = GetSelectedCardId();
                if (!currentLevel.availableCardIds.Contains(cardId))
                {
                    currentLevel.availableCardIds.Add(cardId);
                }
            }
        }
        
        private void RemoveObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
            {
                GameObject hitObject = hit.collider.gameObject;
                
                if (placedObjects.Contains(hitObject))
                {
                    // Remove from level data based on object type
                    if (hitObject.CompareTag("Enemy"))
                    {
                        // Remove enemy from current wave
                        if (currentLevel.waves.Count > 0)
                        {
                            WaveManager.Wave currentWave = currentLevel.waves[currentLevel.waves.Count - 1];
                            currentWave.enemies.RemoveAt(currentWave.enemies.Count - 1);
                        }
                    }
                    else if (hitObject.CompareTag("Monster"))
                    {
                        // Remove monster from available monsters
                        string monsterId = GetSelectedMonsterId();
                        currentLevel.availableMonsterIds.Remove(monsterId);
                    }
                    else if (hitObject.CompareTag("Card"))
                    {
                        // Remove card from available cards
                        string cardId = GetSelectedCardId();
                        currentLevel.availableCardIds.Remove(cardId);
                    }
                    
                    // Remove from scene
                    placedObjects.Remove(hitObject);
                    Destroy(hitObject);
                }
            }
        }
        
        public void SelectEnemy(int index)
        {
            selectedEnemyIndex = index;
            
            // Update preview
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            
            currentPreview = Instantiate(enemyPreviewPrefab);
            currentPreview.tag = "Enemy";
        }
        
        public void SelectMonster(int index)
        {
            selectedMonsterIndex = index;
            
            // Update preview
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            
            currentPreview = Instantiate(monsterPreviewPrefab);
            currentPreview.tag = "Monster";
        }
        
        public void SelectCard(int index)
        {
            selectedCardIndex = index;
            
            // Update preview
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            
            currentPreview = Instantiate(cardPreviewPrefab);
            currentPreview.tag = "Card";
        }
        
        public void AddNewWave()
        {
            WaveManager.Wave newWave = new WaveManager.Wave
            {
                waveName = "Wave " + (currentLevel.waves.Count + 1),
                enemies = new List<WaveManager.EnemySpawnInfo>(),
                spawnDelay = 5f
            };
            
            currentLevel.waves.Add(newWave);
        }
        
        public void UpdateLevelName(string name)
        {
            if (currentLevel != null)
            {
                currentLevel.levelName = name;
            }
        }
        
        public void UpdateLevelDescription(string description)
        {
            if (currentLevel != null)
            {
                currentLevel.description = description;
            }
        }
        
        public void UpdateLevelDifficulty(int difficulty)
        {
            if (currentLevel != null)
            {
                currentLevel.difficulty = difficulty;
            }
        }
        
        public void UpdateStartingDuelPoints(int points)
        {
            if (currentLevel != null)
            {
                currentLevel.startingDuelPoints = points;
            }
        }
        
        public void UpdateStartingLifePoints(int points)
        {
            if (currentLevel != null)
            {
                currentLevel.startingLifePoints = points;
            }
        }
        
        private string GetSelectedEnemyId()
        {
            // This would be replaced with actual enemy IDs from your game
            string[] enemyIds = { "Kuriboh", "BlueEyesWhiteDragon", "DarkMagician" };
            return enemyIds[selectedEnemyIndex % enemyIds.Length];
        }
        
        private string GetSelectedMonsterId()
        {
            // This would be replaced with actual monster IDs from your game
            string[] monsterIds = { "BlueEyesWhiteDragon", "DarkMagician", "Kuriboh" };
            return monsterIds[selectedMonsterIndex % monsterIds.Length];
        }
        
        private string GetSelectedCardId()
        {
            // This would be replaced with actual card IDs from your game
            string[] cardIds = { "MirrorForce", "Polymerization", "PotOfGreed" };
            return cardIds[selectedCardIndex % cardIds.Length];
        }
        
        public CustomLevel GetCurrentLevel()
        {
            return currentLevel;
        }
        
        public bool IsEditing()
        {
            return isEditing;
        }
        
        public bool IsPlaytesting()
        {
            return isPlaytesting;
        }
        
        public void SetGridDimensions(int width, int height)
        {
            width = Mathf.Clamp(width, 5, 20);
            height = Mathf.Clamp(height, 5, 20);
            
            gridDimensions = new Vector2Int(width, height);
            currentLevel.gridWidth = width;
            currentLevel.gridHeight = height;
            
            // TODO: Update grid visualization
        }
        
        public void AddWave(WaveManager.Wave wave)
        {
            if (currentLevel.waves.Count >= 10)
            {
                Debug.LogWarning("LevelEditor: Maximum number of waves reached!");
                return;
            }
            
            currentLevel.waves.Add(wave);
            
            if (waveEditorUI != null)
            {
                waveEditorUI.UpdateWaveList(currentLevel.waves);
            }
        }
        
        public void RemoveWave(int index)
        {
            if (index < 0 || index >= currentLevel.waves.Count)
            {
                Debug.LogWarning("LevelEditor: Invalid wave index!");
                return;
            }
            
            currentLevel.waves.RemoveAt(index);
            
            if (waveEditorUI != null)
            {
                waveEditorUI.UpdateWaveList(currentLevel.waves);
            }
        }
        
        public void UpdateWave(int index, WaveManager.Wave wave)
        {
            if (index < 0 || index >= currentLevel.waves.Count)
            {
                Debug.LogWarning("LevelEditor: Invalid wave index!");
                return;
            }
            
            currentLevel.waves[index] = wave;
            
            if (waveEditorUI != null)
            {
                waveEditorUI.UpdateWaveList(currentLevel.waves);
            }
        }
        
        public void ValidateLevel()
        {
            if (levelValidator == null || validationUI == null)
            {
                Debug.LogError("LevelEditor: Required components not found!");
                return;
            }
            
            LevelValidator.ValidationResult result = levelValidator.ValidateLevel(currentLevel);
            validationUI.ShowValidationResults(result);
        }
        
        public void SaveLevel()
        {
            // TODO: Implement level saving
            Debug.Log("Saving level...");
        }
        
        public void LoadLevel(string levelId)
        {
            // TODO: Implement level loading
            Debug.Log($"Loading level: {levelId}");
        }
        
        public void PublishLevel()
        {
            if (levelValidator == null)
            {
                Debug.LogError("LevelEditor: LevelValidator not found!");
                return;
            }
            
            LevelValidator.ValidationResult result = levelValidator.ValidateLevel(currentLevel);
            
            if (!result.isValid)
            {
                Debug.LogWarning("LevelEditor: Cannot publish invalid level!");
                return;
            }
            
            // TODO: Implement level publishing
            Debug.Log("Publishing level...");
        }
        
        private void OnDestroy()
        {
            // Clean up resources
            StopEditing();
        }
    }
} 