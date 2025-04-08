using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    public class CardGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private YuGiOhAPIManager apiManager;
        [SerializeField] private float generationInterval = 0.5f;
        [SerializeField] private int maxCardsInHand = 7;
        [SerializeField] private int initialHandSize = 5;
        
        [Header("Card Type Weights")]
        [SerializeField] private float normalMonsterWeight = 1.0f;
        [SerializeField] private float effectMonsterWeight = 1.2f;
        [SerializeField] private float ritualMonsterWeight = 1.5f;
        [SerializeField] private float fusionMonsterWeight = 1.8f;
        [SerializeField] private float synchroMonsterWeight = 2.0f;
        [SerializeField] private float xyzMonsterWeight = 2.2f;
        [SerializeField] private float linkMonsterWeight = 2.5f;
        [SerializeField] private float spellCardWeight = 1.0f;
        [SerializeField] private float trapCardWeight = 1.0f;
        
        [Header("Card Rarity Weights")]
        [SerializeField] private float commonWeight = 1.0f;
        [SerializeField] private float rareWeight = 0.7f;
        [SerializeField] private float superRareWeight = 0.4f;
        [SerializeField] private float ultraRareWeight = 0.2f;
        [SerializeField] private float secretRareWeight = 0.1f;
        
        [Header("Card Power Scaling")]
        [SerializeField] private float basePowerMultiplier = 1.0f;
        [SerializeField] private float powerScalingFactor = 0.1f;
        [SerializeField] private int maxBasePower = 5000;
        
        private List<YuGiOhCard> availableCards = new List<YuGiOhCard>();
        private List<YuGiOhCard> currentHand = new List<YuGiOhCard>();
        private Dictionary<string, float> cardTypeWeights = new Dictionary<string, float>();
        private Dictionary<string, float> rarityWeights = new Dictionary<string, float>();
        private bool isGenerating = false;
        private Coroutine generationCoroutine;
        
        public delegate void CardGeneratedHandler(YuGiOhCard card);
        public event CardGeneratedHandler OnCardGenerated;
        
        public delegate void HandUpdatedHandler(List<YuGiOhCard> hand);
        public event HandUpdatedHandler OnHandUpdated;
        
        private void Awake()
        {
            if (apiManager == null)
            {
                apiManager = FindObjectOfType<YuGiOhAPIManager>();
                if (apiManager == null)
                {
                    Debug.LogError("CardGenerator: YuGiOhAPIManager not found!");
                }
            }
            
            InitializeWeights();
        }
        
        private void InitializeWeights()
        {
            // Initialize card type weights
            cardTypeWeights["Normal Monster"] = normalMonsterWeight;
            cardTypeWeights["Effect Monster"] = effectMonsterWeight;
            cardTypeWeights["Ritual Monster"] = ritualMonsterWeight;
            cardTypeWeights["Fusion Monster"] = fusionMonsterWeight;
            cardTypeWeights["Synchro Monster"] = synchroMonsterWeight;
            cardTypeWeights["XYZ Monster"] = xyzMonsterWeight;
            cardTypeWeights["Link Monster"] = linkMonsterWeight;
            cardTypeWeights["Spell Card"] = spellCardWeight;
            cardTypeWeights["Trap Card"] = trapCardWeight;
            
            // Initialize rarity weights
            rarityWeights["Common"] = commonWeight;
            rarityWeights["Rare"] = rareWeight;
            rarityWeights["Super Rare"] = superRareWeight;
            rarityWeights["Ultra Rare"] = ultraRareWeight;
            rarityWeights["Secret Rare"] = secretRareWeight;
        }
        
        private void Start()
        {
            if (apiManager != null)
            {
                StartCoroutine(WaitForAPILoad());
            }
        }
        
        private IEnumerator WaitForAPILoad()
        {
            while (apiManager.IsLoading())
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            availableCards = apiManager.GetAllCards();
            Debug.Log($"Loaded {availableCards.Count} cards for generation");
            
            // Generate initial hand
            for (int i = 0; i < initialHandSize; i++)
            {
                GenerateCard();
            }
            
            // Start continuous generation
            StartGeneration();
        }
        
        public void StartGeneration()
        {
            if (isGenerating)
            {
                return;
            }
            
            isGenerating = true;
            generationCoroutine = StartCoroutine(GenerationLoop());
        }
        
        public void StopGeneration()
        {
            if (!isGenerating)
            {
                return;
            }
            
            isGenerating = false;
            if (generationCoroutine != null)
            {
                StopCoroutine(generationCoroutine);
                generationCoroutine = null;
            }
        }
        
        private IEnumerator GenerationLoop()
        {
            while (isGenerating)
            {
                if (currentHand.Count < maxCardsInHand)
                {
                    GenerateCard();
                }
                
                yield return new WaitForSeconds(generationInterval);
            }
        }
        
        public void GenerateCard()
        {
            if (availableCards.Count == 0)
            {
                Debug.LogWarning("No cards available for generation");
                return;
            }
            
            // Select card type based on weights
            string selectedType = SelectCardType();
            
            // Filter cards by type
            var typeCards = availableCards.Where(card => card.type == selectedType).ToList();
            if (typeCards.Count == 0)
            {
                Debug.LogWarning($"No cards of type {selectedType} available");
                return;
            }
            
            // Select card based on rarity weights
            YuGiOhCard selectedCard = SelectCardByRarity(typeCards);
            
            // Add to hand
            currentHand.Add(selectedCard);
            
            // Notify listeners
            OnCardGenerated?.Invoke(selectedCard);
            OnHandUpdated?.Invoke(currentHand);
        }
        
        private string SelectCardType()
        {
            float totalWeight = cardTypeWeights.Values.Sum();
            float random = Random.Range(0f, totalWeight);
            
            float cumulativeWeight = 0f;
            foreach (var weight in cardTypeWeights)
            {
                cumulativeWeight += weight.Value;
                if (random <= cumulativeWeight)
                {
                    return weight.Key;
                }
            }
            
            return cardTypeWeights.Keys.First();
        }
        
        private YuGiOhCard SelectCardByRarity(List<YuGiOhCard> cards)
        {
            // Group cards by rarity
            var cardsByRarity = cards.GroupBy(card => GetCardRarity(card))
                                   .ToDictionary(g => g.Key, g => g.ToList());
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var rarity in cardsByRarity.Keys)
            {
                if (rarityWeights.ContainsKey(rarity))
                {
                    totalWeight += rarityWeights[rarity] * cardsByRarity[rarity].Count;
                }
            }
            
            // Select rarity based on weights
            float random = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            string selectedRarity = "";
            
            foreach (var rarity in cardsByRarity.Keys)
            {
                if (rarityWeights.ContainsKey(rarity))
                {
                    cumulativeWeight += rarityWeights[rarity] * cardsByRarity[rarity].Count;
                    if (random <= cumulativeWeight)
                    {
                        selectedRarity = rarity;
                        break;
                    }
                }
            }
            
            // Select random card from selected rarity
            var rarityCards = cardsByRarity[selectedRarity];
            return rarityCards[Random.Range(0, rarityCards.Count)];
        }
        
        private string GetCardRarity(YuGiOhCard card)
        {
            // This is a simplified rarity determination
            // In a real implementation, you would use the card's set information
            if (card.card_sets == null || card.card_sets.Count == 0)
            {
                return "Common";
            }
            
            var rarities = card.card_sets.Select(set => set.set_rarity).Distinct().ToList();
            if (rarities.Contains("Secret Rare"))
            {
                return "Secret Rare";
            }
            else if (rarities.Contains("Ultra Rare"))
            {
                return "Ultra Rare";
            }
            else if (rarities.Contains("Super Rare"))
            {
                return "Super Rare";
            }
            else if (rarities.Contains("Rare"))
            {
                return "Rare";
            }
            else
            {
                return "Common";
            }
        }
        
        public float CalculateCardPower(YuGiOhCard card)
        {
            float basePower = 0f;
            
            // Extract ATK and DEF from card description
            string desc = card.desc.ToLower();
            if (desc.Contains("atk") && desc.Contains("/"))
            {
                string[] parts = desc.Split(new[] { "atk", "/" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string atkStr = new string(parts[0].Where(c => char.IsDigit(c)).ToArray());
                    if (int.TryParse(atkStr, out int atk))
                    {
                        basePower = atk;
                    }
                }
            }
            
            // Apply rarity multiplier
            float rarityMultiplier = 1.0f;
            string rarity = GetCardRarity(card);
            switch (rarity)
            {
                case "Secret Rare":
                    rarityMultiplier = 1.5f;
                    break;
                case "Ultra Rare":
                    rarityMultiplier = 1.3f;
                    break;
                case "Super Rare":
                    rarityMultiplier = 1.2f;
                    break;
                case "Rare":
                    rarityMultiplier = 1.1f;
                    break;
            }
            
            // Apply type multiplier
            float typeMultiplier = 1.0f;
            if (cardTypeWeights.ContainsKey(card.type))
            {
                typeMultiplier = cardTypeWeights[card.type];
            }
            
            // Calculate final power
            float power = basePower * basePowerMultiplier * rarityMultiplier * typeMultiplier;
            
            // Apply scaling factor
            power = Mathf.Min(power * (1f + powerScalingFactor), maxBasePower);
            
            return power;
        }
        
        public void RemoveCardFromHand(YuGiOhCard card)
        {
            if (currentHand.Contains(card))
            {
                currentHand.Remove(card);
                OnHandUpdated?.Invoke(currentHand);
            }
        }
        
        public List<YuGiOhCard> GetCurrentHand()
        {
            return new List<YuGiOhCard>(currentHand);
        }
        
        public void ClearHand()
        {
            currentHand.Clear();
            OnHandUpdated?.Invoke(currentHand);
        }
    }
} 