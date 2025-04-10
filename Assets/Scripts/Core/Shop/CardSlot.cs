using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Shop
{
    public class CardSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardCostText;
        [SerializeField] private Button purchaseButton;
        
        private YuGiOhCard currentCard;
        private CardShop cardShop;
        
        public YuGiOhCard Card => currentCard;
        
        private void Awake()
        {
            cardShop = GetComponentInParent<CardShop>();
            if (cardShop == null)
            {
                Debug.LogError("CardShop not found in parent!");
                return;
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            }
        }
        
        public void SetCard(YuGiOhCard card)
        {
            currentCard = card;
            
            if (card != null)
            {
                // Update UI
                if (cardImage != null)
                {
                    cardImage.sprite = card.CardImage;
                    cardImage.gameObject.SetActive(true);
                }
                
                if (cardNameText != null)
                {
                    cardNameText.text = card.CardName;
                }
                
                if (cardCostText != null)
                {
                    cardCostText.text = card.Cost.ToString();
                }
                
                if (purchaseButton != null)
                {
                    purchaseButton.interactable = true;
                }
            }
            else
            {
                ClearCard();
            }
        }
        
        public void ClearCard()
        {
            currentCard = null;
            
            // Clear UI
            if (cardImage != null)
            {
                cardImage.gameObject.SetActive(false);
            }
            
            if (cardNameText != null)
            {
                cardNameText.text = string.Empty;
            }
            
            if (cardCostText != null)
            {
                cardCostText.text = string.Empty;
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.interactable = false;
            }
        }
        
        private void OnPurchaseButtonClicked()
        {
            if (currentCard != null)
            {
                cardShop.PurchaseCard(this);
            }
        }
        
        private void OnDestroy()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
            }
        }
    }
} 