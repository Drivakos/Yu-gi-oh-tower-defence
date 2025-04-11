using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Cards
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "YuGiOh/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        [Header("Monster Cards")]
        [SerializeField] private List<MonsterCard> monsterCards = new List<MonsterCard>();
        
        [Header("Spell Cards")]
        [SerializeField] private List<SpellCard> spellCards = new List<SpellCard>();
        
        [Header("Trap Cards")]
        [SerializeField] private List<TrapCard> trapCards = new List<TrapCard>();
        
        private Dictionary<string, YuGiOhCard> cardDictionary = new Dictionary<string, YuGiOhCard>();
        
        public IReadOnlyList<MonsterCard> MonsterCards => monsterCards;
        public IReadOnlyList<SpellCard> SpellCards => spellCards;
        public IReadOnlyList<TrapCard> TrapCards => trapCards;
        
        private void OnEnable()
        {
            InitializeDatabase();
        }
        
        private void InitializeDatabase()
        {
            cardDictionary.Clear();
            
            // Add all cards to dictionary
            foreach (var card in monsterCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardId))
                {
                    cardDictionary[card.CardId] = card;
                }
            }
            
            foreach (var card in spellCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardId))
                {
                    cardDictionary[card.CardId] = card;
                }
            }
            
            foreach (var card in trapCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardId))
                {
                    cardDictionary[card.CardId] = card;
                }
            }
        }
        
        public YuGiOhCard GetCardById(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError("Card ID cannot be null or empty");
                return null;
            }
            
            if (cardDictionary.TryGetValue(cardId, out YuGiOhCard card))
            {
                return card;
            }
            
            Debug.LogWarning($"Card with ID {cardId} not found in database");
            return null;
        }
        
        public List<YuGiOhCard> GetCardsByType(CardType type)
        {
            switch (type)
            {
                case CardType.Monster:
                    return new List<YuGiOhCard>(monsterCards);
                case CardType.Spell:
                    return new List<YuGiOhCard>(spellCards);
                case CardType.Trap:
                    return new List<YuGiOhCard>(trapCards);
                default:
                    Debug.LogError($"Invalid card type: {type}");
                    return new List<YuGiOhCard>();
            }
        }
        
        public List<YuGiOhCard> GetCardsByRarity(CardRarity rarity)
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (var card in cardDictionary.Values)
            {
                if (card.Rarity == rarity)
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
        
        public List<YuGiOhCard> SearchCards(string searchTerm)
        {
            List<YuGiOhCard> result = new List<YuGiOhCard>();
            
            foreach (var card in cardDictionary.Values)
            {
                if (card.CardName.ToLower().Contains(searchTerm.ToLower()) ||
                    card.Description.ToLower().Contains(searchTerm.ToLower()))
                {
                    result.Add(card);
                }
            }
            
            return result;
        }
    }
} 