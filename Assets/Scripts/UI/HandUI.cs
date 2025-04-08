using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.UI
{
    public class HandUI : MonoBehaviour
    {
        [Header("Hand Settings")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private float cardSpacing = 10f;
        [SerializeField] private float cardArcRadius = 500f;
        [SerializeField] private float cardArcAngle = 60f;
        [SerializeField] private float cardHeight = 100f;
        
        [Header("Animation Settings")]
        [SerializeField] private float cardDrawDuration = 0.3f;
        [SerializeField] private float cardDiscardDuration = 0.2f;
        [SerializeField] private float cardHoverHeight = 50f;
        [SerializeField] private float cardHoverDuration = 0.2f;
        
        private CardGenerator cardGenerator;
        private List<CardUI> cardsInHand = new List<CardUI>();
        private Dictionary<YuGiOhCard, CardUI> cardToUI = new Dictionary<YuGiOhCard, CardUI>();
        private bool isAnimating = false;
        
        public delegate void CardPlayedHandler(YuGiOhCard card);
        public event CardPlayedHandler OnCardPlayed;
        
        private void Awake()
        {
            cardGenerator = FindObjectOfType<CardGenerator>();
            if (cardGenerator == null)
            {
                Debug.LogError("HandUI: CardGenerator not found!");
            }
        }
        
        private void OnEnable()
        {
            if (cardGenerator != null)
            {
                cardGenerator.OnCardGenerated += OnCardGenerated;
                cardGenerator.OnHandUpdated += OnHandUpdated;
            }
        }
        
        private void OnDisable()
        {
            if (cardGenerator != null)
            {
                cardGenerator.OnCardGenerated -= OnCardGenerated;
                cardGenerator.OnHandUpdated -= OnHandUpdated;
            }
        }
        
        private void OnCardGenerated(YuGiOhCard card)
        {
            StartCoroutine(AddCardToHand(card));
        }
        
        private void OnHandUpdated(List<YuGiOhCard> hand)
        {
            // Remove cards that are no longer in hand
            List<CardUI> cardsToRemove = new List<CardUI>();
            foreach (var cardUI in cardsInHand)
            {
                YuGiOhCard card = cardUI.GetCardData();
                if (!hand.Contains(card))
                {
                    cardsToRemove.Add(cardUI);
                }
            }
            
            foreach (var cardUI in cardsToRemove)
            {
                StartCoroutine(RemoveCardFromHand(cardUI));
            }
            
            // Update card positions
            UpdateCardPositions();
        }
        
        private IEnumerator AddCardToHand(YuGiOhCard card)
        {
            if (cardPrefab == null || handContainer == null)
            {
                yield break;
            }
            
            // Create card UI
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            
            if (cardUI != null)
            {
                // Set card data
                cardUI.SetCardData(card);
                
                // Add event listeners
                cardUI.OnCardSelected += OnCardSelected;
                
                // Add to lists
                cardsInHand.Add(cardUI);
                cardToUI[card] = cardUI;
                
                // Animate card entry
                yield return StartCoroutine(AnimateCardEntry(cardUI));
                
                // Update positions
                UpdateCardPositions();
            }
        }
        
        private IEnumerator RemoveCardFromHand(CardUI cardUI)
        {
            if (cardUI == null)
            {
                yield break;
            }
            
            YuGiOhCard card = cardUI.GetCardData();
            
            // Remove from lists
            cardsInHand.Remove(cardUI);
            cardToUI.Remove(card);
            
            // Animate card exit
            yield return StartCoroutine(AnimateCardExit(cardUI));
            
            // Destroy card object
            Destroy(cardUI.gameObject);
            
            // Update positions
            UpdateCardPositions();
        }
        
        private IEnumerator AnimateCardEntry(CardUI cardUI)
        {
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            Vector3 startPosition = rectTransform.localPosition + new Vector3(0, -500f, 0);
            Vector3 targetPosition = CalculateCardPosition(cardsInHand.Count - 1);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < cardDrawDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardDrawDuration;
                
                rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                
                yield return null;
            }
            
            rectTransform.localPosition = targetPosition;
        }
        
        private IEnumerator AnimateCardExit(CardUI cardUI)
        {
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            Vector3 startPosition = rectTransform.localPosition;
            Vector3 targetPosition = startPosition + new Vector3(0, -500f, 0);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < cardDiscardDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardDiscardDuration;
                
                rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                
                yield return null;
            }
            
            rectTransform.localPosition = targetPosition;
        }
        
        private void UpdateCardPositions()
        {
            if (isAnimating)
            {
                return;
            }
            
            StartCoroutine(UpdateCardPositionsCoroutine());
        }
        
        private IEnumerator UpdateCardPositionsCoroutine()
        {
            isAnimating = true;
            
            float elapsedTime = 0f;
            Dictionary<CardUI, Vector3> startPositions = new Dictionary<CardUI, Vector3>();
            Dictionary<CardUI, Vector3> targetPositions = new Dictionary<CardUI, Vector3>();
            
            // Calculate start and target positions
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                CardUI cardUI = cardsInHand[i];
                RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                
                startPositions[cardUI] = rectTransform.localPosition;
                targetPositions[cardUI] = CalculateCardPosition(i);
            }
            
            // Animate to new positions
            while (elapsedTime < cardHoverDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardHoverDuration;
                
                foreach (var cardUI in cardsInHand)
                {
                    if (startPositions.ContainsKey(cardUI) && targetPositions.ContainsKey(cardUI))
                    {
                        RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                        rectTransform.localPosition = Vector3.Lerp(startPositions[cardUI], targetPositions[cardUI], t);
                    }
                }
                
                yield return null;
            }
            
            // Ensure final positions
            foreach (var cardUI in cardsInHand)
            {
                if (targetPositions.ContainsKey(cardUI))
                {
                    RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                    rectTransform.localPosition = targetPositions[cardUI];
                }
            }
            
            isAnimating = false;
        }
        
        private Vector3 CalculateCardPosition(int index)
        {
            int cardCount = cardsInHand.Count;
            if (cardCount <= 1)
            {
                return Vector3.zero;
            }
            
            // Calculate angle for this card
            float angleStep = cardArcAngle / (cardCount - 1);
            float startAngle = -cardArcAngle / 2;
            float angle = startAngle + (index * angleStep);
            
            // Convert angle to radians
            float angleRad = angle * Mathf.Deg2Rad;
            
            // Calculate position on arc
            float x = Mathf.Sin(angleRad) * cardArcRadius;
            float y = (1 - Mathf.Cos(angleRad)) * cardArcRadius;
            
            return new Vector3(x, y, 0);
        }
        
        private void OnCardSelected(YuGiOhCard card)
        {
            OnCardPlayed?.Invoke(card);
            
            // Remove card from hand
            if (cardToUI.TryGetValue(card, out CardUI cardUI))
            {
                StartCoroutine(RemoveCardFromHand(cardUI));
            }
        }
        
        public void ClearHand()
        {
            foreach (var cardUI in cardsInHand)
            {
                Destroy(cardUI.gameObject);
            }
            
            cardsInHand.Clear();
            cardToUI.Clear();
        }
        
        public List<YuGiOhCard> GetCardsInHand()
        {
            List<YuGiOhCard> cards = new List<YuGiOhCard>();
            foreach (var cardUI in cardsInHand)
            {
                cards.Add(cardUI.GetCardData());
            }
            return cards;
        }
    }
} 