using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core.Cards;

namespace YuGiOhTowerDefense.Core.UI
{
    /// <summary>
    /// Manages the card selection interface
    /// </summary>
    public class CardSelectionUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI handCountText;
        
        [Header("Card Details")]
        [SerializeField] private GameObject cardDetailsPanel;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private Image cardImage;
        
        private List<CardButton> cardButtons = new List<CardButton>();
        private Card selectedCard;
        
        private void Awake()
        {
            if (cardDetailsPanel != null)
            {
                cardDetailsPanel.SetActive(false);
            }
        }
        
        public void Initialize(List<Card> deck)
        {
            ClearCardButtons();
            CreateCardButtons(deck);
            UpdateCounters(deck.Count, 0);
        }
        
        public void UpdateHand(List<Card> hand)
        {
            ClearCardButtons();
            CreateCardButtons(hand);
            UpdateCounters(0, hand.Count);
        }
        
        private void ClearCardButtons()
        {
            foreach (var button in cardButtons)
            {
                Destroy(button.gameObject);
            }
            cardButtons.Clear();
        }
        
        private void CreateCardButtons(List<Card> cards)
        {
            if (cardContainer == null || cardButtonPrefab == null)
            {
                Debug.LogError("Card container or button prefab not assigned!");
                return;
            }
            
            foreach (var card in cards)
            {
                GameObject buttonObj = Instantiate(cardButtonPrefab, cardContainer);
                CardButton cardButton = buttonObj.GetComponent<CardButton>();
                
                if (cardButton != null)
                {
                    cardButton.Initialize(card, OnCardSelected);
                    cardButtons.Add(cardButton);
                }
            }
        }
        
        private void UpdateCounters(int deckCount, int handCount)
        {
            if (deckCountText != null)
            {
                deckCountText.text = $"Deck: {deckCount}";
            }
            
            if (handCountText != null)
            {
                handCountText.text = $"Hand: {handCount}";
            }
        }
        
        private void OnCardSelected(Card card)
        {
            selectedCard = card;
            ShowCardDetails(card);
        }
        
        private void ShowCardDetails(Card card)
        {
            if (cardDetailsPanel == null)
            {
                return;
            }
            
            cardDetailsPanel.SetActive(true);
            
            if (cardNameText != null)
            {
                cardNameText.text = card.CardName;
            }
            
            if (cardTypeText != null)
            {
                cardTypeText.text = $"{card.CardType} - {card.Attribute}";
            }
            
            if (cardDescriptionText != null)
            {
                cardDescriptionText.text = card.Description;
            }
            
            if (cardImage != null && card.CardImage != null)
            {
                cardImage.sprite = card.CardImage;
            }
        }
        
        public void HideCardDetails()
        {
            if (cardDetailsPanel != null)
            {
                cardDetailsPanel.SetActive(false);
            }
            selectedCard = null;
        }
        
        public Card GetSelectedCard()
        {
            return selectedCard;
        }
    }
    
    /// <summary>
    /// Represents a card button in the selection UI
    /// </summary>
    public class CardButton : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private Button button;
        
        private Card card;
        private System.Action<Card> onSelected;
        
        public void Initialize(Card card, System.Action<Card> onSelected)
        {
            this.card = card;
            this.onSelected = onSelected;
            
            if (cardImage != null && card.CardImage != null)
            {
                cardImage.sprite = card.CardImage;
            }
            
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        private void OnButtonClicked()
        {
            onSelected?.Invoke(card);
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
} 