using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.UI
{
    public class MobileHandUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect handScrollRect;
        [SerializeField] private RectTransform handContent;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private float cardWidth = 120f;
        [SerializeField] private float cardSpacing = 10f;
        
        [Header("Animation Settings")]
        [SerializeField] private float cardDrawDuration = 0.3f;
        [SerializeField] private float cardDiscardDuration = 0.2f;
        [SerializeField] private float cardSelectScale = 1.2f;
        [SerializeField] private float cardSelectDuration = 0.2f;
        
        [Header("Mobile Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float swipeSpeed = 500f;
        [SerializeField] private bool useSwipeNavigation = true;
        
        private CardGenerator cardGenerator;
        private List<CardUI> cardsInHand = new List<CardUI>();
        private Dictionary<YuGiOhCard, CardUI> cardToUI = new Dictionary<YuGiOhCard, CardUI>();
        private CardUI selectedCard;
        private Vector2 touchStart;
        private bool isDragging;
        
        public delegate void CardPlayedHandler(YuGiOhCard card);
        public event CardPlayedHandler OnCardPlayed;
        
        private void Awake()
        {
            cardGenerator = FindObjectOfType<CardGenerator>();
            if (cardGenerator == null)
            {
                Debug.LogError("MobileHandUI: CardGenerator not found!");
            }
            
            // Initialize scroll rect
            if (handScrollRect != null)
            {
                handScrollRect.horizontal = true;
                handScrollRect.vertical = false;
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
        
        private void Update()
        {
            if (!useSwipeNavigation || handScrollRect == null)
            {
                return;
            }
            
            // Handle touch input for card selection
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStart = touch.position;
                        isDragging = false;
                        break;
                        
                    case TouchPhase.Moved:
                        if (Vector2.Distance(touch.position, touchStart) > swipeThreshold)
                        {
                            isDragging = true;
                        }
                        break;
                        
                    case TouchPhase.Ended:
                        if (!isDragging)
                        {
                            // Check if we tapped on a card
                            CheckCardSelection(touch.position);
                        }
                        isDragging = false;
                        break;
                }
            }
        }
        
        private void CheckCardSelection(Vector2 touchPosition)
        {
            foreach (var cardUI in cardsInHand)
            {
                RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPosition))
                {
                    OnCardSelected(cardUI.GetCardData());
                    break;
                }
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
            
            // Update layout
            UpdateHandLayout();
        }
        
        private IEnumerator AddCardToHand(YuGiOhCard card)
        {
            if (cardPrefab == null || handContent == null)
            {
                yield break;
            }
            
            // Create card UI
            GameObject cardObj = Instantiate(cardPrefab, handContent);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            
            if (cardUI != null)
            {
                // Set card data
                cardUI.SetCardData(card);
                
                // Add to lists
                cardsInHand.Add(cardUI);
                cardToUI[card] = cardUI;
                
                // Animate card entry
                yield return StartCoroutine(AnimateCardEntry(cardUI));
                
                // Update layout
                UpdateHandLayout();
                
                // Scroll to the new card
                ScrollToCard(cardUI);
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
            
            // Update layout
            UpdateHandLayout();
        }
        
        private IEnumerator AnimateCardEntry(CardUI cardUI)
        {
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < cardDrawDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardDrawDuration;
                
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                
                yield return null;
            }
            
            rectTransform.localScale = Vector3.one;
        }
        
        private IEnumerator AnimateCardExit(CardUI cardUI)
        {
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            Vector3 startScale = rectTransform.localScale;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < cardDiscardDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardDiscardDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                yield return null;
            }
            
            rectTransform.localScale = Vector3.zero;
        }
        
        private void UpdateHandLayout()
        {
            if (handContent == null)
            {
                return;
            }
            
            // Set content width based on number of cards
            float contentWidth = (cardsInHand.Count * (cardWidth + cardSpacing)) - cardSpacing;
            handContent.sizeDelta = new Vector2(contentWidth, handContent.sizeDelta.y);
            
            // Position cards
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                CardUI cardUI = cardsInHand[i];
                RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                
                float xPos = (i * (cardWidth + cardSpacing)) + (cardWidth * 0.5f);
                rectTransform.anchoredPosition = new Vector2(xPos, 0);
            }
        }
        
        private void ScrollToCard(CardUI cardUI)
        {
            if (handScrollRect == null)
            {
                return;
            }
            
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            float normalizedPosition = rectTransform.anchoredPosition.x / (handContent.sizeDelta.x - handScrollRect.viewport.rect.width);
            handScrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
        }
        
        private void OnCardSelected(YuGiOhCard card)
        {
            if (selectedCard != null)
            {
                // Deselect previous card
                StartCoroutine(AnimateCardSelect(selectedCard, false));
            }
            
            if (cardToUI.TryGetValue(card, out CardUI cardUI))
            {
                selectedCard = cardUI;
                StartCoroutine(AnimateCardSelect(cardUI, true));
                OnCardPlayed?.Invoke(card);
            }
        }
        
        private IEnumerator AnimateCardSelect(CardUI cardUI, bool select)
        {
            RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
            Vector3 startScale = rectTransform.localScale;
            Vector3 targetScale = select ? Vector3.one * cardSelectScale : Vector3.one;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < cardSelectDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardSelectDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
            
            if (!select)
            {
                selectedCard = null;
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
            selectedCard = null;
            
            UpdateHandLayout();
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