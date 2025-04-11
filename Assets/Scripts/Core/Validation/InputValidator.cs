using System;
using System.Text.RegularExpressions;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Validation
{
    public static class InputValidator
    {
        private const int MIN_DECK_NAME_LENGTH = 3;
        private const int MAX_DECK_NAME_LENGTH = 50;
        private static readonly Regex INVALID_CHARS_REGEX = new Regex(@"[<>:""/\\|?*]");
        
        public static bool ValidateDeckName(string name, out string error)
        {
            error = null;
            
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Deck name cannot be empty";
                return false;
            }
            
            if (name.Length < MIN_DECK_NAME_LENGTH)
            {
                error = $"Deck name must be at least {MIN_DECK_NAME_LENGTH} characters long";
                return false;
            }
            
            if (name.Length > MAX_DECK_NAME_LENGTH)
            {
                error = $"Deck name cannot be longer than {MAX_DECK_NAME_LENGTH} characters";
                return false;
            }
            
            if (INVALID_CHARS_REGEX.IsMatch(name))
            {
                error = "Deck name contains invalid characters";
                return false;
            }
            
            return true;
        }
        
        public static bool ValidateCardData(CardData card, out string error)
        {
            error = null;
            
            if (card == null)
            {
                error = "Card data cannot be null";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(card.CardId))
            {
                error = "Card ID cannot be empty";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(card.CardName))
            {
                error = "Card name cannot be empty";
                return false;
            }
            
            if (card is MonsterCardData monster)
            {
                if (monster.Attack < 0)
                {
                    error = "Monster attack cannot be negative";
                    return false;
                }
                
                if (monster.Defense < 0)
                {
                    error = "Monster defense cannot be negative";
                    return false;
                }
                
                if (monster.Level < 1 || monster.Level > 12)
                {
                    error = "Monster level must be between 1 and 12";
                    return false;
                }
            }
            
            return true;
        }
        
        public static bool ValidateDeckSize(int cardCount, out string error)
        {
            error = null;
            
            if (cardCount < 40)
            {
                error = "Deck must have at least 40 cards";
                return false;
            }
            
            if (cardCount > 60)
            {
                error = "Deck cannot have more than 60 cards";
                return false;
            }
            
            return true;
        }
        
        public static bool ValidateCardCopies(int copyCount, out string error)
        {
            error = null;
            
            if (copyCount > 3)
            {
                error = "Cannot have more than 3 copies of any card";
                return false;
            }
            
            return true;
        }
        
        public static bool ValidateDeckComposition(int monsterCount, int spellCount, int trapCount, out string error)
        {
            error = null;
            
            int totalCards = monsterCount + spellCount + trapCount;
            
            if (totalCards < 40)
            {
                error = "Deck must have at least 40 cards";
                return false;
            }
            
            if (totalCards > 60)
            {
                error = "Deck cannot have more than 60 cards";
                return false;
            }
            
            // Optional: Add composition validation rules
            // Example: Require at least 20 monster cards
            if (monsterCount < 20)
            {
                error = "Deck must have at least 20 monster cards";
                return false;
            }
            
            return true;
        }
    }
} 