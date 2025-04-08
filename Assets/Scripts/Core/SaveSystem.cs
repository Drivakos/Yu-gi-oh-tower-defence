using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core
{
    public class SaveSystem : MonoBehaviour
    {
        [System.Serializable]
        public class GameSaveData
        {
            public int duelPoints;
            public int lifePoints;
            public int currentWave;
            public float difficultyMultiplier;
            public List<MonsterSaveData> placedMonsters;
            public List<CardSaveData> availableCards;
        }

        [System.Serializable]
        public class MonsterSaveData
        {
            public string monsterId;
            public Vector3 position;
            public Quaternion rotation;
            public float currentHealth;
            public int level;
        }

        [System.Serializable]
        public class CardSaveData
        {
            public string cardId;
            public bool isAvailable;
        }

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "yugioh_td_save.dat";
        [SerializeField] private bool useEncryption = true;
        
        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private GameManager gameManager;
        private WaveManager waveManager;
        private TileManager tileManager;
        
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            waveManager = FindObjectOfType<WaveManager>();
            tileManager = FindObjectOfType<TileManager>();
        }
        
        public void SaveGame()
        {
            GameSaveData saveData = new GameSaveData
            {
                duelPoints = gameManager.GetDuelPoints(),
                lifePoints = gameManager.GetLifePoints(),
                currentWave = waveManager.GetCurrentWaveIndex(),
                difficultyMultiplier = waveManager.GetCurrentDifficultyMultiplier(),
                placedMonsters = new List<MonsterSaveData>(),
                availableCards = new List<CardSaveData>()
            };
            
            // Save placed monsters
            foreach (var tile in tileManager.GetAllTiles())
            {
                if (tile.IsOccupied())
                {
                    Monster monster = tile.GetPlacedMonster();
                    if (monster != null)
                    {
                        saveData.placedMonsters.Add(new MonsterSaveData
                        {
                            monsterId = monster.GetStats().id,
                            position = monster.transform.position,
                            rotation = monster.transform.rotation,
                            currentHealth = monster.GetCurrentHealth(),
                            level = monster.GetLevel()
                        });
                    }
                }
            }
            
            // Save available cards
            foreach (var card in gameManager.GetAvailableCards())
            {
                saveData.availableCards.Add(new CardSaveData
                {
                    cardId = card.GetStats().id,
                    isAvailable = true
                });
            }
            
            // Serialize and save data
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(SavePath, FileMode.Create))
            {
                if (useEncryption)
                {
                    // Simple XOR encryption with a key
                    byte[] data = SerializeToBytes(saveData);
                    byte[] key = System.Text.Encoding.UTF8.GetBytes("YUGIOH_TD_KEY");
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(data[i] ^ key[i % key.Length]);
                    }
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    formatter.Serialize(stream, saveData);
                }
            }
            
            Debug.Log($"Game saved to {SavePath}");
        }
        
        public bool LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found!");
                return false;
            }
            
            GameSaveData saveData;
            
            try
            {
                if (useEncryption)
                {
                    // Decrypt data
                    byte[] data = File.ReadAllBytes(SavePath);
                    byte[] key = System.Text.Encoding.UTF8.GetBytes("YUGIOH_TD_KEY");
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(data[i] ^ key[i % key.Length]);
                    }
                    saveData = DeserializeFromBytes(data);
                }
                else
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream stream = new FileStream(SavePath, FileMode.Open))
                    {
                        saveData = (GameSaveData)formatter.Deserialize(stream);
                    }
                }
                
                // Restore game state
                gameManager.SetDuelPoints(saveData.duelPoints);
                gameManager.SetLifePoints(saveData.lifePoints);
                waveManager.SetWaveIndex(saveData.currentWave);
                waveManager.SetDifficultyMultiplier(saveData.difficultyMultiplier);
                
                // Clear existing monsters
                tileManager.ClearAllTiles();
                
                // Restore placed monsters
                foreach (var monsterData in saveData.placedMonsters)
                {
                    Monster monsterPrefab = gameManager.GetMonsterById(monsterData.monsterId);
                    if (monsterPrefab != null)
                    {
                        Monster monster = Instantiate(monsterPrefab, monsterData.position, monsterData.rotation);
                        monster.Initialize(1f, 1f, 1f); // Base multipliers
                        monster.SetLevel(monsterData.level);
                        monster.SetCurrentHealth(monsterData.currentHealth);
                        
                        // Find nearest tile and place monster
                        Tile nearestTile = tileManager.GetNearestTile(monsterData.position);
                        if (nearestTile != null)
                        {
                            nearestTile.SetPlacedMonster(monster);
                        }
                    }
                }
                
                // Restore available cards
                foreach (var cardData in saveData.availableCards)
                {
                    if (cardData.isAvailable)
                    {
                        gameManager.UnlockCard(cardData.cardId);
                    }
                }
                
                Debug.Log("Game loaded successfully!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading save file: {e.Message}");
                return false;
            }
        }
        
        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted!");
            }
        }
        
        private byte[] SerializeToBytes(GameSaveData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                return stream.ToArray();
            }
        }
        
        private GameSaveData DeserializeFromBytes(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                return (GameSaveData)formatter.Deserialize(stream);
            }
        }
    }
} 