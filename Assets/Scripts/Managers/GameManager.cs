using UnityEngine;
using System.Collections.Generic;
using YuGiOh.Cards;

namespace YuGiOh.Managers
{
    public class GameManager : BaseManager
    {
        [Header("Game Settings")]
        [SerializeField] private int startingDuelPoints = 1000;
        [SerializeField] private int maxDuelPoints = 9999;

        private int currentDuelPoints;
        private List<Card> activeCards = new List<Card>();

        private void Start()
        {
            currentDuelPoints = startingDuelPoints;
        }

        public void RegisterCard(Card card)
        {
            if (!activeCards.Contains(card))
            {
                activeCards.Add(card);
            }
        }

        public void UnregisterCard(Card card)
        {
            activeCards.Remove(card);
        }

        public bool CanAffordMonster(int cost)
        {
            return currentDuelPoints >= cost;
        }

        public void SpendDuelPoints(int amount)
        {
            currentDuelPoints = Mathf.Max(0, currentDuelPoints - amount);
        }

        public void AddDuelPoints(int amount)
        {
            currentDuelPoints = Mathf.Min(maxDuelPoints, currentDuelPoints + amount);
        }

        public int GetCurrentDuelPoints()
        {
            return currentDuelPoints;
        }

        public List<Card> GetActiveCards()
        {
            return activeCards;
        }
    }
} 