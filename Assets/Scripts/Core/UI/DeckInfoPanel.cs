using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.UI
{
    public class DeckInfoPanel : MonoBehaviour
    {
        [Header("Deck Info")]
        [SerializeField] private TMP_Text deckNameText;
        [SerializeField] private TMP_Text cardCountText;
        [SerializeField] private TMP_Text monsterCountText;
        [SerializeField] private TMP_Text spellCountText;
        [SerializeField] private TMP_Text trapCountText;
        
        [Header("Validation")]
        [SerializeField] private GameObject validationPanel;
        [SerializeField] private TMP_Text validationText;
        [SerializeField] private Image validationIcon;
        [SerializeField] private Sprite validIcon;
        [SerializeField] private Sprite invalidIcon;
        
        public void UpdateDeckInfo(Deck deck)
        {
            if (deck == null)
            {
                ClearInfo();
                return;
            }
            
            deckNameText.text = deck.DeckName;
            cardCountText.text = $"Total: {deck.CardCount}";
            
            // Count card types
            int monsterCount = 0;
            int spellCount = 0;
            int trapCount = 0;
            
            foreach (var card in deck.Cards)
            {
                if (card is MonsterCardData)
                    monsterCount++;
                else if (card is SpellCardData)
                    spellCount++;
                else if (card is TrapCardData)
                    trapCount++;
            }
            
            monsterCountText.text = $"Monsters: {monsterCount}";
            spellCountText.text = $"Spells: {spellCount}";
            trapCountText.text = $"Traps: {trapCount}";
            
            // Update validation status
            UpdateValidationStatus(deck.IsValid());
        }
        
        public void UpdateValidationStatus(bool isValid)
        {
            validationPanel.SetActive(true);
            validationText.text = isValid ? "Deck is valid" : "Deck is invalid";
            validationIcon.sprite = isValid ? validIcon : invalidIcon;
            validationIcon.color = isValid ? Color.green : Color.red;
        }
        
        private void ClearInfo()
        {
            deckNameText.text = "No Deck Selected";
            cardCountText.text = "Total: 0";
            monsterCountText.text = "Monsters: 0";
            spellCountText.text = "Spells: 0";
            trapCountText.text = "Traps: 0";
            validationPanel.SetActive(false);
        }
    }
} 