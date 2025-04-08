using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardRevealUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject revealPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Animation Settings")]
        [SerializeField] private float revealDelay = 0.5f;
        [SerializeField] private float cardRevealDuration = 0.5f;
        [SerializeField] private float cardScaleMultiplier = 1.2f;
        [SerializeField] private float cardRotationAngle = 360f;
        [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private float glowDuration = 1.0f;
        
        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 20f;
        [SerializeField] private int maxCardsPerRow = 3;
        [SerializeField] private float cardSize = 200f;
        
        private List<YuGiOhCard> cardsToReveal = new List<YuGiOhCard>();
        private List<GameObject> revealedCards = new List<GameObject>();
        private bool isRevealing;
        private Coroutine revealCoroutine;
        
        private void Awake()
        {
            // Initialize UI elements
            if (revealPanel == null)
            {
                revealPanel = transform.Find("RevealPanel")?.gameObject;
                if (revealPanel == null)
                {
                    Debug.LogError("Reveal panel not found!");
                }
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = revealPanel?.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = revealPanel?.AddComponent<CanvasGroup>();
                }
            }
            
            if (cardContainer == null)
            {
                cardContainer = revealPanel?.transform.Find("CardContainer");
                if (cardContainer == null)
                {
                    Debug.LogError("Card container not found!");
                }
            }
            
            // Set up button listeners
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideReveal);
                closeButton.interactable = false;
            }
            
            // Hide panel initially
            if (revealPanel != null)
            {
                revealPanel.SetActive(false);
            }
        }
        
        public void ShowReveal(List<YuGiOhCard> cards, string title = "Cards Revealed", string description = "")
        {
            if (cards == null || cards.Count == 0)
            {
                return;
            }
            
            // Store cards to reveal
            cardsToReveal = new List<YuGiOhCard>(cards);
            
            // Update title and description
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
            
            // Clear previous cards
            ClearRevealedCards();
            
            // Show panel
            if (revealPanel != null)
            {
                revealPanel.SetActive(true);
                canvasGroup.alpha = 1f;
            }
            
            // Start reveal animation
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
            }
            
            revealCoroutine = StartCoroutine(RevealCards());
        }
        
        public void HideReveal()
        {
            if (isRevealing)
            {
                return;
            }
            
            // Hide panel with fade
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
            }
            
            revealCoroutine = StartCoroutine(FadeOut());
        }
        
        private void ClearRevealedCards()
        {
            foreach (GameObject cardObj in revealedCards)
            {
                if (cardObj != null)
                {
                    Destroy(cardObj);
                }
            }
            
            revealedCards.Clear();
        }
        
        private IEnumerator RevealCards()
        {
            isRevealing = true;
            
            // Disable close button during reveal
            if (closeButton != null)
            {
                closeButton.interactable = false;
            }
            
            // Calculate layout
            int cardCount = cardsToReveal.Count;
            int rows = Mathf.CeilToInt((float)cardCount / maxCardsPerRow);
            float totalWidth = Mathf.Min(cardCount, maxCardsPerRow) * (cardSize + cardSpacing) - cardSpacing;
            float totalHeight = rows * (cardSize + cardSpacing) - cardSpacing;
            
            // Center the container
            if (cardContainer != null)
            {
                RectTransform containerRect = cardContainer.GetComponent<RectTransform>();
                if (containerRect != null)
                {
                    containerRect.sizeDelta = new Vector2(totalWidth, totalHeight);
                }
            }
            
            // Reveal each card with delay
            for (int i = 0; i < cardCount; i++)
            {
                YuGiOhCard card = cardsToReveal[i];
                
                // Calculate position
                int row = i / maxCardsPerRow;
                int col = i % maxCardsPerRow;
                float xPos = col * (cardSize + cardSpacing) - totalWidth / 2 + cardSize / 2;
                float yPos = -row * (cardSize + cardSpacing) + totalHeight / 2 - cardSize / 2;
                
                // Create card object
                GameObject cardObj = CreateCardObject(card, new Vector3(xPos, yPos, 0));
                revealedCards.Add(cardObj);
                
                // Animate card reveal
                yield return StartCoroutine(AnimateCardReveal(cardObj, card));
                
                // Wait before revealing next card
                yield return new WaitForSeconds(revealDelay);
            }
            
            // Enable close button after all cards are revealed
            if (closeButton != null)
            {
                closeButton.interactable = true;
            }
            
            isRevealing = false;
        }
        
        private GameObject CreateCardObject(YuGiOhCard card, Vector3 position)
        {
            if (cardPrefab == null || cardContainer == null)
            {
                return null;
            }
            
            // Instantiate card
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            cardObj.transform.localPosition = position;
            
            // Set up card UI
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCardData(card);
            }
            
            return cardObj;
        }
        
        private IEnumerator AnimateCardReveal(GameObject cardObj, YuGiOhCard card)
        {
            if (cardObj == null)
            {
                yield break;
            }
            
            // Get components
            RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
            CanvasGroup cardCanvasGroup = cardObj.GetComponent<CanvasGroup>();
            Image cardImage = cardObj.GetComponent<Image>();
            
            if (rectTransform == null || cardCanvasGroup == null || cardImage == null)
            {
                yield break;
            }
            
            // Set initial state
            rectTransform.localScale = Vector3.zero;
            cardCanvasGroup.alpha = 0f;
            
            // Determine glow color based on rarity
            Color glowColor = GetRarityGlowColor(card.rarity);
            
            // Animate reveal
            float elapsedTime = 0f;
            
            while (elapsedTime < cardRevealDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardRevealDuration;
                float curveValue = revealCurve.Evaluate(t);
                
                // Scale up
                float scale = Mathf.Lerp(0f, 1f, curveValue);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                
                // Rotate
                float rotation = Mathf.Lerp(0f, cardRotationAngle, curveValue);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
                
                // Fade in
                cardCanvasGroup.alpha = curveValue;
                
                // Glow effect
                float glow = Mathf.Sin(t * Mathf.PI) * glowIntensity;
                cardImage.color = Color.Lerp(Color.white, glowColor, glow * curveValue);
                
                yield return null;
            }
            
            // Ensure final state
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            cardCanvasGroup.alpha = 1f;
            cardImage.color = Color.white;
            
            // Add subtle glow based on rarity
            cardImage.color = Color.Lerp(Color.white, glowColor, 0.2f);
        }
        
        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                yield break;
            }
            
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < 0.3f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / 0.3f;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            
            // Hide panel
            if (revealPanel != null)
            {
                revealPanel.SetActive(false);
            }
            
            // Clear cards
            ClearRevealedCards();
        }
        
        private Color GetRarityGlowColor(string rarity)
        {
            if (string.IsNullOrEmpty(rarity))
            {
                return Color.white;
            }
            
            if (rarity == "Common")
            {
                return new Color(0.8f, 0.8f, 0.8f);
            }
            else if (rarity == "Rare")
            {
                return new Color(0.2f, 0.4f, 0.8f);
            }
            else if (rarity == "Super Rare")
            {
                return new Color(0.8f, 0.6f, 0.2f);
            }
            else if (rarity == "Ultra Rare")
            {
                return new Color(0.8f, 0.2f, 0.2f);
            }
            else if (rarity == "Secret Rare")
            {
                return new Color(0.8f, 0.8f, 0.2f);
            }
            
            return Color.white;
        }
    }
} 