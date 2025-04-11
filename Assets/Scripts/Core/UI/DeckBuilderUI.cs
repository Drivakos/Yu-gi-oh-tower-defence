using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core.Cards;

namespace YuGiOhTowerDefense.Core.UI
{
    public class DeckBuilderUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private CardDatabase cardDatabase;
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private Transform deckListParent;
        [SerializeField] private GameObject deckButtonPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField deckNameInput;
        [SerializeField] private Button createDeckButton;
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private Button saveDeckButton;
        [SerializeField] private TMP_Text mainDeckCountText;
        [SerializeField] private TMP_Text extraDeckCountText;
        [SerializeField] private TMP_Text validationText;
        
        [Header("Filters")]
        [SerializeField] private TMP_Dropdown cardTypeFilter;
        [SerializeField] private TMP_InputField searchInput;
        
        private Deck currentDeck;
        private List<CardButton> cardButtons = new List<CardButton>();
        private List<DeckButton> deckButtons = new List<DeckButton>();
        
        private void Awake()
        {
            if (deckManager == null || cardDatabase == null)
            {
                Debug.LogError("Required references are missing!");
                enabled = false;
                return;
            }
            
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Initialize filters
            cardTypeFilter.ClearOptions();
            cardTypeFilter.AddOptions(new List<string> { "All", "Monster", "Spell", "Trap" });
            
            // Add listeners
            createDeckButton.onClick.AddListener(CreateNewDeck);
            deleteDeckButton.onClick.AddListener(DeleteCurrentDeck);
            saveDeckButton.onClick.AddListener(SaveCurrentDeck);
            deckNameInput.onValueChanged.AddListener(OnDeckNameChanged);
            cardTypeFilter.onValueChanged.AddListener(OnFilterChanged);
            searchInput.onValueChanged.AddListener(OnSearchChanged);
            
            // Initial state
            UpdateDeckList();
            UpdateCardGrid();
            UpdateDeckInfo();
        }
        
        private void CreateNewDeck()
        {
            string deckName = deckNameInput.text.Trim();
            if (deckManager.CreateNewDeck(deckName))
            {
                currentDeck = deckManager.CurrentDeck;
                UpdateDeckList();
                UpdateDeckInfo();
                ClearInputs();
            }
        }
        
        private void DeleteCurrentDeck()
        {
            if (currentDeck != null)
            {
                if (deckManager.DeleteDeck(currentDeck.DeckName))
                {
                    currentDeck = deckManager.CurrentDeck;
                    UpdateDeckList();
                    UpdateDeckInfo();
                    ClearInputs();
                }
            }
        }
        
        private void SaveCurrentDeck()
        {
            if (currentDeck != null)
            {
                deckManager.SaveAllDecks();
                UpdateValidationText();
            }
        }
        
        private void OnDeckNameChanged(string newName)
        {
            createDeckButton.interactable = deckManager.IsDeckNameValid(newName.Trim());
        }
        
        private void OnFilterChanged(int index)
        {
            UpdateCardGrid();
        }
        
        private void OnSearchChanged(string searchText)
        {
            UpdateCardGrid();
        }
        
        private void UpdateDeckList()
        {
            // Clear existing buttons
            foreach (DeckButton button in deckButtons)
            {
                Destroy(button.gameObject);
            }
            deckButtons.Clear();
            
            // Create new buttons
            foreach (string deckName in deckManager.GetDeckNames())
            {
                GameObject buttonObj = Instantiate(deckButtonPrefab, deckListParent);
                DeckButton button = buttonObj.GetComponent<DeckButton>();
                button.Initialize(deckName, () => SelectDeck(deckName));
                deckButtons.Add(button);
            }
        }
        
        private void UpdateCardGrid()
        {
            // Clear existing buttons
            foreach (CardButton button in cardButtons)
            {
                Destroy(button.gameObject);
            }
            cardButtons.Clear();
            
            // Get filtered cards
            List<CardData> filteredCards = GetFilteredCards();
            
            // Create new buttons
            foreach (CardData card in filteredCards)
            {
                GameObject buttonObj = Instantiate(cardButtonPrefab, cardGridParent);
                CardButton button = buttonObj.GetComponent<CardButton>();
                button.Initialize(card, () => AddCardToDeck(card));
                cardButtons.Add(button);
            }
        }
        
        private List<CardData> GetFilteredCards()
        {
            string searchText = searchInput.text.ToLower();
            string filterType = cardTypeFilter.options[cardTypeFilter.value].text;
            
            List<CardData> filteredCards = new List<CardData>();
            foreach (CardData card in cardDatabase.GetAllCards())
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) || 
                                   card.cardName.ToLower().Contains(searchText) ||
                                   card.description.ToLower().Contains(searchText);
                
                bool matchesType = filterType == "All" || 
                                 (filterType == "Monster" && card is MonsterCardData) ||
                                 (filterType == "Spell" && card is SpellCardData) ||
                                 (filterType == "Trap" && card is TrapCardData);
                
                if (matchesSearch && matchesType)
                {
                    filteredCards.Add(card);
                }
            }
            
            return filteredCards;
        }
        
        private void SelectDeck(string deckName)
        {
            if (deckManager.SelectDeck(deckName))
            {
                currentDeck = deckManager.CurrentDeck;
                UpdateDeckInfo();
                UpdateValidationText();
            }
        }
        
        private void AddCardToDeck(CardData card)
        {
            if (currentDeck == null) return;
            
            if (card is MonsterCardData monsterCard)
            {
                if (monsterCard.isSpecialSummon)
                {
                    currentDeck.AddCardToExtraDeck(card.cardId);
                }
                else
                {
                    currentDeck.AddCardToMainDeck(card.cardId);
                }
            }
            else
            {
                currentDeck.AddCardToMainDeck(card.cardId);
            }
            
            UpdateDeckInfo();
            UpdateValidationText();
        }
        
        private void UpdateDeckInfo()
        {
            if (currentDeck != null)
            {
                mainDeckCountText.text = $"Main Deck: {currentDeck.MainDeckCount}/60";
                extraDeckCountText.text = $"Extra Deck: {currentDeck.ExtraDeckCount}/15";
            }
            else
            {
                mainDeckCountText.text = "Main Deck: 0/60";
                extraDeckCountText.text = "Extra Deck: 0/15";
            }
        }
        
        private void UpdateValidationText()
        {
            if (currentDeck == null)
            {
                validationText.text = "No deck selected";
                validationText.color = Color.yellow;
                return;
            }
            
            if (currentDeck.IsValid())
            {
                validationText.text = "Deck is valid";
                validationText.color = Color.green;
            }
            else
            {
                validationText.text = "Deck is invalid";
                validationText.color = Color.red;
            }
        }
        
        private void ClearInputs()
        {
            deckNameInput.text = "";
            searchInput.text = "";
            cardTypeFilter.value = 0;
        }
    }
} 