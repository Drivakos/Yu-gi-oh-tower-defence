using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Cards
{
    public class DeckManager : MonoBehaviour
    {
        [SerializeField] private CardDatabase cardDatabase;
        [SerializeField] private int maxDecks = 5;
        
        private List<Deck> decks = new List<Deck>();
        private Deck currentDeck;
        
        public int DeckCount => decks.Count;
        public Deck CurrentDeck => currentDeck;
        public bool HasValidDeck => currentDeck != null && currentDeck.IsValid();
        
        private void Awake()
        {
            if (cardDatabase == null)
            {
                Debug.LogError("CardDatabase reference is missing!");
                enabled = false;
                return;
            }
            
            LoadDecks();
        }
        
        public bool CreateNewDeck(string deckName)
        {
            if (string.IsNullOrEmpty(deckName))
            {
                Debug.LogError("Deck name cannot be null or empty");
                return false;
            }
            
            if (decks.Count >= maxDecks)
            {
                Debug.LogError($"Maximum number of decks ({maxDecks}) reached");
                return false;
            }
            
            // Check if deck name already exists
            foreach (Deck deck in decks)
            {
                if (deck.DeckName == deckName)
                {
                    Debug.LogError($"Deck with name {deckName} already exists");
                    return false;
                }
            }
            
            Deck newDeck = new Deck(deckName, cardDatabase);
            decks.Add(newDeck);
            currentDeck = newDeck;
            return true;
        }
        
        public bool DeleteDeck(string deckName)
        {
            Deck deckToDelete = decks.Find(d => d.DeckName == deckName);
            if (deckToDelete == null)
            {
                Debug.LogError($"Deck with name {deckName} not found");
                return false;
            }
            
            decks.Remove(deckToDelete);
            
            // Clear saved data
            PlayerPrefs.DeleteKey($"Deck_{deckName}_Name");
            PlayerPrefs.DeleteKey($"Deck_{deckName}_Main");
            PlayerPrefs.DeleteKey($"Deck_{deckName}_Extra");
            PlayerPrefs.Save();
            
            // If we deleted the current deck, select another one if available
            if (currentDeck == deckToDelete)
            {
                currentDeck = decks.Count > 0 ? decks[0] : null;
            }
            
            return true;
        }
        
        public bool SelectDeck(string deckName)
        {
            Deck deck = decks.Find(d => d.DeckName == deckName);
            if (deck == null)
            {
                Debug.LogError($"Deck with name {deckName} not found");
                return false;
            }
            
            currentDeck = deck;
            return true;
        }
        
        public List<string> GetDeckNames()
        {
            List<string> names = new List<string>();
            foreach (Deck deck in decks)
            {
                names.Add(deck.DeckName);
            }
            return names;
        }
        
        public Deck GetDeck(string deckName)
        {
            return decks.Find(d => d.DeckName == deckName);
        }
        
        private void LoadDecks()
        {
            // Find all saved decks
            for (int i = 0; i < maxDecks; i++)
            {
                string deckName = PlayerPrefs.GetString($"Deck_{i}_Name", "");
                if (!string.IsNullOrEmpty(deckName))
                {
                    Deck deck = new Deck(deckName, cardDatabase);
                    deck.LoadDeck(deckName);
                    decks.Add(deck);
                }
            }
            
            // Select the first deck if available
            if (decks.Count > 0)
            {
                currentDeck = decks[0];
            }
        }
        
        public void SaveAllDecks()
        {
            foreach (Deck deck in decks)
            {
                deck.SaveDeck();
            }
        }
        
        public bool IsDeckNameValid(string deckName)
        {
            if (string.IsNullOrEmpty(deckName))
            {
                return false;
            }
            
            // Check if name already exists
            foreach (Deck deck in decks)
            {
                if (deck.DeckName == deckName)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void ClearAllDecks()
        {
            foreach (Deck deck in decks)
            {
                PlayerPrefs.DeleteKey($"Deck_{deck.DeckName}_Name");
                PlayerPrefs.DeleteKey($"Deck_{deck.DeckName}_Main");
                PlayerPrefs.DeleteKey($"Deck_{deck.DeckName}_Extra");
            }
            
            decks.Clear();
            currentDeck = null;
            PlayerPrefs.Save();
        }
    }
} 