using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public DateTime saveDate;
        public string playerName;
        public int playerLevel;
        public int playerExperience;
        
        // Only store serializable data (references to cards stored by ID)
        public PlayerSaveData playerData;
        public ProgressSaveData progressData;
        public SettingsSaveData settingsData;
        public AchievementSaveData achievementData;
        
        public GameSaveData()
        {
            saveDate = DateTime.Now;
            playerData = new PlayerSaveData();
            progressData = new ProgressSaveData();
            settingsData = new SettingsSaveData();
            achievementData = new AchievementSaveData();
        }
    }
    
    [Serializable]
    public class PlayerSaveData
    {
        public Dictionary<string, int> currencyAmounts = new Dictionary<string, int>();
        public List<string> ownedCardIds = new List<string>();
        public Dictionary<string, int> ownedCardQuantities = new Dictionary<string, int>();
        public List<DeckSaveData> decks = new List<DeckSaveData>();
        public string activeDeckId;
    }
    
    [Serializable]
    public class DeckSaveData
    {
        public string id;
        public string name;
        public string description;
        public List<string> cardIds = new List<string>();
        public DateTime createdAt;
        public DateTime lastModified;
    }
    
    [Serializable]
    public class ProgressSaveData
    {
        public int highestLevelCompleted;
        public Dictionary<string, int> levelStars = new Dictionary<string, int>();
        public Dictionary<string, bool> unlockedContent = new Dictionary<string, bool>();
        public Dictionary<string, int> towerUpgrades = new Dictionary<string, int>();
    }
    
    [Serializable]
    public class SettingsSaveData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public int graphicsQuality = 1;
        public bool vibrationEnabled = true;
        public bool tutorialEnabled = true;
        public string languageCode = "en";
    }
    
    [Serializable]
    public class AchievementSaveData
    {
        public Dictionary<string, bool> achievementUnlocked = new Dictionary<string, bool>();
        public Dictionary<string, int> achievementProgress = new Dictionary<string, int>();
    }
    
    public class SaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool useEncryption = true;
        [SerializeField] private bool allowMultipleSaves = false;
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private string saveFileName = "save.dat";
        
        [Header("Autosave Events")]
        [SerializeField] private bool saveOnLevelComplete = true;
        [SerializeField] private bool saveOnCardAcquired = true;
        [SerializeField] private bool saveOnAppQuit = true;
        [SerializeField] private bool saveOnAppPause = true;
        
        // References
        private PlayerCollection playerCollection;
        private CurrencyManager currencyManager;
        private DeckManager deckManager;
        
        // State
        private GameSaveData currentSaveData;
        private float lastAutoSaveTime;
        private bool saveInProgress = false;
        private string savePath;
        
        // Events
        public event Action<bool, string> OnSaveCompleted;
        public event Action<bool, string> OnLoadCompleted;
        
        public static SaveManager Instance { get; private set; }
        
        public bool HasSaveData => File.Exists(savePath);
        public DateTime LastSaveTime => currentSaveData?.saveDate ?? DateTime.MinValue;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize path
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            // Initialize save data
            currentSaveData = new GameSaveData();
            
            // Load on start
            AutoLoad();
        }
        
        private void Start()
        {
            // Find game managers
            playerCollection = PlayerCollection.Instance;
            currencyManager = CurrencyManager.Instance;
            deckManager = DeckManager.Instance;
            
            // Subscribe to events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void Update()
        {
            // Check for autosave
            if (autoSaveEnabled && Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                AutoSave();
            }
        }
        
        private void OnApplicationQuit()
        {
            if (saveOnAppQuit)
            {
                AutoSave();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveOnAppPause)
            {
                AutoSave();
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reconnect to instances that might have been recreated
            playerCollection = PlayerCollection.Instance;
            currencyManager = CurrencyManager.Instance;
            deckManager = DeckManager.Instance;
        }
        
        public void AutoSave()
        {
            if (saveInProgress)
                return;
            
            lastAutoSaveTime = Time.time;
            SaveGame(0);
        }
        
        public void AutoLoad()
        {
            LoadGame(0);
        }
        
        public void SaveGame(int slotIndex = 0)
        {
            if (saveInProgress)
            {
                OnSaveCompleted?.Invoke(false, "Save already in progress");
                return;
            }
            
            saveInProgress = true;
            
            // Construct save path
            string path = savePath;
            if (allowMultipleSaves && slotIndex > 0)
            {
                path = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.dat");
            }
            
            // Create new save data
            GameSaveData saveData = new GameSaveData
            {
                saveDate = DateTime.Now,
                playerName = PlayerPrefs.GetString("PlayerName", "Player"),
                playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1),
                playerExperience = PlayerPrefs.GetInt("PlayerExperience", 0)
            };
            
            // Save player collection data
            if (playerCollection != null)
            {
                List<YuGiOhCard> playerCards = playerCollection.GetAllCards();
                foreach (var card in playerCards)
                {
                    string cardId = card.Id;
                    int quantity = playerCollection.GetCardQuantity(cardId);
                    
                    saveData.playerData.ownedCardIds.Add(cardId);
                    saveData.playerData.ownedCardQuantities[cardId] = quantity;
                }
            }
            
            // Save currency data
            if (currencyManager != null)
            {
                foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
                {
                    int amount = currencyManager.GetCurrency(type);
                    saveData.playerData.currencyAmounts[type.ToString()] = amount;
                }
            }
            
            // Save deck data
            if (deckManager != null)
            {
                List<Deck> decks = deckManager.GetAllDecks();
                foreach (var deck in decks)
                {
                    DeckSaveData deckData = new DeckSaveData
                    {
                        id = Guid.NewGuid().ToString(),
                        name = deck.name,
                        description = deck.description,
                        cardIds = new List<string>(deck.cardIds),
                        createdAt = deck.createdAt,
                        lastModified = deck.lastModified
                    };
                    
                    saveData.playerData.decks.Add(deckData);
                    
                    // Save active deck
                    if (deckManager.ActiveDeck == deck)
                    {
                        saveData.playerData.activeDeckId = deckData.id;
                    }
                }
            }
            
            // Save progress data
            saveData.progressData.highestLevelCompleted = PlayerPrefs.GetInt("HighestLevel", 0);
            
            // Save settings data
            saveData.settingsData.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            saveData.settingsData.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            saveData.settingsData.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            saveData.settingsData.graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
            saveData.settingsData.vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
            saveData.settingsData.tutorialEnabled = PlayerPrefs.GetInt("TutorialEnabled", 1) == 1;
            saveData.settingsData.languageCode = PlayerPrefs.GetString("LanguageCode", "en");
            
            // Serialize and save
            string json = JsonUtility.ToJson(saveData);
            
            try
            {
                if (useEncryption)
                {
                    json = EncryptDecrypt(json);
                }
                
                File.WriteAllText(path, json);
                currentSaveData = saveData;
                
                Debug.Log($"Game saved successfully to {path}");
                OnSaveCompleted?.Invoke(true, "Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
                OnSaveCompleted?.Invoke(false, $"Error saving game: {e.Message}");
            }
            finally
            {
                saveInProgress = false;
            }
        }
        
        public async void SaveGameAsync(int slotIndex = 0)
        {
            if (saveInProgress)
            {
                OnSaveCompleted?.Invoke(false, "Save already in progress");
                return;
            }
            
            saveInProgress = true;
            
            try
            {
                await Task.Run(() => {
                    // Construct save path
                    string path = savePath;
                    if (allowMultipleSaves && slotIndex > 0)
                    {
                        path = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.dat");
                    }
                    
                    // Same data collection as in SaveGame, but runs on a background thread
                    // Note: We need to be careful with Unity objects in a background thread
                    
                    // Create new save data
                    GameSaveData saveData = new GameSaveData
                    {
                        saveDate = DateTime.Now,
                        playerName = PlayerPrefs.GetString("PlayerName", "Player"),
                        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1),
                        playerExperience = PlayerPrefs.GetInt("PlayerExperience", 0)
                    };
                    
                    // Serialize
                    string json = JsonUtility.ToJson(saveData);
                    
                    if (useEncryption)
                    {
                        json = EncryptDecrypt(json);
                    }
                    
                    File.WriteAllText(path, json);
                    
                    // We need to update the current save data on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        currentSaveData = saveData;
                    });
                });
                
                Debug.Log("Game saved successfully (async)");
                OnSaveCompleted?.Invoke(true, "Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
                OnSaveCompleted?.Invoke(false, $"Error saving game: {e.Message}");
            }
            finally
            {
                saveInProgress = false;
            }
        }
        
        public void LoadGame(int slotIndex = 0)
        {
            // Construct save path
            string path = savePath;
            if (allowMultipleSaves && slotIndex > 0)
            {
                path = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.dat");
            }
            
            if (!File.Exists(path))
            {
                Debug.Log($"No save file found at {path}");
                OnLoadCompleted?.Invoke(false, "No save file found");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(path);
                
                if (useEncryption)
                {
                    json = EncryptDecrypt(json);
                }
                
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                if (saveData == null)
                {
                    Debug.LogError("Failed to deserialize save data");
                    OnLoadCompleted?.Invoke(false, "Failed to deserialize save data");
                    return;
                }
                
                // Store save data
                currentSaveData = saveData;
                
                // Apply player data
                PlayerPrefs.SetString("PlayerName", saveData.playerName);
                PlayerPrefs.SetInt("PlayerLevel", saveData.playerLevel);
                PlayerPrefs.SetInt("PlayerExperience", saveData.playerExperience);
                
                // Load cards into player collection
                if (playerCollection != null)
                {
                    playerCollection.ClearCollection();
                    
                    for (int i = 0; i < saveData.playerData.ownedCardIds.Count; i++)
                    {
                        string cardId = saveData.playerData.ownedCardIds[i];
                        int quantity = 1;
                        
                        if (saveData.playerData.ownedCardQuantities.ContainsKey(cardId))
                        {
                            quantity = saveData.playerData.ownedCardQuantities[cardId];
                        }
                        
                        playerCollection.AddCard(cardId, quantity);
                    }
                }
                
                // Load currency
                if (currencyManager != null)
                {
                    currencyManager.ResetCurrency();
                    
                    foreach (var currency in saveData.playerData.currencyAmounts)
                    {
                        if (Enum.TryParse<CurrencyType>(currency.Key, out CurrencyType type))
                        {
                            currencyManager.AddCurrency(type, currency.Value, "Loaded from save");
                        }
                    }
                }
                
                // Load decks
                if (deckManager != null)
                {
                    // Clear existing decks
                    foreach (var deck in deckManager.GetAllDecks())
                    {
                        deckManager.DeleteDeck(deck);
                    }
                    
                    // Create decks from save data
                    Deck activeDeck = null;
                    
                    foreach (var deckData in saveData.playerData.decks)
                    {
                        Deck deck = deckManager.CreateDeck(deckData.name, deckData.description);
                        
                        if (deck != null)
                        {
                            // Add cards to deck
                            foreach (string cardId in deckData.cardIds)
                            {
                                deckManager.AddCardToDeck(deck, cardId);
                            }
                            
                            // Check if this is the active deck
                            if (deckData.id == saveData.playerData.activeDeckId)
                            {
                                activeDeck = deck;
                            }
                        }
                    }
                    
                    // Set active deck
                    if (activeDeck != null)
                    {
                        deckManager.SetActiveDeck(activeDeck);
                    }
                }
                
                // Apply settings
                PlayerPrefs.SetFloat("MasterVolume", saveData.settingsData.masterVolume);
                PlayerPrefs.SetFloat("MusicVolume", saveData.settingsData.musicVolume);
                PlayerPrefs.SetFloat("SFXVolume", saveData.settingsData.sfxVolume);
                PlayerPrefs.SetInt("GraphicsQuality", saveData.settingsData.graphicsQuality);
                PlayerPrefs.SetInt("VibrationEnabled", saveData.settingsData.vibrationEnabled ? 1 : 0);
                PlayerPrefs.SetInt("TutorialEnabled", saveData.settingsData.tutorialEnabled ? 1 : 0);
                PlayerPrefs.SetString("LanguageCode", saveData.settingsData.languageCode);
                
                // Apply progress data
                PlayerPrefs.SetInt("HighestLevel", saveData.progressData.highestLevelCompleted);
                
                PlayerPrefs.Save();
                
                Debug.Log($"Game loaded successfully from {path}");
                OnLoadCompleted?.Invoke(true, "Game loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                OnLoadCompleted?.Invoke(false, $"Error loading game: {e.Message}");
            }
        }
        
        public bool DeleteSave(int slotIndex = 0)
        {
            // Construct save path
            string path = savePath;
            if (allowMultipleSaves && slotIndex > 0)
            {
                path = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.dat");
            }
            
            if (!File.Exists(path))
            {
                return false;
            }
            
            try
            {
                File.Delete(path);
                Debug.Log($"Save file deleted: {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting save file: {e.Message}");
                return false;
            }
        }
        
        public List<GameSaveData> GetAllSaves()
        {
            List<GameSaveData> saves = new List<GameSaveData>();
            
            if (!allowMultipleSaves)
            {
                if (File.Exists(savePath))
                {
                    try
                    {
                        string json = File.ReadAllText(savePath);
                        if (useEncryption)
                        {
                            json = EncryptDecrypt(json);
                        }
                        
                        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                        if (saveData != null)
                        {
                            saves.Add(saveData);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error reading save file: {e.Message}");
                    }
                }
            }
            else
            {
                for (int i = 0; i <= maxSaveSlots; i++)
                {
                    string path = (i == 0) ? savePath : Path.Combine(Application.persistentDataPath, $"save_{i}.dat");
                    
                    if (File.Exists(path))
                    {
                        try
                        {
                            string json = File.ReadAllText(path);
                            if (useEncryption)
                            {
                                json = EncryptDecrypt(json);
                            }
                            
                            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                            if (saveData != null)
                            {
                                saves.Add(saveData);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error reading save file: {e.Message}");
                        }
                    }
                }
            }
            
            return saves;
        }
        
        // Simple XOR encryption/decryption
        private string EncryptDecrypt(string data)
        {
            string key = "YuGiOhTowerDefense";
            char[] result = new char[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ key[i % key.Length]);
            }
            
            return new string(result);
        }
    }
    
    // Helper class to run code on the main thread
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
        
        private void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
        
        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
} 