using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;
using System;
using System.Threading.Tasks;

namespace YuGiOhTowerDefense.Core.UI
{
    public class CardPreviewPanel : MonoBehaviour
    {
        [Header("Card Display")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text cardTypeText;
        [SerializeField] private TMP_Text cardDescriptionText;
        
        [Header("Monster Stats")]
        [SerializeField] private GameObject monsterStatsPanel;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text attributeText;
        [SerializeField] private TMP_Text typeText;
        
        [Header("Spell/Trap Info")]
        [SerializeField] private GameObject spellTrapPanel;
        [SerializeField] private TMP_Text spellTrapTypeText;
        [SerializeField] private TMP_Text spellTrapIconText;
        
        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button addToDeckButton;
        [SerializeField] private Button removeFromDeckButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        private CanvasGroup canvasGroup;
        private CardData currentCard;
        private Action<CardData> onAddToDeck;
        private Action<CardData> onRemoveFromDeck;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            InitializeButtons();
            Hide();
        }
        
        private void InitializeButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            
            if (addToDeckButton != null)
            {
                addToDeckButton.onClick.AddListener(OnAddToDeckClicked);
            }
            
            if (removeFromDeckButton != null)
            {
                removeFromDeckButton.onClick.AddListener(OnRemoveFromDeckClicked);
            }
        }
        
        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
            
            if (addToDeckButton != null)
            {
                addToDeckButton.onClick.RemoveListener(OnAddToDeckClicked);
            }
            
            if (removeFromDeckButton != null)
            {
                removeFromDeckButton.onClick.RemoveListener(OnRemoveFromDeckClicked);
            }
        }
        
        public void Show(CardData card, Action<CardData> addCallback = null, Action<CardData> removeCallback = null)
        {
            currentCard = card;
            onAddToDeck = addCallback;
            onRemoveFromDeck = removeCallback;
            
            UpdateCardDisplay();
            LoadCardImage();
            
            gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
        
        public void Hide()
        {
            StartCoroutine(FadeOut());
        }
        
        private async void LoadCardImage()
        {
            if (currentCard == null || cardImage == null) return;
            
            try
            {
                Sprite sprite = await CardImageManager.Instance.LoadCardImage(currentCard.CardId);
                cardImage.sprite = sprite;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading card image: {e.Message}");
            }
        }
        
        private void UpdateCardDisplay()
        {
            if (currentCard == null) return;
            
            // Set common card info
            cardNameText.text = currentCard.CardName;
            cardDescriptionText.text = currentCard.Description;
            
            // Handle different card types
            if (currentCard is MonsterCardData monster)
            {
                monsterStatsPanel.SetActive(true);
                spellTrapPanel.SetActive(false);
                
                cardTypeText.text = "Monster Card";
                attackText.text = $"ATK: {monster.Attack}";
                defenseText.text = $"DEF: {monster.Defense}";
                levelText.text = $"Level: {monster.Level}";
                attributeText.text = $"Attribute: {monster.Attribute}";
                typeText.text = $"Type: {monster.Type}";
            }
            else if (currentCard is SpellCardData spell)
            {
                monsterStatsPanel.SetActive(false);
                spellTrapPanel.SetActive(true);
                
                cardTypeText.text = "Spell Card";
                spellTrapTypeText.text = $"Type: {spell.SpellType}";
                spellTrapIconText.text = GetSpellIcon(spell.SpellType);
            }
            else if (currentCard is TrapCardData trap)
            {
                monsterStatsPanel.SetActive(false);
                spellTrapPanel.SetActive(true);
                
                cardTypeText.text = "Trap Card";
                spellTrapTypeText.text = $"Type: {trap.TrapType}";
                spellTrapIconText.text = GetTrapIcon(trap.TrapType);
            }
            
            // Update button states
            UpdateButtonStates();
        }
        
        private void UpdateButtonStates()
        {
            if (addToDeckButton != null)
            {
                addToDeckButton.gameObject.SetActive(onAddToDeck != null);
            }
            
            if (removeFromDeckButton != null)
            {
                removeFromDeckButton.gameObject.SetActive(onRemoveFromDeck != null);
            }
        }
        
        private string GetSpellIcon(SpellType type)
        {
            return type switch
            {
                SpellType.Normal => "⚪",
                SpellType.Continuous => "🟢",
                SpellType.QuickPlay => "⚡",
                SpellType.Field => "🌍",
                SpellType.Equip => "🛡️",
                SpellType.Ritual => "✨",
                _ => "❓"
            };
        }
        
        private string GetTrapIcon(TrapType type)
        {
            return type switch
            {
                TrapType.Normal => "⚪",
                TrapType.Continuous => "🟢",
                TrapType.Counter => "🔄",
                _ => "❓"
            };
        }
        
        private void OnAddToDeckClicked()
        {
            onAddToDeck?.Invoke(currentCard);
            Hide();
        }
        
        private void OnRemoveFromDeckClicked()
        {
            onRemoveFromDeck?.Invoke(currentCard);
            Hide();
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
} 