using UnityEngine;

namespace YuGiOhTowerDefense.Core
{
    public abstract class Card : MonoBehaviour
    {
        [Header("Card Information")]
        [SerializeField] protected string cardName;
        [SerializeField] protected string cardDescription;
        [SerializeField] protected Sprite cardImage;
        [SerializeField] protected CardType cardType;
        [SerializeField] protected CardRarity rarity;
        [SerializeField] protected CardPack[] availableInPacks;

        public string CardName => cardName;
        public string CardDescription => cardDescription;
        public Sprite CardImage => cardImage;
        public CardType CardType => cardType;
        public CardRarity Rarity => rarity;
        public CardPack[] AvailableInPacks => availableInPacks;

        protected virtual void Start()
        {
            // Initialize card
        }

        protected virtual void Update()
        {
            // Update card state
        }

        protected abstract void ApplyEffect();
    }
} 