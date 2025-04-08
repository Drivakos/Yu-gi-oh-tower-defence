using UnityEngine;
using System.Collections.Generic;
using System;
using YuGiOhTowerDefense.UI;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.UI
{
    public class CardShop : MonoBehaviour
    {
        [Header("Shop Settings")]
        [SerializeField] private int startingGems = 100;
        [SerializeField] private int gemsPerDay = 10;
        [SerializeField] private float dailyRewardCooldown = 86400f; // 24 hours in seconds
        
        [Header("Card Packs")]
        [SerializeField] private List<CardPack> availablePacks = new List<CardPack>();
        
        [Header("References")]
        [SerializeField] private CardGenerator cardGenerator;
        [SerializeField] private ShopUI shopUI;
        
        private int currentGems;
        private float lastDailyRewardTime;
        private Dictionary<string, int> purchasedPacks = new Dictionary<string, int>();
        
        public event Action<int> OnGemsChanged;
        public event Action<CardPack> OnPackPurchased;
        public event Action<List<YuGiOhCard>> OnCardsUnlocked;
        
        private void Awake()
        {
            InitializeShop();
        }
        
        private void Start()
        {
            if (shopUI != null)
            {
                shopUI.OnPackPurchased += HandlePackPurchase;
            }
            
            // Check for daily reward
            CheckDailyReward();
        }
        
        private void OnDestroy()
        {
            if (shopUI != null)
            {
                shopUI.OnPackPurchased -= HandlePackPurchase;
            }
        }
        
        private void InitializeShop()
        {
            // Load saved gems
            currentGems = PlayerPrefs.GetInt("Gems", startingGems);
            
            // Load last daily reward time
            lastDailyRewardTime = PlayerPrefs.GetFloat("LastDailyReward", 0f);
            
            // Load purchased packs
            string purchasedPacksJson = PlayerPrefs.GetString("PurchasedPacks", "{}");
            purchasedPacks = JsonUtility.FromJson<Dictionary<string, int>>(purchasedPacksJson);
            
            if (purchasedPacks == null)
            {
                purchasedPacks = new Dictionary<string, int>();
            }
            
            // Initialize UI
            UpdateShopUI();
        }
        
        private void CheckDailyReward()
        {
            float currentTime = Time.time;
            float timeSinceLastReward = currentTime - lastDailyRewardTime;
            
            if (timeSinceLastReward >= dailyRewardCooldown)
            {
                // Available daily reward
                if (shopUI != null)
                {
                    shopUI.ShowDailyRewardAvailable();
                }
            }
            else
            {
                // Calculate time until next reward
                float timeUntilNextReward = dailyRewardCooldown - timeSinceLastReward;
                if (shopUI != null)
                {
                    shopUI.UpdateDailyRewardTimer(timeUntilNextReward);
                }
            }
        }
        
        public void ClaimDailyReward()
        {
            float currentTime = Time.time;
            float timeSinceLastReward = currentTime - lastDailyRewardTime;
            
            if (timeSinceLastReward >= dailyRewardCooldown)
            {
                // Add gems
                AddGems(gemsPerDay);
                
                // Update last reward time
                lastDailyRewardTime = currentTime;
                PlayerPrefs.SetFloat("LastDailyReward", lastDailyRewardTime);
                
                // Update UI
                if (shopUI != null)
                {
                    shopUI.ShowDailyRewardClaimed(gemsPerDay);
                    shopUI.UpdateDailyRewardTimer(dailyRewardCooldown);
                }
            }
        }
        
        private void HandlePackPurchase(CardPack pack)
        {
            if (pack == null)
            {
                return;
            }
            
            if (currentGems >= pack.cost)
            {
                // Deduct gems
                RemoveGems(pack.cost);
                
                // Unlock cards
                List<YuGiOhCard> unlockedCards = UnlockPackCards(pack);
                
                // Track purchase
                if (purchasedPacks.ContainsKey(pack.id))
                {
                    purchasedPacks[pack.id]++;
                }
                else
                {
                    purchasedPacks[pack.id] = 1;
                }
                
                // Save purchase data
                string purchasedPacksJson = JsonUtility.ToJson(purchasedPacks);
                PlayerPrefs.SetString("PurchasedPacks", purchasedPacksJson);
                
                // Notify listeners
                OnPackPurchased?.Invoke(pack);
                OnCardsUnlocked?.Invoke(unlockedCards);
                
                // Update UI
                if (shopUI != null)
                {
                    shopUI.ShowPackPurchased(pack, unlockedCards);
                }
            }
            else
            {
                // Not enough gems
                if (shopUI != null)
                {
                    shopUI.ShowInsufficientGems();
                }
            }
        }
        
        private List<YuGiOhCard> UnlockPackCards(CardPack pack)
        {
            List<YuGiOhCard> unlockedCards = new List<YuGiOhCard>();
            
            if (cardGenerator == null)
            {
                Debug.LogError("CardShop: CardGenerator not found!");
                return unlockedCards;
            }
            
            // Generate cards based on pack type
            for (int i = 0; i < pack.cardCount; i++)
            {
                YuGiOhCard card = cardGenerator.GenerateCardFromPack(pack);
                if (card != null)
                {
                    unlockedCards.Add(card);
                }
            }
            
            return unlockedCards;
        }
        
        public void AddGems(int amount)
        {
            int previousGems = currentGems;
            currentGems += amount;
            
            // Save gems
            PlayerPrefs.SetInt("Gems", currentGems);
            
            // Notify listeners
            OnGemsChanged?.Invoke(currentGems - previousGems);
            
            // Update UI
            UpdateShopUI();
        }
        
        public void RemoveGems(int amount)
        {
            int previousGems = currentGems;
            currentGems = Mathf.Max(0, currentGems - amount);
            
            // Save gems
            PlayerPrefs.SetInt("Gems", currentGems);
            
            // Notify listeners
            OnGemsChanged?.Invoke(currentGems - previousGems);
            
            // Update UI
            UpdateShopUI();
        }
        
        private void UpdateShopUI()
        {
            if (shopUI != null)
            {
                shopUI.UpdateGemsDisplay(currentGems);
                shopUI.UpdateAvailablePacks(availablePacks);
            }
        }
        
        public int GetCurrentGems()
        {
            return currentGems;
        }
        
        public List<CardPack> GetAvailablePacks()
        {
            return new List<CardPack>(availablePacks);
        }
        
        public int GetPackPurchaseCount(string packId)
        {
            if (purchasedPacks.ContainsKey(packId))
            {
                return purchasedPacks[packId];
            }
            
            return 0;
        }
    }
} 