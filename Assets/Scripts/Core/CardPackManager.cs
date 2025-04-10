using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.UI;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Managers
{
    [System.Serializable]
    public class CardPack
    {
        public string name;
        public string description;
        public int cost;
        public CurrencyType costType = CurrencyType.Coins;
        public int cardsPerPack = 5;
        public Sprite packImage;
        
        [System.Serializable]
        public class RarityDistribution
        {
            public string rarity;
            public float weight;
        }
        
        [System.Serializable]
        public class TypeDistribution
        {
            public string type;
            public float weight;
        }
        
        public List<RarityDistribution> rarityDistribution = new List<RarityDistribution>();
        public List<TypeDistribution> typeDistribution = new List<TypeDistribution>();
    }
    
    public class CardPackManager : MonoBehaviour
    {
        [Header("Pack Settings")]
        [SerializeField] private List<CardPack> availablePacks = new List<CardPack>();
        [SerializeField] private float guaranteedRareChance = 0.8f;
        [SerializeField] private float guaranteedSuperRareChance = 0.2f;
        
        [Header("References")]
        [SerializeField] private YuGiOhAPIManager apiManager;
        [SerializeField] private CardRevealUI cardRevealUI;
        [SerializeField] private PlayerCollection playerCollection;
        [SerializeField] private CurrencyManager currencyManager;
        
        // Card pools by type and rarity
        private Dictionary<string, List<YuGiOhCard>> cardsByType = new Dictionary<string, List<YuGiOhCard>>();
        private Dictionary<string, List<YuGiOhCard>> cardsByRarity = new Dictionary<string, List<YuGiOhCard>>();
        
        public event System.Action<bool, string> OnPackPurchaseAttempt;
        
        private void Awake()
        {
            if (apiManager == null)
            {
                apiManager = FindObjectOfType<YuGiOhAPIManager>();
                if (apiManager == null)
                {
                    Debug.LogError("YuGiOhAPIManager not found!");
                }
            }
            
            if (cardRevealUI == null)
            {
                cardRevealUI = FindObjectOfType<CardRevealUI>();
                if (cardRevealUI == null)
                {
                    Debug.LogError("CardRevealUI not found!");
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
            
            if (currencyManager == null)
            {
                currencyManager = CurrencyManager.Instance;
                if (currencyManager == null)
                {
                    Debug.LogError("CurrencyManager not found!");
                }
            }
            
            // Initialize card pools
            InitializeCardPools();
        }
        
        private void InitializeCardPools()
        {
            if (apiManager == null || !apiManager.IsInitialized)
            {
                return;
            }
            
            // Get all cards from API
            List<YuGiOhCard> allCards = apiManager.GetAllCards();
            
            // Organize cards by type
            cardsByType.Clear();
            foreach (YuGiOhCard card in allCards)
            {
                string type = card.Type;
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }
                
                if (!cardsByType.ContainsKey(type))
                {
                    cardsByType[type] = new List<YuGiOhCard>();
                }
                
                cardsByType[type].Add(card);
            }
            
            // Organize cards by rarity
            cardsByRarity.Clear();
            foreach (YuGiOhCard card in allCards)
            {
                string rarity = card.Rarity;
                if (string.IsNullOrEmpty(rarity))
                {
                    rarity = "Common";
                }
                
                if (!cardsByRarity.ContainsKey(rarity))
                {
                    cardsByRarity[rarity] = new List<YuGiOhCard>();
                }
                
                cardsByRarity[rarity].Add(card);
            }
            
            Debug.Log($"Initialized card pools: {cardsByType.Count} types, {cardsByRarity.Count} rarities");
        }
        
        public List<YuGiOhCard> GeneratePack(CardPack pack)
        {
            if (pack == null || pack.cardsPerPack <= 0)
            {
                return new List<YuGiOhCard>();
            }
            
            List<YuGiOhCard> generatedCards = new List<YuGiOhCard>();
            
            // Determine if we should guarantee a rare or super rare
            bool guaranteeRare = Random.value < guaranteedRareChance;
            bool guaranteeSuperRare = Random.value < guaranteedSuperRareChance;
            
            // Generate cards
            for (int i = 0; i < pack.cardsPerPack; i++)
            {
                // Determine rarity for this card
                string rarity;
                
                if (i == 0 && guaranteeSuperRare)
                {
                    // Guarantee a super rare or higher
                    rarity = DetermineRarity(pack, true);
                }
                else if (i == 0 && guaranteeRare)
                {
                    // Guarantee a rare or higher
                    rarity = DetermineRarity(pack, false);
                }
                else
                {
                    // Normal rarity determination
                    rarity = DetermineRarity(pack);
                }
                
                // Determine card type
                string cardType = DetermineCardType(pack);
                
                // Get available cards for this rarity and type
                List<YuGiOhCard> availableCards = GetAvailableCards(rarity, cardType);
                
                if (availableCards.Count > 0)
                {
                    // Select a random card
                    int randomIndex = Random.Range(0, availableCards.Count);
                    YuGiOhCard selectedCard = availableCards[randomIndex];
                    
                    // Add to generated cards
                    generatedCards.Add(selectedCard);
                }
            }
            
            return generatedCards;
        }
        
        private string DetermineRarity(CardPack pack, bool guaranteeSuperRare = false)
        {
            if (pack.rarityDistribution == null || pack.rarityDistribution.Count == 0)
            {
                return "Common";
            }
            
            // If guaranteeing a super rare, filter to only super rare or higher
            List<CardPack.RarityDistribution> availableRarities;
            
            if (guaranteeSuperRare)
            {
                availableRarities = pack.rarityDistribution
                    .Where(r => r.rarity == "Super Rare" || r.rarity == "Ultra Rare" || r.rarity == "Secret Rare")
                    .ToList();
                
                if (availableRarities.Count == 0)
                {
                    return "Super Rare";
                }
            }
            else
            {
                availableRarities = pack.rarityDistribution;
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var rarity in availableRarities)
            {
                totalWeight += rarity.weight;
            }
            
            // Select rarity based on weight
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var rarity in availableRarities)
            {
                currentWeight += rarity.weight;
                
                if (randomValue <= currentWeight)
                {
                    return rarity.rarity;
                }
            }
            
            // Fallback
            return "Common";
        }
        
        private string DetermineCardType(CardPack pack)
        {
            if (pack.typeDistribution == null || pack.typeDistribution.Count == 0)
            {
                return "Normal Monster";
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var type in pack.typeDistribution)
            {
                totalWeight += type.weight;
            }
            
            // Select type based on weight
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var type in pack.typeDistribution)
            {
                currentWeight += type.weight;
                
                if (randomValue <= currentWeight)
                {
                    return type.type;
                }
            }
            
            // Fallback
            return "Normal Monster";
        }
        
        private List<YuGiOhCard> GetAvailableCards(string rarity, string cardType)
        {
            List<YuGiOhCard> availableCards = new List<YuGiOhCard>();
            
            // Get cards by rarity
            if (cardsByRarity.ContainsKey(rarity))
            {
                availableCards.AddRange(cardsByRarity[rarity]);
            }
            
            // Filter by type if specified
            if (!string.IsNullOrEmpty(cardType) && cardsByType.ContainsKey(cardType))
            {
                availableCards = availableCards.Intersect(cardsByType[cardType]).ToList();
            }
            
            return availableCards;
        }
        
        public bool PurchasePack(CardPack pack)
        {
            if (pack == null)
            {
                OnPackPurchaseAttempt?.Invoke(false, "Invalid pack selected");
                return false;
            }
            
            // Check if player has enough currency
            if (currencyManager == null)
            {
                Debug.LogError("CurrencyManager not available");
                OnPackPurchaseAttempt?.Invoke(false, "Currency system unavailable");
                return false;
            }
            
            if (!currencyManager.HasEnough(pack.costType, pack.cost))
            {
                string message = $"Not enough {pack.costType} to purchase pack. Need {pack.cost}, have {currencyManager.GetCurrency(pack.costType)}";
                Debug.Log(message);
                OnPackPurchaseAttempt?.Invoke(false, message);
                return false;
            }
            
            // Spend currency
            if (!currencyManager.SpendCurrency(pack.costType, pack.cost, $"Purchase {pack.name} Pack"))
            {
                OnPackPurchaseAttempt?.Invoke(false, "Failed to process payment");
                return false;
            }
            
            // Open the pack
            OpenPack(pack);
            
            OnPackPurchaseAttempt?.Invoke(true, $"Successfully purchased {pack.name} Pack");
            return true;
        }
        
        public void OpenPack(CardPack pack)
        {
            if (pack == null)
            {
                return;
            }
            
            // Generate cards for the pack
            List<YuGiOhCard> generatedCards = GeneratePack(pack);
            
            // Add cards to player's collection
            if (playerCollection != null && generatedCards.Count > 0)
            {
                playerCollection.AddCards(generatedCards);
            }
            
            // Show reveal UI
            if (cardRevealUI != null)
            {
                cardRevealUI.ShowReveal(generatedCards, $"{pack.name} Pack", pack.description);
            }
        }
        
        public List<CardPack> GetAvailablePacks()
        {
            return availablePacks;
        }
        
        public CardPack GetPackByName(string packName)
        {
            return availablePacks.FirstOrDefault(p => p.name == packName);
        }
        
        public bool CanAffordPack(CardPack pack)
        {
            if (pack == null || currencyManager == null)
            {
                return false;
            }
            
            return currencyManager.HasEnough(pack.costType, pack.cost);
        }
    }
} 