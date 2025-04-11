using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuGiOhTowerDefense.Cards;
using System.IO;
using Newtonsoft.Json;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class Deck
    {
        public string name;
        public string description;
        public List<string> cardIds = new List<string>();
        public DateTime createdAt;
        public DateTime lastModified;
        
        [NonSerialized]
        private List<YuGiOhCard> _cachedCards;
        
        public Deck(string name, string description = "")
        {
            this.name = name;
            this.description = description;
            this.createdAt = DateTime.Now;
            this.lastModified = DateTime.Now;
        }
        
        public void UpdateLastModified()
        {
            lastModified = DateTime.Now;
            _cachedCards = null; // Clear cache when deck is modified
        }
        
        public void ClearCache()
        {
            _cachedCards = null;
        }
        
        public List<YuGiOhCard> GetCards(YuGiOhAPIManager apiManager)
        {
            if (_cachedCards != null)
            {
                return _cachedCards;
            }
            
            _cachedCards = new List<YuGiOhCard>();
            
            if (apiManager == null)
            {
                Debug.LogWarning("API Manager not available to retrieve card data");
                return _cachedCards;
            }
            
            foreach (string cardId in cardIds)
            {
                YuGiOhCard card = apiManager.GetCardById(cardId);
                if (card != null)
                {
                    _cachedCards.Add(card);
                }
            }
            
            return _cachedCards;
        }
    }
    
    public class DeckManager : MonoBehaviour
    {
        private static DeckManager instance;
        public static DeckManager Instance => instance;
        
        [Header("Settings")]
        [SerializeField] private string decksFolder = "Decks";
        [SerializeField] private string defaultDeckName = "Default Deck";
        
        private string decksPath;
        private List<DeckData> loadedDecks = new List<DeckData>();
        
        public event Action<List<DeckData>> OnDecksLoaded;
        public event Action<DeckData> OnDeckSaved;
        public event Action<DeckData> OnDeckDeleted;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            decksPath = Path.Combine(Application.persistentDataPath, decksFolder);
            
            if (!Directory.Exists(decksPath))
            {
                Directory.CreateDirectory(decksPath);
            }
            
            LoadAllDecks();
        }
        
        public void LoadAllDecks()
        {
            loadedDecks.Clear();
            
            try
            {
                string[] deckFiles = Directory.GetFiles(decksPath, "*.json");
                
                foreach (string file in deckFiles)
                {
                    string json = File.ReadAllText(file);
                    DeckData deck = JsonConvert.DeserializeObject<DeckData>(json);
                    
                    if (deck != null)
                    {
                        loadedDecks.Add(deck);
                    }
                }
                
                OnDecksLoaded?.Invoke(loadedDecks);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading decks: {e.Message}");
            }
        }
        
        public void SaveDeck(DeckData deck)
        {
            if (deck == null)
            {
                Debug.LogError("Cannot save null deck");
                return;
            }
            
            try
            {
                // Validate deck
                if (!ValidateDeck(deck))
                {
                    Debug.LogError("Invalid deck data");
                    return;
                }
                
                // Generate unique filename
                string filename = $"{deck.DeckName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(decksPath, filename);
                
                // Serialize and save
                string json = JsonConvert.SerializeObject(deck, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                // Update loaded decks
                int existingIndex = loadedDecks.FindIndex(d => d.DeckName == deck.DeckName);
                if (existingIndex >= 0)
                {
                    loadedDecks[existingIndex] = deck;
                }
                else
                {
                    loadedDecks.Add(deck);
                }
                
                OnDeckSaved?.Invoke(deck);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving deck: {e.Message}");
            }
        }
        
        public void DeleteDeck(string deckName)
        {
            try
            {
                string[] files = Directory.GetFiles(decksPath, $"{deckName}_*.json");
                
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                
                DeckData deck = loadedDecks.FirstOrDefault(d => d.DeckName == deckName);
                if (deck != null)
                {
                    loadedDecks.Remove(deck);
                    OnDeckDeleted?.Invoke(deck);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting deck: {e.Message}");
            }
        }
        
        public DeckData GetDeck(string deckName)
        {
            return loadedDecks.FirstOrDefault(d => d.DeckName == deckName);
        }
        
        public List<DeckData> GetAllDecks()
        {
            return new List<DeckData>(loadedDecks);
        }
        
        public DeckData CreateDefaultDeck()
        {
            // TODO: Implement default deck creation with basic cards
            return new DeckData
            {
                DeckName = defaultDeckName,
                Cards = new List<CardData>()
            };
        }
        
        private bool ValidateDeck(DeckData deck)
        {
            if (string.IsNullOrEmpty(deck.DeckName))
            {
                Debug.LogError("Deck name cannot be empty");
                return false;
            }
            
            if (deck.Cards == null)
            {
                Debug.LogError("Deck cards list cannot be null");
                return false;
            }
            
            // Check deck size
            if (deck.Cards.Count < 40 || deck.Cards.Count > 60)
            {
                Debug.LogError("Deck size must be between 40 and 60 cards");
                return false;
            }
            
            // Check card copy limits
            var cardCounts = deck.Cards.GroupBy(c => c.CardId)
                                     .ToDictionary(g => g.Key, g => g.Count());
            
            if (cardCounts.Values.Any(count => count > 3))
            {
                Debug.LogError("Cannot have more than 3 copies of any card");
                return false;
            }
            
            return true;
        }
        
        public bool IsDeckValid(DeckData deck)
        {
            return ValidateDeck(deck);
        }
        
        public string GetDeckValidationError(DeckData deck)
        {
            if (string.IsNullOrEmpty(deck.DeckName))
            {
                return "Deck name cannot be empty";
            }
            
            if (deck.Cards == null)
            {
                return "Deck cards list cannot be null";
            }
            
            if (deck.Cards.Count < 40)
            {
                return "Deck must have at least 40 cards";
            }
            
            if (deck.Cards.Count > 60)
            {
                return "Deck cannot have more than 60 cards";
            }
            
            var cardCounts = deck.Cards.GroupBy(c => c.CardId)
                                     .ToDictionary(g => g.Key, g => g.Count());
            
            var invalidCards = cardCounts.Where(kvp => kvp.Value > 3)
                                       .Select(kvp => kvp.Key);
            
            if (invalidCards.Any())
            {
                return $"Cannot have more than 3 copies of any card: {string.Join(", ", invalidCards)}";
            }
            
            return null;
        }
    }
} 