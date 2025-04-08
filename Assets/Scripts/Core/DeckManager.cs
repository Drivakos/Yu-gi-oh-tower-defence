using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        [Header("Deck Settings")]
        [SerializeField] private int maxDecks = 10;
        [SerializeField] private int minDeckSize = 20;
        [SerializeField] private int maxDeckSize = 60;
        [SerializeField] private int maxCopiesPerCard = 3;
        
        [Header("References")]
        [SerializeField] private YuGiOhAPIManager apiManager;
        [SerializeField] private PlayerCollection playerCollection;
        
        private List<Deck> playerDecks = new List<Deck>();
        private Deck activeDeck;
        
        // Events
        public event Action<Deck> OnDeckCreated;
        public event Action<Deck> OnDeckModified;
        public event Action<Deck> OnDeckDeleted;
        public event Action<Deck> OnActiveDeckChanged;
        
        public static DeckManager Instance { get; private set; }
        
        public Deck ActiveDeck => activeDeck;
        public int MinDeckSize => minDeckSize;
        public int MaxDeckSize => maxDeckSize;
        
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
            
            if (apiManager == null)
            {
                apiManager = FindObjectOfType<YuGiOhAPIManager>();
                if (apiManager == null)
                {
                    Debug.LogError("YuGiOhAPIManager not found!");
                }
            }
            
            if (playerCollection == null)
            {
                playerCollection = PlayerCollection.Instance;
                if (playerCollection == null)
                {
                    Debug.LogError("PlayerCollection not found!");
                }
            }
            
            LoadDecks();
        }
        
        public Deck CreateDeck(string name, string description = "")
        {
            if (playerDecks.Count >= maxDecks)
            {
                Debug.LogWarning($"Maximum number of decks ({maxDecks}) reached. Delete a deck before creating a new one.");
                return null;
            }
            
            if (string.IsNullOrEmpty(name))
            {
                name = $"Deck {playerDecks.Count + 1}";
            }
            
            Deck newDeck = new Deck(name, description);
            playerDecks.Add(newDeck);
            
            SaveDecks();
            OnDeckCreated?.Invoke(newDeck);
            
            // If this is the first deck, set it as active
            if (playerDecks.Count == 1)
            {
                SetActiveDeck(newDeck);
            }
            
            return newDeck;
        }
        
        public bool RenameDeck(Deck deck, string newName)
        {
            if (deck == null || string.IsNullOrEmpty(newName))
            {
                return false;
            }
            
            deck.name = newName;
            deck.UpdateLastModified();
            
            SaveDecks();
            OnDeckModified?.Invoke(deck);
            
            return true;
        }
        
        public bool UpdateDeckDescription(Deck deck, string newDescription)
        {
            if (deck == null)
            {
                return false;
            }
            
            deck.description = newDescription;
            deck.UpdateLastModified();
            
            SaveDecks();
            OnDeckModified?.Invoke(deck);
            
            return true;
        }
        
        public bool DeleteDeck(Deck deck)
        {
            if (deck == null)
            {
                return false;
            }
            
            bool wasActive = (deck == activeDeck);
            
            if (playerDecks.Remove(deck))
            {
                SaveDecks();
                OnDeckDeleted?.Invoke(deck);
                
                // If the active deck was deleted, select a new one if available
                if (wasActive)
                {
                    activeDeck = null;
                    if (playerDecks.Count > 0)
                    {
                        SetActiveDeck(playerDecks[0]);
                    }
                    else
                    {
                        OnActiveDeckChanged?.Invoke(null);
                    }
                }
                
                return true;
            }
            
            return false;
        }
        
        public bool SetActiveDeck(Deck deck)
        {
            if (deck == null || !playerDecks.Contains(deck))
            {
                return false;
            }
            
            if (activeDeck != deck)
            {
                activeDeck = deck;
                OnActiveDeckChanged?.Invoke(activeDeck);
            }
            
            return true;
        }
        
        public bool AddCardToDeck(Deck deck, string cardId)
        {
            if (deck == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }
            
            // Check if player owns the card
            if (playerCollection != null && !playerCollection.HasCard(cardId))
            {
                Debug.LogWarning($"Cannot add card {cardId} to deck: Player does not own this card");
                return false;
            }
            
            // Check if the deck is already at maximum size
            if (deck.cardIds.Count >= maxDeckSize)
            {
                Debug.LogWarning($"Cannot add card to deck: Deck already at maximum size ({maxDeckSize})");
                return false;
            }
            
            // Check if there are already the maximum number of copies of this card in the deck
            int copiesInDeck = deck.cardIds.Count(id => id == cardId);
            if (copiesInDeck >= maxCopiesPerCard)
            {
                Debug.LogWarning($"Cannot add more copies of card {cardId}: Maximum ({maxCopiesPerCard}) already in deck");
                return false;
            }
            
            // Add the card to the deck
            deck.cardIds.Add(cardId);
            deck.UpdateLastModified();
            
            SaveDecks();
            OnDeckModified?.Invoke(deck);
            
            return true;
        }
        
        public bool RemoveCardFromDeck(Deck deck, string cardId, bool removeAll = false)
        {
            if (deck == null || string.IsNullOrEmpty(cardId))
            {
                return false;
            }
            
            if (removeAll)
            {
                int removedCount = deck.cardIds.RemoveAll(id => id == cardId);
                if (removedCount > 0)
                {
                    deck.UpdateLastModified();
                    SaveDecks();
                    OnDeckModified?.Invoke(deck);
                    return true;
                }
            }
            else
            {
                int index = deck.cardIds.FindIndex(id => id == cardId);
                if (index >= 0)
                {
                    deck.cardIds.RemoveAt(index);
                    deck.UpdateLastModified();
                    SaveDecks();
                    OnDeckModified?.Invoke(deck);
                    return true;
                }
            }
            
            return false;
        }
        
        public bool IsDeckValid(Deck deck)
        {
            if (deck == null)
            {
                return false;
            }
            
            // Check if deck meets minimum size requirement
            if (deck.cardIds.Count < minDeckSize)
            {
                return false;
            }
            
            // Check if all cards are owned by the player
            if (playerCollection != null)
            {
                Dictionary<string, int> cardCounts = new Dictionary<string, int>();
                
                foreach (string cardId in deck.cardIds)
                {
                    if (!cardCounts.ContainsKey(cardId))
                    {
                        cardCounts[cardId] = 0;
                    }
                    
                    cardCounts[cardId]++;
                    
                    // Check if player owns enough copies of this card
                    if (cardCounts[cardId] > playerCollection.GetCardQuantity(cardId))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        public List<Deck> GetAllDecks()
        {
            return new List<Deck>(playerDecks);
        }
        
        public Deck GetDeckByName(string name)
        {
            return playerDecks.FirstOrDefault(deck => deck.name == name);
        }
        
        private void SaveDecks()
        {
            DeckData data = new DeckData
            {
                decks = playerDecks,
                activeDeckName = (activeDeck != null) ? activeDeck.name : ""
            };
            
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("PlayerDecks", json);
            PlayerPrefs.Save();
            
            Debug.Log($"Saved {playerDecks.Count} decks");
        }
        
        private void LoadDecks()
        {
            playerDecks.Clear();
            activeDeck = null;
            
            if (PlayerPrefs.HasKey("PlayerDecks"))
            {
                string json = PlayerPrefs.GetString("PlayerDecks");
                DeckData data = JsonUtility.FromJson<DeckData>(json);
                
                if (data != null && data.decks != null)
                {
                    playerDecks = data.decks;
                    
                    // Find active deck
                    if (!string.IsNullOrEmpty(data.activeDeckName))
                    {
                        activeDeck = playerDecks.FirstOrDefault(deck => deck.name == data.activeDeckName);
                    }
                    
                    Debug.Log($"Loaded {playerDecks.Count} decks");
                }
            }
            
            if (activeDeck == null && playerDecks.Count > 0)
            {
                activeDeck = playerDecks[0];
            }
            
            // Clear cached card data
            foreach (var deck in playerDecks)
            {
                deck.ClearCache();
            }
        }
        
        public Dictionary<string, int> GetDeckStats(Deck deck)
        {
            Dictionary<string, int> stats = new Dictionary<string, int>
            {
                { "Total Cards", deck.cardIds.Count },
                { "Monster Cards", 0 },
                { "Spell Cards", 0 },
                { "Trap Cards", 0 }
            };
            
            // Get full card data to analyze types
            List<YuGiOhCard> cards = deck.GetCards(apiManager);
            
            foreach (var card in cards)
            {
                if (card.Type.Contains("Monster"))
                {
                    stats["Monster Cards"]++;
                }
                else if (card.Type.Contains("Spell"))
                {
                    stats["Spell Cards"]++;
                }
                else if (card.Type.Contains("Trap"))
                {
                    stats["Trap Cards"]++;
                }
            }
            
            return stats;
        }
        
        [Serializable]
        private class DeckData
        {
            public List<Deck> decks;
            public string activeDeckName;
        }
    }
} 