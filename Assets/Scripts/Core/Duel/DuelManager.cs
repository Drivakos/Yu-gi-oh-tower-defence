using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Duel
{
    public class DuelManager : MonoBehaviour
    {
        [Header("Duel Settings")]
        [SerializeField] private int startingLifePoints = 8000;
        [SerializeField] private int startingHandSize = 5;
        
        [Header("Zones")]
        [SerializeField] private Transform[] monsterZones;
        [SerializeField] private Transform[] spellTrapZones;
        [SerializeField] private Transform fieldZone;
        [SerializeField] private Transform graveyardZone;
        [SerializeField] private Transform deckZone;
        [SerializeField] private Transform extraDeckZone;
        
        private int currentLifePoints;
        private List<YuGiOhCard> mainDeck = new List<YuGiOhCard>();
        private List<YuGiOhCard> extraDeck = new List<YuGiOhCard>();
        private List<YuGiOhCard> graveyard = new List<YuGiOhCard>();
        private List<YuGiOhCard> hand = new List<YuGiOhCard>();
        private YuGiOhCard[] fieldMonsters = new YuGiOhCard[5];
        private YuGiOhCard[] fieldSpellTraps = new YuGiOhCard[5];
        private YuGiOhCard fieldSpell;
        
        private bool isPlayerTurn = true;
        private DuelPhase currentPhase = DuelPhase.Draw;
        
        public event System.Action<int> OnLifePointsChanged;
        public event System.Action<DuelPhase> OnPhaseChanged;
        public event System.Action<YuGiOhCard> OnCardDrawn;
        public event System.Action<YuGiOhCard> OnCardPlayed;
        
        private void Start()
        {
            InitializeDuel();
        }
        
        private void InitializeDuel()
        {
            currentLifePoints = startingLifePoints;
            OnLifePointsChanged?.Invoke(currentLifePoints);
            
            // TODO: Initialize decks from player's collection
            ShuffleDeck();
            
            // Draw starting hand
            for (int i = 0; i < startingHandSize; i++)
            {
                DrawCard();
            }
            
            StartTurn();
        }
        
        private void StartTurn()
        {
            currentPhase = DuelPhase.Draw;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // Draw Phase
            DrawCard();
            
            // Move to Main Phase 1
            currentPhase = DuelPhase.Main1;
            OnPhaseChanged?.Invoke(currentPhase);
        }
        
        public void EndTurn()
        {
            // End Phase
            currentPhase = DuelPhase.End;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // Switch turns
            isPlayerTurn = !isPlayerTurn;
            StartTurn();
        }
        
        public void DrawCard()
        {
            if (mainDeck.Count > 0)
            {
                YuGiOhCard card = mainDeck[0];
                mainDeck.RemoveAt(0);
                hand.Add(card);
                OnCardDrawn?.Invoke(card);
            }
        }
        
        public bool PlayCard(YuGiOhCard card, int zoneIndex)
        {
            if (!hand.Contains(card)) return false;
            
            switch (card.CardType)
            {
                case CardType.Monster:
                    return PlayMonsterCard(card, zoneIndex);
                case CardType.Spell:
                case CardType.Trap:
                    return PlaySpellTrapCard(card, zoneIndex);
                default:
                    return false;
            }
        }
        
        private bool PlayMonsterCard(YuGiOhCard card, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= monsterZones.Length) return false;
            if (fieldMonsters[zoneIndex] != null) return false;
            
            hand.Remove(card);
            fieldMonsters[zoneIndex] = card;
            
            // Instantiate monster in the zone
            GameObject monster = Instantiate(card.Prefab, monsterZones[zoneIndex]);
            monster.transform.localPosition = Vector3.zero;
            
            OnCardPlayed?.Invoke(card);
            return true;
        }
        
        private bool PlaySpellTrapCard(YuGiOhCard card, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= spellTrapZones.Length) return false;
            if (fieldSpellTraps[zoneIndex] != null) return false;
            
            hand.Remove(card);
            fieldSpellTraps[zoneIndex] = card;
            
            // Instantiate spell/trap in the zone
            GameObject spellTrap = Instantiate(card.Prefab, spellTrapZones[zoneIndex]);
            spellTrap.transform.localPosition = Vector3.zero;
            
            OnCardPlayed?.Invoke(card);
            return true;
        }
        
        public void ActivateTrap(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= spellTrapZones.Length) return;
            
            YuGiOhCard card = fieldSpellTraps[zoneIndex];
            if (card == null || card.CardType != CardType.Trap) return;
            
            // TODO: Implement trap activation logic
            // This will depend on the specific trap card
            
            // Send to graveyard after activation
            fieldSpellTraps[zoneIndex] = null;
            graveyard.Add(card);
        }
        
        private void ShuffleDeck()
        {
            // Fisher-Yates shuffle algorithm
            for (int i = mainDeck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                YuGiOhCard temp = mainDeck[i];
                mainDeck[i] = mainDeck[j];
                mainDeck[j] = temp;
            }
        }
    }
    
    public enum DuelPhase
    {
        Draw,
        Standby,
        Main1,
        Battle,
        Main2,
        End
    }
} 