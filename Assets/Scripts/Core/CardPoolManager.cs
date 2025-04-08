using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Core
{
    public class CardPoolManager : MonoBehaviour
    {
        [System.Serializable]
        public class BossReward
        {
            public string bossId;
            public float cardDropChance = 0.5f;
            public int minCards = 1;
            public int maxCards = 3;
            public float minRarityChance = 0.3f;
            public List<string> guaranteedCards = new List<string>();
        }
        
        [Header("Card Pools")]
        [SerializeField] private List<string> playerCardPool = new List<string>();
        [SerializeField] private List<string> enemyCardPool = new List<string>();
        [SerializeField] private List<string> bossCardPool = new List<string>();
        
        [Header("Boss Rewards")]
        [SerializeField] private List<BossReward> bossRewards = new List<BossReward>();
        [SerializeField] private float baseBossCardDropChance = 0.3f;
        [SerializeField] private float bossCardRarityBonus = 0.2f;
        
        [Header("Generation Settings")]
        [SerializeField] private float minCardPower = 100f;
        [SerializeField] private float maxCardPower = 1000f;
        [SerializeField] private AnimationCurve powerDistribution = AnimationCurve.Linear(0, 0, 1, 1);
        
        private YuGiOhAPIManager apiManager;
        private Dictionary<string, YuGiOhCard> cardDatabase = new Dictionary<string, YuGiOhCard>();
        private Dictionary<string, List<YuGiOhCard>> cardPoolByType = new Dictionary<string, List<YuGiOhCard>>();
        private Dictionary<string, List<YuGiOhCard>> cardPoolByRarity = new Dictionary<string, List<YuGiOhCard>>();
        
        private void Awake()
        {
            apiManager = GetComponent<YuGiOhAPIManager>();
            if (apiManager == null)
            {
                Debug.LogError("YuGiOhAPIManager not found!");
            }
        }
        
        public void InitializeCardPools()
        {
            if (apiManager == null || !apiManager.IsInitialized)
            {
                Debug.LogError("API Manager not initialized!");
                return;
            }
            
            // Clear existing pools
            cardDatabase.Clear();
            cardPoolByType.Clear();
            cardPoolByRarity.Clear();
            
            // Load all cards from API
            foreach (var card in apiManager.GetAllCards())
            {
                cardDatabase[card.id] = card;
                
                // Skip cards that don't meet power requirements
                if (card.power < minCardPower || card.power > maxCardPower)
                {
                    continue;
                }
                
                // Add to type pool
                string cardType = GetCardType(card.type);
                if (!cardPoolByType.ContainsKey(cardType))
                {
                    cardPoolByType[cardType] = new List<YuGiOhCard>();
                }
                cardPoolByType[cardType].Add(card);
                
                // Add to rarity pool
                string rarity = GetCardRarity(card.rarity);
                if (!cardPoolByRarity.ContainsKey(rarity))
                {
                    cardPoolByRarity[rarity] = new List<YuGiOhCard>();
                }
                cardPoolByRarity[rarity].Add(card);
            }
        }
        
        public List<YuGiOhCard> GeneratePlayerCards(int count)
        {
            return GenerateCards(count, playerCardPool);
        }
        
        public List<YuGiOhCard> GenerateEnemyCards(int count)
        {
            return GenerateCards(count, enemyCardPool);
        }
        
        public List<YuGiOhCard> GenerateBossCards(int count)
        {
            return GenerateCards(count, bossCardPool);
        }
        
        public List<YuGiOhCard> GenerateBossRewards(string bossId)
        {
            List<YuGiOhCard> rewards = new List<YuGiOhCard>();
            
            // Find boss reward configuration
            var bossReward = bossRewards.FirstOrDefault(r => r.bossId == bossId);
            if (bossReward == null)
            {
                Debug.LogWarning($"No reward configuration found for boss: {bossId}");
                return rewards;
            }
            
            // Check if cards should be dropped
            if (Random.value > bossReward.cardDropChance)
            {
                return rewards;
            }
            
            // Determine number of cards to drop
            int cardCount = Random.Range(bossReward.minCards, bossReward.maxCards + 1);
            
            // Add guaranteed cards first
            foreach (var cardId in bossReward.guaranteedCards)
            {
                if (cardDatabase.TryGetValue(cardId, out YuGiOhCard card))
                {
                    rewards.Add(card);
                }
            }
            
            // Generate additional random cards
            int remainingCards = cardCount - rewards.Count;
            if (remainingCards > 0)
            {
                var randomCards = GenerateCards(remainingCards, bossCardPool);
                rewards.AddRange(randomCards);
            }
            
            return rewards;
        }
        
        private List<YuGiOhCard> GenerateCards(int count, List<string> cardPool)
        {
            List<YuGiOhCard> generatedCards = new List<YuGiOhCard>();
            
            for (int i = 0; i < count; i++)
            {
                // Select a random card from the pool
                if (cardPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, cardPool.Count);
                    string cardId = cardPool[randomIndex];
                    
                    if (cardDatabase.TryGetValue(cardId, out YuGiOhCard card))
                    {
                        generatedCards.Add(card);
                    }
                }
            }
            
            return generatedCards;
        }
        
        public void AddCardToPlayerPool(string cardId)
        {
            if (!playerCardPool.Contains(cardId))
            {
                playerCardPool.Add(cardId);
            }
        }
        
        public void AddCardToEnemyPool(string cardId)
        {
            if (!enemyCardPool.Contains(cardId))
            {
                enemyCardPool.Add(cardId);
            }
        }
        
        public void AddCardToBossPool(string cardId)
        {
            if (!bossCardPool.Contains(cardId))
            {
                bossCardPool.Add(cardId);
            }
        }
        
        private string GetCardType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return "Monster";
            }
            
            type = type.ToLower();
            
            if (type.Contains("spell"))
            {
                return "Spell";
            }
            else if (type.Contains("trap"))
            {
                return "Trap";
            }
            
            return "Monster";
        }
        
        private string GetCardRarity(string rarity)
        {
            if (string.IsNullOrEmpty(rarity))
            {
                return "Common";
            }
            
            rarity = rarity.ToLower();
            
            if (rarity.Contains("secret"))
            {
                return "Secret Rare";
            }
            else if (rarity.Contains("ultra"))
            {
                return "Ultra Rare";
            }
            else if (rarity.Contains("super"))
            {
                return "Super Rare";
            }
            else if (rarity.Contains("rare"))
            {
                return "Rare";
            }
            
            return "Common";
        }
        
        public YuGiOhCard GetCardById(string cardId)
        {
            if (cardDatabase.TryGetValue(cardId, out YuGiOhCard card))
            {
                return card;
            }
            
            return null;
        }
        
        public List<YuGiOhCard> GetCardsByType(string cardType)
        {
            if (cardPoolByType.TryGetValue(cardType, out List<YuGiOhCard> cards))
            {
                return cards;
            }
            
            return new List<YuGiOhCard>();
        }
        
        public List<YuGiOhCard> GetCardsByRarity(string rarity)
        {
            if (cardPoolByRarity.TryGetValue(rarity, out List<YuGiOhCard> cards))
            {
                return cards;
            }
            
            return new List<YuGiOhCard>();
        }
    }
} 