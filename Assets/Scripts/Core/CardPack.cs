using System.Collections.Generic;
using UnityEngine;

namespace YuGiOhTowerDefense.Core
{
    [CreateAssetMenu(fileName = "New Card Pack", menuName = "YuGiOh/Card Pack")]
    public class CardPack : ScriptableObject
    {
        [Header("Pack Information")]
        [SerializeField] private string packName;
        [SerializeField] [TextArea(2, 4)] private string description;
        [SerializeField] private Sprite packIcon;
        [SerializeField] private int cost;
        [SerializeField] private int cardsPerPack = 5;

        [Header("Card Distribution")]
        [SerializeField] private List<Card> commonCards = new List<Card>();
        [SerializeField] private List<Card> rareCards = new List<Card>();
        [SerializeField] private List<Card> superRareCards = new List<Card>();
        [SerializeField] private List<Card> ultraRareCards = new List<Card>();

        [Header("Rarity Chances")]
        [Range(0, 100)] [SerializeField] private float commonChance = 70f;
        [Range(0, 100)] [SerializeField] private float rareChance = 20f;
        [Range(0, 100)] [SerializeField] private float superRareChance = 8f;
        [Range(0, 100)] [SerializeField] private float ultraRareChance = 2f;

        public string PackName => packName;
        public string Description => description;
        public Sprite PackIcon => packIcon;
        public int Cost => cost;
        public int CardsPerPack => cardsPerPack;

        public List<Card> GetCardsByRarity(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => commonCards,
                CardRarity.Rare => rareCards,
                CardRarity.SuperRare => superRareCards,
                CardRarity.UltraRare => ultraRareCards,
                _ => new List<Card>()
            };
        }

        public float GetRarityChance(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => commonChance,
                CardRarity.Rare => rareChance,
                CardRarity.SuperRare => superRareChance,
                CardRarity.UltraRare => ultraRareChance,
                _ => 0f
            };
        }
    }
} 