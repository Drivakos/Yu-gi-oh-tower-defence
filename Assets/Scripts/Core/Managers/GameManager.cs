using System.Collections.Generic;
using UnityEngine;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Factories;

namespace YuGiOhTowerDefense.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startingHandSize = 5;
        [SerializeField] private int maxHandSize = 7;
        [SerializeField] private Transform cardSpawnPoint;
        [SerializeField] private CardFactory cardFactory;

        private List<YuGiOhCard> deck = new List<YuGiOhCard>();
        private List<YuGiOhCard> hand = new List<YuGiOhCard>();
        private List<YuGiOhCard> graveyard = new List<YuGiOhCard>();
        private List<MonsterCard> field = new List<MonsterCard>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeDeck();
            DrawStartingHand();
        }

        private void InitializeDeck()
        {
            // TODO: Load deck from save data or create default deck
            // For now, we'll create a test deck
            for (int i = 0; i < 40; i++)
            {
                YuGiOhCard card = cardFactory.CreateRandomCard();
                deck.Add(card);
            }
        }

        private void DrawStartingHand()
        {
            for (int i = 0; i < startingHandSize; i++)
            {
                DrawCard();
            }
        }

        public void DrawCard()
        {
            if (deck.Count == 0)
            {
                Debug.Log("No more cards in deck!");
                return;
            }

            if (hand.Count >= maxHandSize)
            {
                Debug.Log("Hand is full!");
                return;
            }

            YuGiOhCard card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            // Spawn card in hand
            if (cardSpawnPoint != null)
            {
                GameObject cardObj = Instantiate(card.gameObject, cardSpawnPoint.position, Quaternion.identity);
                cardObj.transform.SetParent(cardSpawnPoint);
            }
        }

        public void PlayCard(YuGiOhCard card, Vector3 position)
        {
            if (!hand.Contains(card))
            {
                Debug.Log("Card not in hand!");
                return;
            }

            hand.Remove(card);

            if (card is MonsterCard monsterCard)
            {
                field.Add(monsterCard);
                monsterCard.Summon();
                // Position monster on the field
                monsterCard.transform.position = position;
            }
            else if (card is SpellCard spellCard)
            {
                spellCard.Activate();
                graveyard.Add(spellCard);
            }
            else if (card is TrapCard trapCard)
            {
                trapCard.Set();
                // Trap remains on field until activated
            }
        }

        public void MoveCardToGraveyard(YuGiOhCard card)
        {
            if (field.Contains(card as MonsterCard))
            {
                field.Remove(card as MonsterCard);
            }
            graveyard.Add(card);
        }

        public List<MonsterCard> GetFieldMonsters()
        {
            return new List<MonsterCard>(field);
        }

        public List<YuGiOhCard> GetHand()
        {
            return new List<YuGiOhCard>(hand);
        }
    }
} 