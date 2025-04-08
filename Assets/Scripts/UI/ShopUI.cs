using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Shop Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button closeShopButton;
        
        [Header("Gems Display")]
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private GameObject gemsChangeEffect;
        [SerializeField] private TextMeshProUGUI gemsChangeText;
        [SerializeField] private float gemsChangeDuration = 1.5f;
        
        [Header("Daily Reward")]
        [SerializeField] private GameObject dailyRewardPanel;
        [SerializeField] private Button claimDailyRewardButton;
        [SerializeField] private TextMeshProUGUI dailyRewardTimerText;
        [SerializeField] private GameObject dailyRewardClaimedEffect;
        [SerializeField] private TextMeshProUGUI dailyRewardAmountText;
        
        [Header("Card Packs")]
        [SerializeField] private Transform packsContainer;
        [SerializeField] private GameObject packButtonPrefab;
        [SerializeField] private float packButtonSpacing = 10f;
        
        [Header("Pack Details")]
        [SerializeField] private GameObject packDetailsPanel;
        [SerializeField] private Image packImage;
        [SerializeField] private TextMeshProUGUI packNameText;
        [SerializeField] private TextMeshProUGUI packDescriptionText;
        [SerializeField] private TextMeshProUGUI packCostText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;
        
        [Header("Card Reveal")]
        [SerializeField] private GameObject cardRevealPanel;
        [SerializeField] private Transform cardRevealContainer;
        [SerializeField] private GameObject cardRevealPrefab;
        [SerializeField] private float cardRevealDelay = 0.5f;
        [SerializeField] private Button closeRevealButton;
        
        [Header("Animations")]
        [SerializeField] private float panelFadeDuration = 0.3f;
        [SerializeField] private float cardRevealDuration = 0.5f;
        
        private CardShop cardShop;
        private CardPack selectedPack;
        private List<GameObject> packButtons = new List<GameObject>();
        private List<GameObject> revealedCards = new List<GameObject>();
        
        public event Action<CardPack> OnPackPurchased;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void Start()
        {
            if (openShopButton != null)
            {
                openShopButton.onClick.AddListener(OpenShop);
            }
            
            if (closeShopButton != null)
            {
                closeShopButton.onClick.AddListener(CloseShop);
            }
            
            if (claimDailyRewardButton != null)
            {
                claimDailyRewardButton.onClick.AddListener(ClaimDailyReward);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(PurchaseSelectedPack);
            }
            
            if (closeRevealButton != null)
            {
                closeRevealButton.onClick.AddListener(CloseCardReveal);
            }
            
            // Find CardShop
            cardShop = FindObjectOfType<CardShop>();
            if (cardShop != null)
            {
                cardShop.OnGemsChanged += HandleGemsChanged;
                cardShop.OnPackPurchased += HandlePackPurchased;
                cardShop.OnCardsUnlocked += HandleCardsUnlocked;
            }
            else
            {
                Debug.LogError("ShopUI: CardShop not found!");
            }
            
            // Initialize UI
            UpdateGemsDisplay(0);
            CloseShop();
            ClosePackDetails();
            CloseCardReveal();
        }
        
        private void OnDestroy()
        {
            if (openShopButton != null)
            {
                openShopButton.onClick.RemoveListener(OpenShop);
            }
            
            if (closeShopButton != null)
            {
                closeShopButton.onClick.RemoveListener(CloseShop);
            }
            
            if (claimDailyRewardButton != null)
            {
                claimDailyRewardButton.onClick.RemoveListener(ClaimDailyReward);
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(PurchaseSelectedPack);
            }
            
            if (closeRevealButton != null)
            {
                closeRevealButton.onClick.RemoveListener(CloseCardReveal);
            }
            
            if (cardShop != null)
            {
                cardShop.OnGemsChanged -= HandleGemsChanged;
                cardShop.OnPackPurchased -= HandlePackPurchased;
                cardShop.OnCardsUnlocked -= HandleCardsUnlocked;
            }
        }
        
        private void InitializeUI()
        {
            // Initialize panels
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
            
            if (packDetailsPanel != null)
            {
                packDetailsPanel.SetActive(false);
            }
            
            if (cardRevealPanel != null)
            {
                cardRevealPanel.SetActive(false);
            }
            
            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.SetActive(false);
            }
            
            if (gemsChangeEffect != null)
            {
                gemsChangeEffect.SetActive(false);
            }
            
            if (dailyRewardClaimedEffect != null)
            {
                dailyRewardClaimedEffect.SetActive(false);
            }
        }
        
        public void OpenShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                UpdateAvailablePacks(cardShop?.GetAvailablePacks() ?? new List<CardPack>());
            }
        }
        
        public void CloseShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
            
            ClosePackDetails();
        }
        
        private void OpenPackDetails(CardPack pack)
        {
            if (pack == null || packDetailsPanel == null)
            {
                return;
            }
            
            selectedPack = pack;
            
            // Update UI
            if (packImage != null && pack.packImage != null)
            {
                packImage.sprite = pack.packImage;
            }
            
            if (packNameText != null)
            {
                packNameText.text = pack.name;
            }
            
            if (packDescriptionText != null)
            {
                packDescriptionText.text = pack.description;
            }
            
            if (packCostText != null)
            {
                packCostText.text = $"{pack.cost} Gems";
            }
            
            if (purchaseButton != null)
            {
                bool canAfford = cardShop != null && cardShop.GetCurrentGems() >= pack.cost;
                purchaseButton.interactable = canAfford;
                
                if (purchaseButtonText != null)
                {
                    purchaseButtonText.text = canAfford ? "Purchase" : "Not Enough Gems";
                }
            }
            
            // Show panel
            packDetailsPanel.SetActive(true);
        }
        
        private void ClosePackDetails()
        {
            if (packDetailsPanel != null)
            {
                packDetailsPanel.SetActive(false);
            }
            
            selectedPack = null;
        }
        
        private void OpenCardReveal(List<YuGiOhCard> cards)
        {
            if (cards == null || cards.Count == 0 || cardRevealPanel == null || cardRevealContainer == null)
            {
                return;
            }
            
            // Clear previous cards
            foreach (GameObject card in revealedCards)
            {
                Destroy(card);
            }
            
            revealedCards.Clear();
            
            // Create new cards
            for (int i = 0; i < cards.Count; i++)
            {
                GameObject cardObject = Instantiate(cardRevealPrefab, cardRevealContainer);
                CardRevealUI cardRevealUI = cardObject.GetComponent<CardRevealUI>();
                
                if (cardRevealUI != null)
                {
                    cardRevealUI.SetCard(cards[i]);
                    cardRevealUI.SetRevealDelay(i * cardRevealDelay);
                }
                
                revealedCards.Add(cardObject);
            }
            
            // Show panel
            cardRevealPanel.SetActive(true);
        }
        
        private void CloseCardReveal()
        {
            if (cardRevealPanel != null)
            {
                cardRevealPanel.SetActive(false);
            }
            
            // Clear cards
            foreach (GameObject card in revealedCards)
            {
                Destroy(card);
            }
            
            revealedCards.Clear();
        }
        
        private void PurchaseSelectedPack()
        {
            if (selectedPack != null)
            {
                OnPackPurchased?.Invoke(selectedPack);
                ClosePackDetails();
            }
        }
        
        private void ClaimDailyReward()
        {
            if (cardShop != null)
            {
                cardShop.ClaimDailyReward();
            }
        }
        
        public void UpdateGemsDisplay(int gems)
        {
            if (gemsText != null)
            {
                gemsText.text = $"{gems} Gems";
            }
        }
        
        public void ShowGemsChange(int change)
        {
            if (gemsChangeEffect != null && gemsChangeText != null)
            {
                gemsChangeText.text = change > 0 ? $"+{change}" : change.ToString();
                gemsChangeEffect.SetActive(true);
                
                // Hide after duration
                Invoke("HideGemsChange", gemsChangeDuration);
            }
        }
        
        private void HideGemsChange()
        {
            if (gemsChangeEffect != null)
            {
                gemsChangeEffect.SetActive(false);
            }
        }
        
        public void ShowDailyRewardAvailable()
        {
            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.SetActive(true);
            }
        }
        
        public void ShowDailyRewardClaimed(int amount)
        {
            if (dailyRewardClaimedEffect != null && dailyRewardAmountText != null)
            {
                dailyRewardAmountText.text = $"+{amount}";
                dailyRewardClaimedEffect.SetActive(true);
                
                // Hide after duration
                Invoke("HideDailyRewardClaimed", gemsChangeDuration);
            }
        }
        
        private void HideDailyRewardClaimed()
        {
            if (dailyRewardClaimedEffect != null)
            {
                dailyRewardClaimedEffect.SetActive(false);
            }
        }
        
        public void UpdateDailyRewardTimer(float timeUntilNextReward)
        {
            if (dailyRewardTimerText != null)
            {
                int hours = Mathf.FloorToInt(timeUntilNextReward / 3600);
                int minutes = Mathf.FloorToInt((timeUntilNextReward % 3600) / 60);
                int seconds = Mathf.FloorToInt(timeUntilNextReward % 60);
                
                dailyRewardTimerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
            }
        }
        
        public void UpdateAvailablePacks(List<CardPack> packs)
        {
            if (packsContainer == null || packButtonPrefab == null)
            {
                return;
            }
            
            // Clear previous buttons
            foreach (GameObject button in packButtons)
            {
                Destroy(button);
            }
            
            packButtons.Clear();
            
            // Create new buttons
            for (int i = 0; i < packs.Count; i++)
            {
                CardPack pack = packs[i];
                GameObject buttonObject = Instantiate(packButtonPrefab, packsContainer);
                PackButtonUI packButton = buttonObject.GetComponent<PackButtonUI>();
                
                if (packButton != null)
                {
                    packButton.SetPack(pack);
                    packButton.OnPackSelected += OpenPackDetails;
                }
                
                packButtons.Add(buttonObject);
            }
        }
        
        public void ShowPackPurchased(CardPack pack, List<YuGiOhCard> cards)
        {
            OpenCardReveal(cards);
        }
        
        public void ShowInsufficientGems()
        {
            // You could add a visual or audio feedback here
            Debug.Log("Not enough gems to purchase this pack!");
        }
        
        private void HandleGemsChanged(int change)
        {
            UpdateGemsDisplay(cardShop?.GetCurrentGems() ?? 0);
            ShowGemsChange(change);
        }
        
        private void HandlePackPurchased(CardPack pack)
        {
            // Update UI if needed
        }
        
        private void HandleCardsUnlocked(List<YuGiOhCard> cards)
        {
            // Cards are revealed in ShowPackPurchased
        }
    }
} 