using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class DeckBuilderUI : MonoBehaviour
    {
        [Header("Deck Management")]
        [SerializeField] private Button createDeckButton;
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private Button saveDeckButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Dropdown deckSelector;
        [SerializeField] private TMP_InputField deckNameInput;
        [SerializeField] private TMP_InputField deckDescriptionInput;
        
        [Header("Deck Stats")]
        [SerializeField] private TextMeshProUGUI cardCountText;
        [SerializeField] private TextMeshProUGUI monsterCountText;
        [SerializeField] private TextMeshProUGUI spellCountText;
        [SerializeField] private TextMeshProUGUI trapCountText;
        [SerializeField] private Slider deckSizeSlider;
        
        [Header("Collection View")]
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private Transform collectionCardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private TMP_Dropdown cardTypeFilter;
        [SerializeField] private TMP_Dropdown cardRarityFilter;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;
        [SerializeField] private Button clearFilterButton;
        
        [Header("Deck View")]
        [SerializeField] private GameObject deckPanel;
        [SerializeField] private Transform deckCardContainer;
        [SerializeField] private TextMeshProUGUI emptyDeckText;
        
        [Header("Card Details")]
        [SerializeField] private GameObject cardDetailsPanel;
        [SerializeField] private Image cardDetailImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private TextMeshProUGUI cardRarityText;
        [SerializeField] private Button addCardButton;
        [SerializeField] private Button removeCardButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelConfirmButton;
        
        [Header("Mobile UI")]
        [SerializeField] private Button toggleCollectionButton;
        [SerializeField] private Button toggleDeckButton;
        
        // References
        private DeckManager deckManager;
        private PlayerCollection playerCollection;
        private YuGiOhAPIManager apiManager;
        
        // State
        private Deck currentDeck;
        private YuGiOhCard selectedCard;
        private List<YuGiOhCard> filteredCollection = new List<YuGiOhCard>();
        private List<CardUI> collectionCardUIItems = new List<CardUI>();
        private List<CardUI> deckCardUIItems = new List<CardUI>();
        private System.Action confirmationCallback;
        
        private void Awake()
        {
            deckManager = DeckManager.Instance;
            playerCollection = PlayerCollection.Instance;
            apiManager = FindObjectOfType<YuGiOhAPIManager>();
            
            if (deckManager == null || playerCollection == null || apiManager == null)
            {
                Debug.LogError("Missing required dependencies in DeckBuilderUI");
                gameObject.SetActive(false);
                return;
            }
            
            // Initialize UI
            SetupButtons();
            SetupDropdowns();
            
            // Hide confirmation dialog
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }
            
            // Hide card details initially
            if (cardDetailsPanel != null)
            {
                cardDetailsPanel.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            if (deckManager != null)
            {
                deckManager.OnDeckCreated += HandleDeckCreated;
                deckManager.OnDeckModified += HandleDeckModified;
                deckManager.OnDeckDeleted += HandleDeckDeleted;
            }
            
            // Refresh UI
            RefreshDeckSelector();
            RefreshCollectionView();
            
            // Select active deck if any
            if (deckManager.ActiveDeck != null)
            {
                SelectDeck(deckManager.ActiveDeck);
            }
        }
        
        private void OnDisable()
        {
            if (deckManager != null)
            {
                deckManager.OnDeckCreated -= HandleDeckCreated;
                deckManager.OnDeckModified -= HandleDeckModified;
                deckManager.OnDeckDeleted -= HandleDeckDeleted;
            }
        }
        
        private void SetupButtons()
        {
            if (createDeckButton != null)
                createDeckButton.onClick.AddListener(HandleCreateDeckClicked);
            
            if (deleteDeckButton != null)
                deleteDeckButton.onClick.AddListener(HandleDeleteDeckClicked);
            
            if (saveDeckButton != null)
                saveDeckButton.onClick.AddListener(HandleSaveDeckClicked);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(HandleCancelClicked);
            
            if (searchButton != null)
                searchButton.onClick.AddListener(ApplyFilters);
            
            if (clearFilterButton != null)
                clearFilterButton.onClick.AddListener(ClearFilters);
            
            if (addCardButton != null)
                addCardButton.onClick.AddListener(HandleAddCardClicked);
            
            if (removeCardButton != null)
                removeCardButton.onClick.AddListener(HandleRemoveCardClicked);
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            
            if (cancelConfirmButton != null)
                cancelConfirmButton.onClick.AddListener(HandleCancelConfirmClicked);
            
            if (toggleCollectionButton != null)
                toggleCollectionButton.onClick.AddListener(() => TogglePanel(collectionPanel, true));
            
            if (toggleDeckButton != null)
                toggleDeckButton.onClick.AddListener(() => TogglePanel(deckPanel, true));
        }
        
        private void SetupDropdowns()
        {
            if (deckSelector != null)
            {
                deckSelector.onValueChanged.AddListener(HandleDeckSelectionChanged);
            }
            
            if (cardTypeFilter != null)
            {
                cardTypeFilter.ClearOptions();
                
                List<string> typeOptions = new List<string> { "All Types" };
                typeOptions.AddRange(new List<string> 
                { 
                    "Normal Monster", 
                    "Effect Monster", 
                    "Ritual Monster", 
                    "Fusion Monster",
                    "Synchro Monster",
                    "XYZ Monster",
                    "Pendulum Monster",
                    "Link Monster",
                    "Spell Card",
                    "Trap Card"
                });
                
                cardTypeFilter.AddOptions(typeOptions);
                cardTypeFilter.onValueChanged.AddListener(_ => ApplyFilters());
            }
            
            if (cardRarityFilter != null)
            {
                cardRarityFilter.ClearOptions();
                
                List<string> rarityOptions = new List<string> { "All Rarities" };
                rarityOptions.AddRange(new List<string>
                {
                    "Common",
                    "Rare",
                    "Super Rare",
                    "Ultra Rare",
                    "Secret Rare"
                });
                
                cardRarityFilter.AddOptions(rarityOptions);
                cardRarityFilter.onValueChanged.AddListener(_ => ApplyFilters());
            }
        }
        
        private void RefreshDeckSelector()
        {
            if (deckSelector == null || deckManager == null)
                return;
            
            deckSelector.ClearOptions();
            
            List<Deck> decks = deckManager.GetAllDecks();
            List<string> deckNames = decks.Select(d => d.name).ToList();
            
            if (deckNames.Count == 0)
            {
                deckNames.Add("No Decks Available");
                deleteDeckButton.interactable = false;
            }
            else
            {
                deleteDeckButton.interactable = true;
            }
            
            deckSelector.AddOptions(deckNames);
            
            // Select the active deck if there is one
            if (deckManager.ActiveDeck != null)
            {
                int index = deckNames.IndexOf(deckManager.ActiveDeck.name);
                if (index >= 0)
                {
                    deckSelector.value = index;
                }
            }
        }
        
        private void HandleDeckSelectionChanged(int index)
        {
            if (deckManager == null)
                return;
            
            List<Deck> decks = deckManager.GetAllDecks();
            if (index < 0 || index >= decks.Count)
                return;
            
            SelectDeck(decks[index]);
        }
        
        private void SelectDeck(Deck deck)
        {
            currentDeck = deck;
            
            // Update UI
            if (deck != null)
            {
                deckNameInput.text = deck.name;
                deckDescriptionInput.text = deck.description;
                UpdateDeckStats();
                RefreshDeckView();
            }
            else
            {
                deckNameInput.text = "";
                deckDescriptionInput.text = "";
                ClearDeckView();
            }
            
            // Update button states
            deleteDeckButton.interactable = (deck != null);
            saveDeckButton.interactable = (deck != null);
        }
        
        private void UpdateDeckStats()
        {
            if (currentDeck == null)
                return;
            
            Dictionary<string, int> stats = deckManager.GetDeckStats(currentDeck);
            
            if (cardCountText != null)
                cardCountText.text = $"{stats["Total Cards"]} / {deckManager.MaxDeckSize}";
            
            if (monsterCountText != null)
                monsterCountText.text = stats["Monster Cards"].ToString();
            
            if (spellCountText != null)
                spellCountText.text = stats["Spell Cards"].ToString();
            
            if (trapCountText != null)
                trapCountText.text = stats["Trap Cards"].ToString();
            
            if (deckSizeSlider != null)
            {
                deckSizeSlider.maxValue = deckManager.MaxDeckSize;
                deckSizeSlider.value = stats["Total Cards"];
            }
            
            // Show/hide empty deck text
            if (emptyDeckText != null)
                emptyDeckText.gameObject.SetActive(stats["Total Cards"] == 0);
        }
        
        private void RefreshCollectionView()
        {
            ClearCollectionView();
            
            if (playerCollection == null || collectionCardContainer == null || cardPrefab == null)
                return;
            
            List<YuGiOhCard> cards = playerCollection.GetAllCards();
            ApplyFilters(cards);
            
            foreach (var card in filteredCollection)
            {
                // Create card UI
                GameObject cardObj = Instantiate(cardPrefab, collectionCardContainer);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                
                if (cardUI != null)
                {
                    cardUI.SetCardData(card);
                    
                    // Add click listener
                    Button button = cardObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => SelectCardFromCollection(card));
                    }
                    
                    collectionCardUIItems.Add(cardUI);
                }
            }
        }
        
        private void ClearCollectionView()
        {
            if (collectionCardContainer == null)
                return;
            
            foreach (var cardUI in collectionCardUIItems)
            {
                if (cardUI != null)
                    Destroy(cardUI.gameObject);
            }
            
            collectionCardUIItems.Clear();
        }
        
        private void RefreshDeckView()
        {
            ClearDeckView();
            
            if (currentDeck == null || deckCardContainer == null || cardPrefab == null)
                return;
            
            List<YuGiOhCard> cards = currentDeck.GetCards(apiManager);
            
            foreach (var card in cards)
            {
                // Create card UI
                GameObject cardObj = Instantiate(cardPrefab, deckCardContainer);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                
                if (cardUI != null)
                {
                    cardUI.SetCardData(card);
                    
                    // Add click listener
                    Button button = cardObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => SelectCardFromDeck(card));
                    }
                    
                    deckCardUIItems.Add(cardUI);
                }
            }
            
            UpdateDeckStats();
        }
        
        private void ClearDeckView()
        {
            if (deckCardContainer == null)
                return;
            
            foreach (var cardUI in deckCardUIItems)
            {
                if (cardUI != null)
                    Destroy(cardUI.gameObject);
            }
            
            deckCardUIItems.Clear();
            
            if (emptyDeckText != null)
                emptyDeckText.gameObject.SetActive(true);
        }
        
        private void ApplyFilters()
        {
            List<YuGiOhCard> cards = playerCollection.GetAllCards();
            ApplyFilters(cards);
            RefreshCollectionView();
        }
        
        private void ApplyFilters(List<YuGiOhCard> cards)
        {
            filteredCollection.Clear();
            
            // Apply type filter
            string typeFilter = "All Types";
            if (cardTypeFilter != null && cardTypeFilter.value > 0)
            {
                typeFilter = cardTypeFilter.options[cardTypeFilter.value].text;
            }
            
            // Apply rarity filter
            string rarityFilter = "All Rarities";
            if (cardRarityFilter != null && cardRarityFilter.value > 0)
            {
                rarityFilter = cardRarityFilter.options[cardRarityFilter.value].text;
            }
            
            // Apply search filter
            string searchFilter = "";
            if (searchInput != null)
            {
                searchFilter = searchInput.text.ToLower();
            }
            
            // Apply all filters
            foreach (var card in cards)
            {
                bool matchesType = typeFilter == "All Types" || card.Type.Contains(typeFilter);
                bool matchesRarity = rarityFilter == "All Rarities" || card.Rarity == rarityFilter;
                bool matchesSearch = string.IsNullOrEmpty(searchFilter) || 
                                    card.Name.ToLower().Contains(searchFilter) || 
                                    card.Description.ToLower().Contains(searchFilter);
                
                if (matchesType && matchesRarity && matchesSearch)
                {
                    filteredCollection.Add(card);
                }
            }
        }
        
        private void ClearFilters()
        {
            if (cardTypeFilter != null)
                cardTypeFilter.value = 0;
            
            if (cardRarityFilter != null)
                cardRarityFilter.value = 0;
            
            if (searchInput != null)
                searchInput.text = "";
            
            ApplyFilters();
        }
        
        private void SelectCardFromCollection(YuGiOhCard card)
        {
            selectedCard = card;
            ShowCardDetails(card, true);
        }
        
        private void SelectCardFromDeck(YuGiOhCard card)
        {
            selectedCard = card;
            ShowCardDetails(card, false);
        }
        
        private void ShowCardDetails(YuGiOhCard card, bool fromCollection)
        {
            if (card == null || cardDetailsPanel == null)
                return;
            
            // Show the details panel
            cardDetailsPanel.SetActive(true);
            
            // Update card info
            if (cardNameText != null)
                cardNameText.text = card.Name;
            
            if (cardTypeText != null)
                cardTypeText.text = card.Type;
            
            if (cardDescriptionText != null)
                cardDescriptionText.text = card.Description;
            
            if (cardRarityText != null)
                cardRarityText.text = card.Rarity;
            
            // Load card image
            if (cardDetailImage != null && !string.IsNullOrEmpty(card.ImageUrl))
            {
                // TODO: Implement image loading from URL
                // For now, set a placeholder color
                cardDetailImage.color = GetCardTypeColor(card.Type);
            }
            
            // Set button states
            if (addCardButton != null)
            {
                int copiesInDeck = currentDeck?.cardIds.Count(id => id == card.Id) ?? 0;
                int maxCopies = 3; // This should match the DeckManager's maxCopiesPerCard
                bool canAdd = fromCollection && currentDeck != null && copiesInDeck < maxCopies;
                
                addCardButton.gameObject.SetActive(fromCollection);
                addCardButton.interactable = canAdd;
            }
            
            if (removeCardButton != null)
            {
                removeCardButton.gameObject.SetActive(!fromCollection);
            }
        }
        
        private Color GetCardTypeColor(string cardType)
        {
            if (string.IsNullOrEmpty(cardType))
                return Color.gray;
            
            if (cardType.Contains("Normal Monster"))
                return new Color(0.8f, 0.7f, 0.5f);
            if (cardType.Contains("Effect Monster"))
                return new Color(0.8f, 0.6f, 0.3f);
            if (cardType.Contains("Ritual Monster"))
                return new Color(0.4f, 0.6f, 0.8f);
            if (cardType.Contains("Fusion Monster"))
                return new Color(0.6f, 0.3f, 0.7f);
            if (cardType.Contains("Synchro Monster"))
                return new Color(0.8f, 0.8f, 0.8f);
            if (cardType.Contains("XYZ Monster"))
                return new Color(0.3f, 0.3f, 0.3f);
            if (cardType.Contains("Pendulum Monster"))
                return new Color(0.4f, 0.8f, 0.6f);
            if (cardType.Contains("Link Monster"))
                return new Color(0.2f, 0.4f, 0.8f);
            if (cardType.Contains("Spell"))
                return new Color(0.2f, 0.7f, 0.4f);
            if (cardType.Contains("Trap"))
                return new Color(0.8f, 0.2f, 0.2f);
            
            return Color.gray;
        }
        
        private void HideCardDetails()
        {
            if (cardDetailsPanel != null)
            {
                cardDetailsPanel.SetActive(false);
            }
            
            selectedCard = null;
        }
        
        private void HandleCreateDeckClicked()
        {
            string newDeckName = "New Deck";
            Deck newDeck = deckManager.CreateDeck(newDeckName);
            
            if (newDeck != null)
            {
                SelectDeck(newDeck);
                RefreshDeckSelector();
            }
        }
        
        private void HandleDeleteDeckClicked()
        {
            if (currentDeck == null)
                return;
            
            ShowConfirmation($"Delete deck '{currentDeck.name}'? This cannot be undone.", () => {
                deckManager.DeleteDeck(currentDeck);
                currentDeck = null;
            });
        }
        
        private void HandleSaveDeckClicked()
        {
            if (currentDeck == null)
                return;
            
            string newName = deckNameInput.text;
            string newDescription = deckDescriptionInput.text;
            
            if (string.IsNullOrEmpty(newName))
            {
                // Show error message
                return;
            }
            
            deckManager.RenameDeck(currentDeck, newName);
            deckManager.UpdateDeckDescription(currentDeck, newDescription);
            
            RefreshDeckSelector();
        }
        
        private void HandleCancelClicked()
        {
            // Return to previous screen or reset changes
            gameObject.SetActive(false);
        }
        
        private void HandleAddCardClicked()
        {
            if (currentDeck == null || selectedCard == null)
                return;
            
            if (deckManager.AddCardToDeck(currentDeck, selectedCard.Id))
            {
                // Card added successfully
                RefreshDeckView();
                HideCardDetails();
            }
        }
        
        private void HandleRemoveCardClicked()
        {
            if (currentDeck == null || selectedCard == null)
                return;
            
            if (deckManager.RemoveCardFromDeck(currentDeck, selectedCard.Id))
            {
                // Card removed successfully
                RefreshDeckView();
                HideCardDetails();
            }
        }
        
        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            if (confirmationDialog == null)
                return;
            
            confirmationDialog.SetActive(true);
            
            if (confirmationText != null)
                confirmationText.text = message;
            
            confirmationCallback = onConfirm;
        }
        
        private void HandleConfirmClicked()
        {
            if (confirmationDialog != null)
                confirmationDialog.SetActive(false);
            
            confirmationCallback?.Invoke();
            confirmationCallback = null;
        }
        
        private void HandleCancelConfirmClicked()
        {
            if (confirmationDialog != null)
                confirmationDialog.SetActive(false);
            
            confirmationCallback = null;
        }
        
        private void TogglePanel(GameObject panel, bool show)
        {
            if (panel == null)
                return;
            
            if (show)
            {
                // Hide all panels first
                if (collectionPanel != null)
                    collectionPanel.SetActive(false);
                
                if (deckPanel != null)
                    deckPanel.SetActive(false);
                
                // Show the requested panel
                panel.SetActive(true);
            }
            else
            {
                panel.SetActive(false);
            }
        }
        
        private void HandleDeckCreated(Deck deck)
        {
            RefreshDeckSelector();
        }
        
        private void HandleDeckModified(Deck deck)
        {
            if (deck == currentDeck)
            {
                RefreshDeckView();
            }
        }
        
        private void HandleDeckDeleted(Deck deck)
        {
            RefreshDeckSelector();
        }
    }
} 