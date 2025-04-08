using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardUI : MonoBehaviour
    {
        [Header("Card Components")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardFrame;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private TextMeshProUGUI cardPowerText;
        [SerializeField] private Image cardGlow;
        
        [Header("Card Colors")]
        [SerializeField] private Color normalMonsterColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color effectMonsterColor = new Color(0.7f, 0.7f, 1f);
        [SerializeField] private Color ritualMonsterColor = new Color(0.7f, 0.5f, 1f);
        [SerializeField] private Color fusionMonsterColor = new Color(0.5f, 0.5f, 1f);
        [SerializeField] private Color synchroMonsterColor = new Color(0.9f, 0.9f, 0.5f);
        [SerializeField] private Color xyzMonsterColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color linkMonsterColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color spellColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color trapColor = new Color(1f, 0.5f, 0.5f);
        
        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color rareColor = new Color(0.5f, 0.5f, 1f);
        [SerializeField] private Color superRareColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color ultraRareColor = new Color(1f, 0.8f, 0f);
        [SerializeField] private Color secretRareColor = new Color(1f, 0f, 1f);
        
        [Header("Animation Settings")]
        [SerializeField] private float glowPulseSpeed = 1.5f;
        [SerializeField] private float glowPulseMin = 0.5f;
        [SerializeField] private float glowPulseMax = 1.0f;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverDuration = 0.2f;
        
        [Header("Mobile Settings")]
        [SerializeField] private float longPressDuration = 0.5f;
        [SerializeField] private bool showDetailsOnLongPress = true;
        
        private YuGiOhCard cardData;
        private Coroutine glowCoroutine;
        private Coroutine hoverCoroutine;
        private float pressStartTime;
        private bool isPressed;
        
        public delegate void CardSelectedHandler(YuGiOhCard card);
        public event CardSelectedHandler OnCardSelected;
        
        private void Awake()
        {
            // Initialize UI components
            if (cardGlow != null)
            {
                cardGlow.gameObject.SetActive(false);
            }
            
            // Add touch event handlers
            AddTouchEventHandlers();
        }
        
        private void AddTouchEventHandlers()
        {
            // Add event trigger component if not present
            UnityEngine.EventSystems.EventTrigger eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Clear existing triggers
            eventTrigger.triggers.Clear();
            
            // Add pointer down trigger
            UnityEngine.EventSystems.EventTrigger.Entry pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnPointerDown(); });
            eventTrigger.triggers.Add(pointerDown);
            
            // Add pointer up trigger
            UnityEngine.EventSystems.EventTrigger.Entry pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnPointerUp(); });
            eventTrigger.triggers.Add(pointerUp);
            
            // Add pointer exit trigger
            UnityEngine.EventSystems.EventTrigger.Entry pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnPointerExit(); });
            eventTrigger.triggers.Add(pointerExit);
        }
        
        public void SetCardData(YuGiOhCard card)
        {
            cardData = card;
            UpdateCardUI();
        }
        
        public YuGiOhCard GetCardData()
        {
            return cardData;
        }
        
        private void UpdateCardUI()
        {
            if (cardData == null)
            {
                return;
            }
            
            // Update card name
            if (cardNameText != null)
            {
                cardNameText.text = cardData.name;
            }
            
            // Update card type
            if (cardTypeText != null)
            {
                cardTypeText.text = cardData.type;
                
                // Set type color
                Color typeColor = GetTypeColor(cardData.type);
                cardTypeText.color = typeColor;
            }
            
            // Update card description
            if (cardDescriptionText != null)
            {
                cardDescriptionText.text = cardData.desc;
            }
            
            // Update card power
            if (cardPowerText != null)
            {
                if (cardData.IsMonster())
                {
                    cardPowerText.text = $"ATK: {cardData.attack} / DEF: {cardData.defense}";
                }
                else
                {
                    cardPowerText.text = $"Power: {cardData.GetPower():F0}";
                }
            }
            
            // Set card frame color based on type
            if (cardFrame != null)
            {
                cardFrame.color = GetTypeColor(cardData.type);
            }
            
            // Set card glow color based on rarity
            if (cardGlow != null)
            {
                cardGlow.color = GetRarityColor(cardData.rarity);
            }
            
            // Load card image
            LoadCardImage();
        }
        
        private void LoadCardImage()
        {
            if (cardImage == null || string.IsNullOrEmpty(cardData.imageUrl))
            {
                return;
            }
            
            // Use small image URL for mobile
            string imageUrl = !string.IsNullOrEmpty(cardData.imageUrlSmall) ? 
                cardData.imageUrlSmall : cardData.imageUrl;
                
            // Start loading the image
            StartCoroutine(LoadImageCoroutine(imageUrl));
        }
        
        private IEnumerator LoadImageCoroutine(string imageUrl)
        {
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
                    cardImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogError($"Failed to load card image: {www.error}");
                }
            }
        }
        
        private Color GetTypeColor(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return Color.white;
            }
            
            type = type.ToLower();
            
            if (type.Contains("normal monster"))
            {
                return normalMonsterColor;
            }
            else if (type.Contains("effect monster"))
            {
                return effectMonsterColor;
            }
            else if (type.Contains("ritual monster"))
            {
                return ritualMonsterColor;
            }
            else if (type.Contains("fusion monster"))
            {
                return fusionMonsterColor;
            }
            else if (type.Contains("synchro monster"))
            {
                return synchroMonsterColor;
            }
            else if (type.Contains("xyz monster"))
            {
                return xyzMonsterColor;
            }
            else if (type.Contains("link monster"))
            {
                return linkMonsterColor;
            }
            else if (type.Contains("spell"))
            {
                return spellColor;
            }
            else if (type.Contains("trap"))
            {
                return trapColor;
            }
            
            return Color.white;
        }
        
        private Color GetRarityColor(string rarity)
        {
            if (string.IsNullOrEmpty(rarity))
            {
                return commonColor;
            }
            
            rarity = rarity.ToLower();
            
            if (rarity.Contains("common"))
            {
                return commonColor;
            }
            else if (rarity.Contains("rare") && !rarity.Contains("super") && !rarity.Contains("ultra") && !rarity.Contains("secret"))
            {
                return rareColor;
            }
            else if (rarity.Contains("super rare"))
            {
                return superRareColor;
            }
            else if (rarity.Contains("ultra rare"))
            {
                return ultraRareColor;
            }
            else if (rarity.Contains("secret rare"))
            {
                return secretRareColor;
            }
            
            return commonColor;
        }
        
        private void OnPointerDown()
        {
            isPressed = true;
            pressStartTime = Time.time;
            
            // Start hover animation
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverAnimation(true));
        }
        
        private void OnPointerUp()
        {
            isPressed = false;
            
            // Check if this was a long press
            if (Time.time - pressStartTime >= longPressDuration)
            {
                if (showDetailsOnLongPress)
                {
                    ShowCardDetails();
                }
            }
            else
            {
                // This was a tap, select the card
                OnCardSelected();
            }
            
            // End hover animation
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverAnimation(false));
        }
        
        private void OnPointerExit()
        {
            isPressed = false;
            
            // End hover animation
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverAnimation(false));
        }
        
        private IEnumerator HoverAnimation(bool hover)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 startScale = rectTransform.localScale;
            Vector3 targetScale = hover ? Vector3.one * hoverScale : Vector3.one;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < hoverDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / hoverDuration;
                
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
        }
        
        private void OnCardSelected()
        {
            if (cardData != null)
            {
                OnCardSelected?.Invoke(cardData);
            }
        }
        
        private void ShowCardDetails()
        {
            // This would open a detailed card view
            // Implementation depends on your UI structure
            Debug.Log($"Showing details for card: {cardData.name}");
            
            // Example: Find a CardDetailsUI component and show it
            CardDetailsUI detailsUI = FindObjectOfType<CardDetailsUI>();
            if (detailsUI != null)
            {
                detailsUI.ShowCardDetails(cardData);
            }
        }
        
        public void StartGlowEffect()
        {
            if (cardGlow == null)
            {
                return;
            }
            
            cardGlow.gameObject.SetActive(true);
            
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            
            glowCoroutine = StartCoroutine(GlowPulse());
        }
        
        public void StopGlowEffect()
        {
            if (cardGlow != null)
            {
                cardGlow.gameObject.SetActive(false);
            }
            
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }
        }
        
        private IEnumerator GlowPulse()
        {
            while (true)
            {
                float alpha = Mathf.Lerp(glowPulseMin, glowPulseMax, (Mathf.Sin(Time.time * glowPulseSpeed) + 1) * 0.5f);
                Color color = cardGlow.color;
                color.a = alpha;
                cardGlow.color = color;
                
                yield return null;
            }
        }
        
        private void OnDestroy()
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
        }
    }
} 