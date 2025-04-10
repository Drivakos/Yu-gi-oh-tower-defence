using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core.Shop
{
    public class CardShop : MonoBehaviour
    {
        [Header("Shop Settings")]
        [SerializeField] private int maxShopSlots = 5;
        [SerializeField] private float refreshCost = 2f;
        [SerializeField] private List<CardPack> availablePacks;
        
        [Header("References")]
        [SerializeField] private Transform shopSlotsParent;
        [SerializeField] private GameObject cardSlotPrefab;
        
        private List<CardSlot> shopSlots = new List<CardSlot>();
        private GameManager gameManager;
        
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
            
            InitializeShop();
        }
        
        private void InitializeShop()
        {
            // Create shop slots
            for (int i = 0; i < maxShopSlots; i++)
            {
                GameObject slotObj = Instantiate(cardSlotPrefab, shopSlotsParent);
                CardSlot slot = slotObj.GetComponent<CardSlot>();
                if (slot != null)
                {
                    shopSlots.Add(slot);
                }
            }
            
            // Fill initial shop
            RefreshShop();
        }
        
        public void RefreshShop()
        {
            if (gameManager.SpendGold(refreshCost))
            {
                foreach (CardSlot slot in shopSlots)
                {
                    // Select a random pack
                    CardPack selectedPack = availablePacks[Random.Range(0, availablePacks.Count)];
                    
                    // Get a random card from the pack
                    YuGiOhCard card = selectedPack.GetRandomCard();
                    
                    // Set the card in the slot
                    slot.SetCard(card);
                }
            }
            else
            {
                Debug.Log("Not enough gold to refresh shop!");
            }
        }
        
        public bool PurchaseCard(CardSlot slot)
        {
            if (slot == null || slot.Card == null)
            {
                return false;
            }
            
            if (gameManager.SpendGold(slot.Card.Cost))
            {
                // Add card to player's deck
                gameManager.AddCardToDeck(slot.Card);
                
                // Clear the slot
                slot.ClearCard();
                
                return true;
            }
            
            return false;
        }
    }
    
    [System.Serializable]
    public class CardPack
    {
        public string packName;
        public List<YuGiOhCard> cards;
        public float rarityWeight = 1f;
        
        public YuGiOhCard GetRandomCard()
        {
            if (cards.Count == 0)
            {
                return null;
            }
            
            // Simple random selection for now
            // Could be expanded to use rarity weights
            return cards[Random.Range(0, cards.Count)];
        }
    }
} 