using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using System.Linq;

namespace YuGiOhTowerDefense.Core.UI
{
    public class CardSearchPanel : MonoBehaviour
    {
        [Header("Search Input")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;
        
        [Header("Filters")]
        [SerializeField] private Toggle monsterToggle;
        [SerializeField] private Toggle spellToggle;
        [SerializeField] private Toggle trapToggle;
        [SerializeField] private TMP_Dropdown attributeDropdown;
        [SerializeField] private TMP_Dropdown typeDropdown;
        
        [Header("Results")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private GameObject cardButtonPrefab;
        
        private List<CardData> allCards;
        private List<CardButton> cardButtons = new List<CardButton>();
        
        private void Awake()
        {
            searchButton.onClick.AddListener(OnSearchClicked);
            monsterToggle.onValueChanged.AddListener(_ => OnSearchClicked());
            spellToggle.onValueChanged.AddListener(_ => OnSearchClicked());
            trapToggle.onValueChanged.AddListener(_ => OnSearchClicked());
            attributeDropdown.onValueChanged.AddListener(_ => OnSearchClicked());
            typeDropdown.onValueChanged.AddListener(_ => OnSearchClicked());
            
            InitializeDropdowns();
        }
        
        private void OnDestroy()
        {
            searchButton.onClick.RemoveListener(OnSearchClicked);
            monsterToggle.onValueChanged.RemoveAllListeners();
            spellToggle.onValueChanged.RemoveAllListeners();
            trapToggle.onValueChanged.RemoveAllListeners();
            attributeDropdown.onValueChanged.RemoveAllListeners();
            typeDropdown.onValueChanged.RemoveAllListeners();
        }
        
        public void Initialize(List<CardData> cards)
        {
            allCards = cards;
            OnSearchClicked();
        }
        
        private void InitializeDropdowns()
        {
            // Initialize attribute dropdown
            attributeDropdown.ClearOptions();
            attributeDropdown.AddOptions(new List<string> { "All Attributes" }
                .Concat(System.Enum.GetNames(typeof(MonsterAttribute)).ToList())
                .ToList());
                
            // Initialize type dropdown
            typeDropdown.ClearOptions();
            typeDropdown.AddOptions(new List<string> { "All Types" }
                .Concat(System.Enum.GetNames(typeof(MonsterType)).ToList())
                .ToList());
        }
        
        private void OnSearchClicked()
        {
            string searchTerm = searchInput.text.ToLower();
            bool showMonsters = monsterToggle.isOn;
            bool showSpells = spellToggle.isOn;
            bool showTraps = trapToggle.isOn;
            string selectedAttribute = attributeDropdown.value > 0 ? 
                attributeDropdown.options[attributeDropdown.value].text : null;
            string selectedType = typeDropdown.value > 0 ? 
                typeDropdown.options[typeDropdown.value].text : null;
                
            var filteredCards = allCards.Where(card => 
            {
                // Filter by card type
                if (card is MonsterCardData && !showMonsters) return false;
                if (card is SpellCardData && !showSpells) return false;
                if (card is TrapCardData && !showTraps) return false;
                
                // Filter by search term
                if (!string.IsNullOrEmpty(searchTerm) && 
                    !card.CardName.ToLower().Contains(searchTerm) &&
                    !card.Description.ToLower().Contains(searchTerm))
                    return false;
                    
                // Filter by attribute and type for monsters
                if (card is MonsterCardData monster)
                {
                    if (!string.IsNullOrEmpty(selectedAttribute) && 
                        monster.Attribute.ToString() != selectedAttribute)
                        return false;
                        
                    if (!string.IsNullOrEmpty(selectedType) && 
                        monster.Type.ToString() != selectedType)
                        return false;
                }
                
                return true;
            }).ToList();
            
            DisplayResults(filteredCards);
        }
        
        private void DisplayResults(List<CardData> cards)
        {
            // Clear existing buttons
            foreach (var button in cardButtons)
            {
                Destroy(button.gameObject);
            }
            cardButtons.Clear();
            
            // Create new buttons
            foreach (var card in cards)
            {
                var buttonObj = Instantiate(cardButtonPrefab, resultsContainer);
                var cardButton = buttonObj.GetComponent<CardButton>();
                if (cardButton != null)
                {
                    cardButton.Initialize(card);
                    cardButtons.Add(cardButton);
                }
            }
        }
    }
} 