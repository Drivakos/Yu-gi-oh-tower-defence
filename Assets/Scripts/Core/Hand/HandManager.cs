using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core.Hand
{
    public class HandManager : MonoBehaviour
    {
        [Header("Hand Settings")]
        [SerializeField] private int maxHandSize = 5;
        [SerializeField] private float cardSpacing = 1f;
        [SerializeField] private float cardRotation = 5f;
        
        [Header("References")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject cardPrefab;
        
        private List<HandCard> handCards = new List<HandCard>();
        private GameManager gameManager;
        
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
            
            // Subscribe to events
            gameManager.OnCardAddedToHand += HandleCardAddedToHand;
            gameManager.OnCardPlayed += HandleCardPlayed;
        }
        
        private void HandleCardAddedToHand(YuGiOhCard card)
        {
            // Create new card in hand
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            HandCard handCard = cardObj.GetComponent<HandCard>();
            
            if (handCard != null)
            {
                handCard.Initialize(card);
                handCards.Add(handCard);
                UpdateHandLayout();
            }
        }
        
        private void HandleCardPlayed(YuGiOhCard card)
        {
            // Find and remove the card from hand
            HandCard handCard = handCards.Find(c => c.Card == card);
            if (handCard != null)
            {
                handCards.Remove(handCard);
                Destroy(handCard.gameObject);
                UpdateHandLayout();
            }
        }
        
        private void UpdateHandLayout()
        {
            if (handCards.Count == 0) return;
            
            float totalWidth = (handCards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2f;
            
            for (int i = 0; i < handCards.Count; i++)
            {
                HandCard card = handCards[i];
                float xPos = startX + (i * cardSpacing);
                
                // Calculate rotation based on position
                float rotation = -cardRotation + ((float)i / (handCards.Count - 1)) * (cardRotation * 2);
                
                // Set position and rotation
                card.transform.localPosition = new Vector3(xPos, 0, 0);
                card.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                
                // Set sorting order (cards in center should appear on top)
                int sortingOrder = Mathf.Abs(i - (handCards.Count / 2));
                card.SetSortingOrder(sortingOrder);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnCardAddedToHand -= HandleCardAddedToHand;
                gameManager.OnCardPlayed -= HandleCardPlayed;
            }
        }
    }
} 