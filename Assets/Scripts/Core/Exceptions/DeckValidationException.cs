using System;

namespace YuGiOhTowerDefense.Core.Exceptions
{
    public enum DeckValidationError
    {
        InvalidSize,
        TooManyCopies,
        InvalidCard,
        DuplicateDeckName,
        InvalidDeckName,
        CardNotOwned,
        DeckNotFound
    }

    public class DeckValidationException : Exception
    {
        public DeckValidationError Error { get; }
        public string Details { get; }

        public DeckValidationException(DeckValidationError error, string details = null) 
            : base(GetErrorMessage(error, details))
        {
            Error = error;
            Details = details;
        }

        private static string GetErrorMessage(DeckValidationError error, string details)
        {
            return error switch
            {
                DeckValidationError.InvalidSize => "Deck size must be between 40 and 60 cards",
                DeckValidationError.TooManyCopies => "Cannot have more than 3 copies of any card",
                DeckValidationError.InvalidCard => "Invalid card data",
                DeckValidationError.DuplicateDeckName => "A deck with this name already exists",
                DeckValidationError.InvalidDeckName => "Invalid deck name",
                DeckValidationError.CardNotOwned => "You do not own this card",
                DeckValidationError.DeckNotFound => "Deck not found",
                _ => "Unknown deck validation error"
            } + (details != null ? $": {details}" : "");
        }
    }
} 