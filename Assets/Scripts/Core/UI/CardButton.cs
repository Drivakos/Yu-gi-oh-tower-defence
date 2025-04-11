using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;
using System;
using System.Threading.Tasks;

namespace YuGiOhTowerDefense.Core.UI
{
    public class CardButton : MonoBehaviour
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
        
        [Header("Spell/Trap Info")]
        [SerializeField] private GameObject spellTrapPanel;
        [SerializeField] private TMP_Text spellTrapTypeText;
        
        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;
        
        [Header("Preview")]
        [SerializeField] private CardPreviewPanel previewPanel;
        
        private CardData cardData;
        private Action<CardData> onCardSelected;
        
        public void Initialize(CardData data, Action<CardData> onSelect = null)
        {
            cardData = data;
            onCardSelected = onSelect;
            
            UpdateCardDisplay();
            LoadCardImage();
        }
        
        private async void LoadCardImage()
        {
            if (cardData == null || cardImage == null) return;
            
            loadingIndicator.SetActive(true);
            cardImage.gameObject.SetActive(false);
            
            try
            {
                Sprite sprite = await CardImageManager.Instance.LoadCardImage(cardData.CardId);
                cardImage.sprite = sprite;
                cardImage.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading card image: {e.Message}");
            }
            finally
            {
                loadingIndicator.SetActive(false);
            }
        }
        
        private void UpdateCardDisplay()
        {
            if (cardData == null) return;
            
            // Set common card info
            cardNameText.text = cardData.CardName;
            cardDescriptionText.text = cardData.Description;
            
            // Handle different card types
            if (cardData is MonsterCardData monster)
            {
                monsterStatsPanel.SetActive(true);
                spellTrapPanel.SetActive(false);
                
                cardTypeText.text = $"{monster.Attribute} / {monster.Type}";
                attackText.text = $"ATK: {monster.Attack}";
                defenseText.text = $"DEF: {monster.Defense}";
                levelText.text = $"Level: {monster.Level}";
            }
            else if (cardData is SpellCardData spell)
            {
                monsterStatsPanel.SetActive(false);
                spellTrapPanel.SetActive(true);
                
                cardTypeText.text = "Spell Card";
                spellTrapTypeText.text = spell.SpellType.ToString();
            }
            else if (cardData is TrapCardData trap)
            {
                monsterStatsPanel.SetActive(false);
                spellTrapPanel.SetActive(true);
                
                cardTypeText.text = "Trap Card";
                spellTrapTypeText.text = trap.TrapType.ToString();
            }
        }
        
        public void OnButtonClicked()
        {
            if (previewPanel != null)
            {
                previewPanel.Show(cardData, onCardSelected);
            }
            else
            {
                onCardSelected?.Invoke(cardData);
            }
        }
    }
} 