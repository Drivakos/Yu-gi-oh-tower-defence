using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Cards
{
    [System.Serializable]
    public class Deck
    {
        [SerializeField] private string deckName;
        [SerializeField] private List<string> mainDeckCardIds = new List<string>();
        [SerializeField] private List<string> extraDeckCardIds = new List<string>();
        
        private CardDatabase cardDatabase;
        
        public string DeckName => deckName;
        public int MainDeckSize => mainDeckCardIds.Count;
        public int ExtraDeckSize => extraDeckCardIds.Count;
        
        public Deck(string name, CardDatabase database)
        {
            deckName = name;
            cardDatabase = database;
        }
        
        public bool AddCardToMainDeck(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError("Card ID cannot be null or empty");
                return false;
            }
            
            YuGiOhCard card = cardDatabase.GetCardById(cardId);
            if (card == null)
            {
                Debug.LogError($"Card with ID {cardId} not found in database");
                return false;
            }
            
            // Check if card is valid for main deck
            if (card.CardType == CardType.Monster)
            {
                MonsterCard monsterCard = card as MonsterCard;
                if (monsterCard != null && 
                    (monsterCard.MonsterType == MonsterType.Fusion ||
                     monsterCard.MonsterType == MonsterType.Synchro ||
                     monsterCard.MonsterType == MonsterType.Xyz ||
                     monsterCard.MonsterType == MonsterType.Pendulum ||
                     monsterCard.MonsterType == MonsterType.Link))
                {
                    Debug.LogError("Cannot add special summon monster to main deck");
                    return false;
                }
            }
            
            // Check deck size limit
            if (mainDeckCardIds.Count >= 60)
            {
                Debug.LogError("Main deck is full (60 cards)");
                return false;
            }
            
            // Check card limit (max 3 copies)
            int cardCount = 0;
            foreach (string id in mainDeckCardIds)
            {
                if (id == cardId)
                {
                    cardCount++;
                }
            }
            
            if (cardCount >= 3)
            {
                Debug.LogError("Cannot add more than 3 copies of the same card");
                return false;
            }
            
            mainDeckCardIds.Add(cardId);
            return true;
        }
        
        public bool AddCardToExtraDeck(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError("Card ID cannot be null or empty");
                return false;
            }
            
            YuGiOhCard card = cardDatabase.GetCardById(cardId);
            if (card == null)
            {
                Debug.LogError($"Card with ID {cardId} not found in database");
                return false;
            }
            
            // Check if card is valid for extra deck
            if (card.CardType == CardType.Monster)
            {
                MonsterCard monsterCard = card as MonsterCard;
                if (monsterCard == null ||
                    (monsterCard.MonsterType != MonsterType.Fusion &&
                     monsterCard.MonsterType != MonsterType.Synchro &&
                     monsterCard.MonsterType != MonsterType.Xyz &&
                     monsterCard.MonsterType != MonsterType.Pendulum &&
                     monsterCard.MonsterType != MonsterType.Link))
                {
                    Debug.LogError("Can only add special summon monsters to extra deck");
                    return false;
                }
            }
            else
            {
                Debug.LogError("Can only add monster cards to extra deck");
                return false;
            }
            
            // Check deck size limit
            if (extraDeckCardIds.Count >= 15)
            {
                Debug.LogError("Extra deck is full (15 cards)");
                return false;
            }
            
            extraDeckCardIds.Add(cardId);
            return true;
        }
        
        public bool RemoveCardFromMainDeck(string cardId)
        {
            return mainDeckCardIds.Remove(cardId);
        }
        
        public bool RemoveCardFromExtraDeck(string cardId)
        {
            return extraDeckCardIds.Remove(cardId);
        }
        
        public List<YuGiOhCard> GetMainDeck()
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (string cardId in mainDeckCardIds)
            {
                YuGiOhCard card = cardDatabase.GetCardById(cardId);
                if (card != null)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public List<YuGiOhCard> GetExtraDeck()
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (string cardId in extraDeckCardIds)
            {
                YuGiOhCard card = cardDatabase.GetCardById(cardId);
                if (card != null)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public bool IsValid()
        {
            // Check main deck size
            if (mainDeckCardIds.Count < 40 || mainDeckCardIds.Count > 60)
            {
                Debug.LogError("Main deck must contain 40-60 cards");
                return false;
            }
            
            // Check extra deck size
            if (extraDeckCardIds.Count > 15)
            {
                Debug.LogError("Extra deck cannot contain more than 15 cards");
                return false;
            }
            
            // Check card limits
            Dictionary<string, int> cardCounts = new Dictionary<string, int>();
            foreach (string cardId in mainDeckCardIds)
            {
                if (cardCounts.ContainsKey(cardId))
                {
                    cardCounts[cardId]++;
                }
                else
                {
                    cardCounts[cardId] = 1;
                }
                
                if (cardCounts[cardId] > 3)
                {
                    Debug.LogError($"Cannot have more than 3 copies of the same card: {cardId}");
                    return false;
                }
            }
            
            return true;
        }
        
        public void SaveDeck()
        {
            // Save deck name
            PlayerPrefs.SetString($"Deck_{deckName}_Name", deckName);
            
            // Save main deck
            PlayerPrefs.SetString($"Deck_{deckName}_Main", string.Join(",", mainDeckCardIds));
            
            // Save extra deck
            PlayerPrefs.SetString($"Deck_{deckName}_Extra", string.Join(",", extraDeckCardIds));
            
            PlayerPrefs.Save();
        }
        
        public void LoadDeck(string name)
        {
            deckName = name;
            
            // Load main deck
            string mainDeckStr = PlayerPrefs.GetString($"Deck_{deckName}_Main", "");
            if (!string.IsNullOrEmpty(mainDeckStr))
            {
                mainDeckCardIds = new List<string>(mainDeckStr.Split(','));
            }
            
            // Load extra deck
            string extraDeckStr = PlayerPrefs.GetString($"Deck_{deckName}_Extra", "");
            if (!string.IsNullOrEmpty(extraDeckStr))
            {
                extraDeckCardIds = new List<string>(extraDeckStr.Split(','));
            }
        }
    }
} 