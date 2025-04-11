using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Cards
{
    [System.Serializable]
    public class PlayerCollection
    {
        [SerializeField] private List<string> unlockedCardIds = new List<string>();
        [SerializeField] private Dictionary<string, int> cardCounts = new Dictionary<string, int>();
        
        private CardDatabase cardDatabase;
        
        public PlayerCollection(CardDatabase database)
        {
            cardDatabase = database;
        }
        
        public void UnlockCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError("Card ID cannot be null or empty");
                return;
            }
            
            if (!unlockedCardIds.Contains(cardId))
            {
                unlockedCardIds.Add(cardId);
                cardCounts[cardId] = 1;
            }
            else
            {
                cardCounts[cardId]++;
            }
        }
        
        public bool HasCard(string cardId)
        {
            return unlockedCardIds.Contains(cardId);
        }
        
        public int GetCardCount(string cardId)
        {
            if (cardCounts.TryGetValue(cardId, out int count))
            {
                return count;
            }
            return 0;
        }
        
        public List<YuGiOhCard> GetUnlockedCards()
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (string cardId in unlockedCardIds)
            {
                YuGiOhCard card = cardDatabase.GetCardById(cardId);
                if (card != null)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public List<YuGiOhCard> GetUnlockedCardsByType(CardType type)
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (string cardId in unlockedCardIds)
            {
                YuGiOhCard card = cardDatabase.GetCardById(cardId);
                if (card != null && card.CardType == type)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public List<YuGiOhCard> GetUnlockedCardsByRarity(CardRarity rarity)
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (string cardId in unlockedCardIds)
            {
                YuGiOhCard card = cardDatabase.GetCardById(cardId);
                if (card != null && card.Rarity == rarity)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public void SaveCollection()
        {
            // Save unlocked cards
            PlayerPrefs.SetString("UnlockedCards", string.Join(",", unlockedCardIds));
            
            // Save card counts
            foreach (var pair in cardCounts)
            {
                PlayerPrefs.SetInt($"CardCount_{pair.Key}", pair.Value);
            }
            
            PlayerPrefs.Save();
        }
        
        public void LoadCollection()
        {
            // Load unlocked cards
            string unlockedCardsStr = PlayerPrefs.GetString("UnlockedCards", "");
            if (!string.IsNullOrEmpty(unlockedCardsStr))
            {
                unlockedCardIds = new List<string>(unlockedCardsStr.Split(','));
            }
            
            // Load card counts
            cardCounts.Clear();
            foreach (string cardId in unlockedCardIds)
            {
                int count = PlayerPrefs.GetInt($"CardCount_{cardId}", 1);
                cardCounts[cardId] = count;
            }
        }
    }
} 