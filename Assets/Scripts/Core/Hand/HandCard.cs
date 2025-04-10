using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core.Hand
{
    public class HandCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardCostText;
        [SerializeField] private Button playButton;
        
        private YuGiOhCard card;
        private Canvas canvas;
        private GameManager gameManager;
        
        public YuGiOhCard Card => card;
        
        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            gameManager = FindObjectOfType<GameManager>();
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }
        
        public void Initialize(YuGiOhCard cardData)
        {
            card = cardData;
            
            // Update UI
            if (cardImage != null)
            {
                cardImage.sprite = card.CardImage;
            }
            
            if (cardNameText != null)
            {
                cardNameText.text = card.CardName;
            }
            
            if (cardCostText != null)
            {
                cardCostText.text = card.Cost.ToString();
            }
        }
        
        public void SetSortingOrder(int order)
        {
            if (canvas != null)
            {
                canvas.sortingOrder = order;
            }
        }
        
        private void OnPlayButtonClicked()
        {
            if (card != null && gameManager != null)
            {
                gameManager.PlayCard(card);
            }
        }
        
        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
        }
    }
} 