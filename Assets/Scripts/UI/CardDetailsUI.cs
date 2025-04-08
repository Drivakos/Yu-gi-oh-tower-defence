using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardDetailsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardFrame;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private TextMeshProUGUI cardStatsText;
        [SerializeField] private TextMeshProUGUI cardRarityText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button playButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float scaleInDuration = 0.3f;
        [SerializeField] private float scaleOutDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Mobile Settings")]
        [SerializeField] private bool enableSwipeToClose = true;
        [SerializeField] private float swipeThreshold = 100f;
        
        [Header("Color Settings")]
        [SerializeField] private Color normalMonsterColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color effectMonsterColor = new Color(0.8f, 0.6f, 0.2f);
        [SerializeField] private Color ritualMonsterColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color fusionMonsterColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color synchroMonsterColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color xyzMonsterColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color linkMonsterColor = new Color(0.2f, 0.6f, 0.8f);
        [SerializeField] private Color spellColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color trapColor = new Color(0.8f, 0.2f, 0.2f);
        
        [SerializeField] private Color commonColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color rareColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color superRareColor = new Color(0.8f, 0.6f, 0.2f);
        [SerializeField] private Color ultraRareColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color secretRareColor = new Color(0.8f, 0.8f, 0.2f);
        
        private YuGiOhCard currentCard;
        private Vector2 touchStartPosition;
        private bool isShowing;
        private Coroutine animationCoroutine;
        
        private void Awake()
        {
            // Initialize UI elements
            if (detailsPanel == null)
            {
                detailsPanel = transform.Find("DetailsPanel")?.gameObject;
                if (detailsPanel == null)
                {
                    Debug.LogError("Details panel not found!");
                }
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = detailsPanel?.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = detailsPanel?.AddComponent<CanvasGroup>();
                }
            }
            
            // Set up button listeners
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideDetails);
            }
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayCard);
            }
            
            // Hide panel initially
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isShowing || !enableSwipeToClose)
            {
                return;
            }
            
            // Handle swipe to close
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 swipeDelta = touch.position - touchStartPosition;
                    
                    if (swipeDelta.magnitude > swipeThreshold)
                    {
                        HideDetails();
                    }
                }
            }
        }
        
        public void ShowDetails(YuGiOhCard card)
        {
            if (card == null)
            {
                return;
            }
            
            currentCard = card;
            isShowing = true;
            
            // Update UI with card data
            UpdateCardDetails();
            
            // Show panel with animation
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(true);
                
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                
                animationCoroutine = StartCoroutine(AnimateDetails(true));
            }
        }
        
        public void HideDetails()
        {
            if (!isShowing)
            {
                return;
            }
            
            isShowing = false;
            
            // Hide panel with animation
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            animationCoroutine = StartCoroutine(AnimateDetails(false));
        }
        
        private void UpdateCardDetails()
        {
            if (currentCard == null)
            {
                return;
            }
            
            // Update card name
            if (cardNameText != null)
            {
                cardNameText.text = currentCard.Name;
            }
            
            // Update card type
            if (cardTypeText != null)
            {
                cardTypeText.text = currentCard.Type;
            }
            
            // Update card description
            if (cardDescriptionText != null)
            {
                cardDescriptionText.text = currentCard.Description;
            }
            
            // Update card stats
            if (cardStatsText != null)
            {
                string statsText = "";
                
                if (currentCard.IsMonster())
                {
                    statsText = $"ATK: {currentCard.Attack} / DEF: {currentCard.Defense}";
                    
                    if (currentCard.Level > 0)
                    {
                        statsText += $"\nLevel: {currentCard.Level}";
                    }
                }
                else
                {
                    statsText = $"Type: {currentCard.Race}";
                }
                
                cardStatsText.text = statsText;
            }
            
            // Update card rarity
            if (cardRarityText != null)
            {
                string rarityText = "Common";
                
                if (currentCard.Rarity == "Rare")
                {
                    rarityText = "Rare";
                }
                else if (currentCard.Rarity == "Super Rare")
                {
                    rarityText = "Super Rare";
                }
                else if (currentCard.Rarity == "Ultra Rare")
                {
                    rarityText = "Ultra Rare";
                }
                else if (currentCard.Rarity == "Secret Rare")
                {
                    rarityText = "Secret Rare";
                }
                
                cardRarityText.text = rarityText;
            }
            
            // Update card frame color
            if (cardFrame != null)
            {
                cardFrame.color = GetCardTypeColor(currentCard.Type);
            }
            
            // Update card image
            if (cardImage != null && !string.IsNullOrEmpty(currentCard.ImageUrl))
            {
                StartCoroutine(LoadCardImage(currentCard.ImageUrl));
            }
            
            // Enable/disable play button based on card type
            if (playButton != null)
            {
                playButton.interactable = currentCard.IsMonster();
            }
        }
        
        private Color GetCardTypeColor(string cardType)
        {
            if (string.IsNullOrEmpty(cardType))
            {
                return normalMonsterColor;
            }
            
            if (cardType.Contains("Normal"))
            {
                return normalMonsterColor;
            }
            else if (cardType.Contains("Effect"))
            {
                return effectMonsterColor;
            }
            else if (cardType.Contains("Ritual"))
            {
                return ritualMonsterColor;
            }
            else if (cardType.Contains("Fusion"))
            {
                return fusionMonsterColor;
            }
            else if (cardType.Contains("Synchro"))
            {
                return synchroMonsterColor;
            }
            else if (cardType.Contains("XYZ"))
            {
                return xyzMonsterColor;
            }
            else if (cardType.Contains("Link"))
            {
                return linkMonsterColor;
            }
            else if (cardType.Contains("Spell"))
            {
                return spellColor;
            }
            else if (cardType.Contains("Trap"))
            {
                return trapColor;
            }
            
            return normalMonsterColor;
        }
        
        private IEnumerator LoadCardImage(string imageUrl)
        {
            if (cardImage == null || string.IsNullOrEmpty(imageUrl))
            {
                yield break;
            }
            
            // Show loading state
            cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
                    
                    // Create sprite from texture
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100.0f
                    );
                    
                    // Apply sprite to image
                    cardImage.sprite = sprite;
                    cardImage.color = Color.white;
                    
                    // Adjust image size to fit the frame
                    AdjustImageSize();
                }
                else
                {
                    Debug.LogError($"Failed to load card image: {www.error}");
                    cardImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                }
            }
        }
        
        private void AdjustImageSize()
        {
            if (cardImage == null || cardImage.sprite == null)
            {
                return;
            }
            
            // Get the aspect ratio of the sprite
            float spriteAspect = (float)cardImage.sprite.texture.width / cardImage.sprite.texture.height;
            
            // Get the rect transform of the image
            RectTransform rectTransform = cardImage.rectTransform;
            
            // Calculate the size that maintains the aspect ratio
            float width = rectTransform.rect.width;
            float height = width / spriteAspect;
            
            // If the height is too tall, scale based on height instead
            if (height > rectTransform.rect.height)
            {
                height = rectTransform.rect.height;
                width = height * spriteAspect;
            }
            
            // Set the size
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        
        private IEnumerator AnimateDetails(bool show)
        {
            if (detailsPanel == null || canvasGroup == null)
            {
                yield break;
            }
            
            float fadeDuration = show ? fadeInDuration : fadeOutDuration;
            float scaleDuration = show ? scaleInDuration : scaleOutDuration;
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = show ? 1f : 0f;
            Vector3 startScale = detailsPanel.transform.localScale;
            Vector3 targetScale = show ? Vector3.one : Vector3.zero;
            float elapsedTime = 0f;
            
            while (elapsedTime < Mathf.Max(fadeDuration, scaleDuration))
            {
                elapsedTime += Time.deltaTime;
                
                // Update alpha
                if (elapsedTime <= fadeDuration)
                {
                    float fadeT = elapsedTime / fadeDuration;
                    float fadeCurveValue = fadeCurve.Evaluate(fadeT);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, fadeCurveValue);
                }
                
                // Update scale
                if (elapsedTime <= scaleDuration)
                {
                    float scaleT = elapsedTime / scaleDuration;
                    float scaleCurveValue = scaleCurve.Evaluate(scaleT);
                    detailsPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, scaleCurveValue);
                }
                
                yield return null;
            }
            
            // Ensure final values
            canvasGroup.alpha = targetAlpha;
            detailsPanel.transform.localScale = targetScale;
            
            if (!show)
            {
                detailsPanel.SetActive(false);
            }
        }
        
        private void PlayCard()
        {
            if (currentCard == null || !currentCard.IsMonster())
            {
                return;
            }
            
            // Find the CardPlacementManager
            CardPlacementManager placementManager = FindObjectOfType<CardPlacementManager>();
            if (placementManager == null)
            {
                Debug.LogError("CardPlacementManager not found!");
                return;
            }
            
            // Start card placement
            placementManager.StartPlacement(currentCard);
            
            // Hide details panel
            HideDetails();
        }
    }
} 