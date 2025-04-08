using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class CollectionCard
    {
        public string cardId;
        public int quantity;
        public bool isNew;
        public DateTime dateAcquired;
        
        public CollectionCard(string id, int qty = 1)
        {
            cardId = id;
            quantity = qty;
            isNew = true;
            dateAcquired = DateTime.Now;
        }
    }
    
    public class PlayerCollection : MonoBehaviour
    {
        [Header("Collection Settings")]
        [SerializeField] private int maxCopiesPerCard = 3;
        
        [Header("References")]
        [SerializeField] private YuGiOhAPIManager apiManager;
        
        private Dictionary<string, CollectionCard> cardCollection = new Dictionary<string, CollectionCard>();
        private List<YuGiOhCard> cachedCardData = null;
        
        public static PlayerCollection Instance { get; private set; }
        
        public int TotalCards => cardCollection.Values.Sum(card => card.quantity);
        public int UniqueCards => cardCollection.Count;
        
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
            
            LoadCollection();
        }
        
        public void AddCard(string cardId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("Attempted to add card with null or empty ID");
                return;
            }
            
            if (cardCollection.TryGetValue(cardId, out CollectionCard card))
            {
                // Card already exists in collection, increase quantity
                int newQuantity = Mathf.Min(card.quantity + quantity, maxCopiesPerCard);
                int added = newQuantity - card.quantity;
                
                card.quantity = newQuantity;
                card.isNew = added > 0;
                
                if (added < quantity)
                {
                    Debug.Log($"Added {added} copies of card {cardId} (reached max of {maxCopiesPerCard})");
                }
                else
                {
                    Debug.Log($"Added {quantity} copies of card {cardId}");
                }
            }
            else
            {
                // New card, add to collection
                cardCollection.Add(cardId, new CollectionCard(cardId, Mathf.Min(quantity, maxCopiesPerCard)));
                Debug.Log($"Added new card {cardId} to collection");
            }
            
            SaveCollection();
        }
        
        public void AddCards(List<YuGiOhCard> cards)
        {
            if (cards == null || cards.Count == 0)
            {
                return;
            }
            
            foreach (var card in cards)
            {
                if (card != null && !string.IsNullOrEmpty(card.Id))
                {
                    AddCard(card.Id);
                }
            }
        }
        
        public bool RemoveCard(string cardId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(cardId) || !cardCollection.TryGetValue(cardId, out CollectionCard card))
            {
                return false;
            }
            
            if (card.quantity <= quantity)
            {
                // Remove the card entirely
                cardCollection.Remove(cardId);
                Debug.Log($"Removed card {cardId} from collection");
            }
            else
            {
                // Reduce quantity
                card.quantity -= quantity;
                Debug.Log($"Removed {quantity} copies of card {cardId}");
            }
            
            SaveCollection();
            return true;
        }
        
        public int GetCardQuantity(string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || !cardCollection.TryGetValue(cardId, out CollectionCard card))
            {
                return 0;
            }
            
            return card.quantity;
        }
        
        public bool HasCard(string cardId, int quantity = 1)
        {
            return GetCardQuantity(cardId) >= quantity;
        }
        
        public List<YuGiOhCard> GetAllCards()
        {
            if (apiManager == null || !apiManager.IsInitialized)
            {
                Debug.LogWarning("API Manager not initialized, unable to get card data");
                return new List<YuGiOhCard>();
            }
            
            if (cachedCardData == null)
            {
                // Cache all card data
                cachedCardData = apiManager.GetAllCards();
            }
            
            List<YuGiOhCard> collectionCards = new List<YuGiOhCard>();
            
            foreach (var collectionCard in cardCollection)
            {
                YuGiOhCard cardData = cachedCardData.FirstOrDefault(card => card.Id == collectionCard.Key);
                if (cardData != null)
                {
                    collectionCards.Add(cardData);
                }
            }
            
            return collectionCards;
        }
        
        public List<YuGiOhCard> GetCardsByType(string cardType)
        {
            return GetAllCards().Where(card => card.Type == cardType).ToList();
        }
        
        public List<YuGiOhCard> GetCardsByRarity(string rarity)
        {
            return GetAllCards().Where(card => card.Rarity == rarity).ToList();
        }
        
        public void MarkCardAsSeen(string cardId)
        {
            if (cardCollection.TryGetValue(cardId, out CollectionCard card))
            {
                card.isNew = false;
            }
            
            SaveCollection();
        }
        
        public List<CollectionCard> GetNewCards()
        {
            return cardCollection.Values.Where(card => card.isNew).ToList();
        }
        
        public void ClearCollection()
        {
            cardCollection.Clear();
            SaveCollection();
            Debug.Log("Collection cleared");
        }
        
        private void SaveCollection()
        {
            // Convert collection to JSON
            CollectionData data = new CollectionData
            {
                cards = cardCollection.Values.ToList()
            };
            
            string json = JsonUtility.ToJson(data);
            
            // Save to PlayerPrefs
            PlayerPrefs.SetString("CardCollection", json);
            PlayerPrefs.Save();
            
            Debug.Log("Collection saved");
        }
        
        private void LoadCollection()
        {
            // Load from PlayerPrefs
            if (PlayerPrefs.HasKey("CardCollection"))
            {
                string json = PlayerPrefs.GetString("CardCollection");
                CollectionData data = JsonUtility.FromJson<CollectionData>(json);
                
                if (data != null && data.cards != null)
                {
                    cardCollection.Clear();
                    foreach (var card in data.cards)
                    {
                        cardCollection[card.cardId] = card;
                    }
                    
                    Debug.Log($"Loaded collection with {cardCollection.Count} unique cards");
                }
            }
            else
            {
                Debug.Log("No saved collection found");
            }
        }
        
        [Serializable]
        private class CollectionData
        {
            public List<CollectionCard> cards;
        }
    }
} 