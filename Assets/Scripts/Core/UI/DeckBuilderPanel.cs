using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Core.UI
{
    public class DeckBuilderPanel : MonoBehaviour
    {
        [Header("Search")]
        [SerializeField] private CardSearchPanel searchPanel;
        [SerializeField] private CardPreviewPanel previewPanel;
        
        [Header("Deck Info")]
        [SerializeField] private TMP_Text deckNameText;
        [SerializeField] private TMP_Text cardCountText;
        [SerializeField] private TMP_Text monsterCountText;
        [SerializeField] private TMP_Text spellCountText;
        [SerializeField] private TMP_Text trapCountText;
        
        [Header("Deck List")]
        [SerializeField] private Transform deckListContainer;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private ScrollRect deckScrollRect;
        
        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button closeButton;
        
        private const int MIN_DECK_SIZE = 40;
        private const int MAX_DECK_SIZE = 60;
        private const int MAX_CARD_COPIES = 3;
        
        private string currentDeckName;
        private List<CardData> currentDeck = new List<CardData>();
        private List<CardButton> deckCardButtons = new List<CardButton>();
        
        public event Action<DeckData> OnDeckSaved;
        
        private void Awake()
        {
            InitializeButtons();
            InitializeSearchPanel();
            UpdateDeckInfo();
        }
        
        private void InitializeButtons()
        {
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(SaveDeck);
            }
            
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearDeck);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
        }
        
        private void InitializeSearchPanel()
        {
            if (searchPanel != null)
            {
                searchPanel.OnCardSelected += OnCardSelectedFromSearch;
            }
        }
        
        private void OnDestroy()
        {
            if (searchPanel != null)
            {
                searchPanel.OnCardSelected -= OnCardSelectedFromSearch;
            }
            
            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(SaveDeck);
            }
            
            if (clearButton != null)
            {
                clearButton.onClick.RemoveListener(ClearDeck);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(ClosePanel);
            }
        }
        
        public void Show(string deckName = null, DeckData existingDeck = null)
        {
            currentDeckName = deckName ?? "New Deck";
            deckNameText.text = currentDeckName;
            
            if (existingDeck != null)
            {
                LoadDeck(existingDeck);
            }
            else
            {
                ClearDeck();
            }
            
            gameObject.SetActive(true);
        }
        
        private void LoadDeck(DeckData deck)
        {
            ClearDeck();
            currentDeckName = deck.DeckName;
            deckNameText.text = currentDeckName;
            
            foreach (var card in deck.Cards)
            {
                AddCardToDeck(card);
            }
            
            UpdateDeckInfo();
        }
        
        private void OnCardSelectedFromSearch(CardData card)
        {
            if (CanAddCardToDeck(card))
            {
                AddCardToDeck(card);
                UpdateDeckInfo();
            }
            else
            {
                // TODO: Show error message (e.g., "Maximum copies reached" or "Deck is full")
            }
        }
        
        private bool CanAddCardToDeck(CardData card)
        {
            // Check deck size limit
            if (currentDeck.Count >= MAX_DECK_SIZE)
            {
                return false;
            }
            
            // Check card copy limit
            int cardCount = currentDeck.Count(c => c.CardId == card.CardId);
            return cardCount < MAX_CARD_COPIES;
        }
        
        private void AddCardToDeck(CardData card)
        {
            currentDeck.Add(card);
            
            // Create and initialize card button
            GameObject buttonObj = Instantiate(cardButtonPrefab, deckListContainer);
            CardButton cardButton = buttonObj.GetComponent<CardButton>();
            
            if (cardButton != null)
            {
                cardButton.Initialize(card, OnDeckCardSelected);
                deckCardButtons.Add(cardButton);
            }
        }
        
        private void OnDeckCardSelected(CardData card)
        {
            if (previewPanel != null)
            {
                previewPanel.Show(card, null, RemoveCardFromDeck);
            }
        }
        
        private void RemoveCardFromDeck(CardData card)
        {
            int index = currentDeck.FindIndex(c => c.CardId == card.CardId);
            if (index >= 0)
            {
                currentDeck.RemoveAt(index);
                
                // Remove and destroy the corresponding button
                CardButton button = deckCardButtons[index];
                deckCardButtons.RemoveAt(index);
                Destroy(button.gameObject);
                
                UpdateDeckInfo();
            }
        }
        
        private void UpdateDeckInfo()
        {
            int totalCards = currentDeck.Count;
            int monsters = currentDeck.Count(c => c is MonsterCardData);
            int spells = currentDeck.Count(c => c is SpellCardData);
            int traps = currentDeck.Count(c => c is TrapCardData);
            
            cardCountText.text = $"Total: {totalCards}/{MAX_DECK_SIZE}";
            monsterCountText.text = $"Monsters: {monsters}";
            spellCountText.text = $"Spells: {spells}";
            trapCountText.text = $"Traps: {traps}";
            
            // Update save button state
            if (saveButton != null)
            {
                saveButton.interactable = totalCards >= MIN_DECK_SIZE && totalCards <= MAX_DECK_SIZE;
            }
        }
        
        private void SaveDeck()
        {
            if (currentDeck.Count < MIN_DECK_SIZE)
            {
                // TODO: Show error message
                return;
            }
            
            DeckData deckData = new DeckData
            {
                DeckName = currentDeckName,
                Cards = new List<CardData>(currentDeck)
            };
            
            OnDeckSaved?.Invoke(deckData);
            ClosePanel();
        }
        
        private void ClearDeck()
        {
            currentDeck.Clear();
            
            foreach (var button in deckCardButtons)
            {
                Destroy(button.gameObject);
            }
            
            deckCardButtons.Clear();
            UpdateDeckInfo();
        }
        
        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
} 