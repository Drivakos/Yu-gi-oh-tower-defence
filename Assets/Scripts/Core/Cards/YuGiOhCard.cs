using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Base
{
    public class YuGiOhCard : MonoBehaviour
    {
        [SerializeField] protected string cardName;
        [SerializeField] protected string description;
        [SerializeField] protected CardType cardType;
        [SerializeField] protected Sprite cardImage;
        [SerializeField] protected int cardId;

        protected bool isSummoned;
        protected bool canAttack;

        public string CardName => cardName;
        public string Description => description;
        public CardType CardType => cardType;
        public Sprite CardImage => cardImage;
        public int CardId => cardId;
        public bool IsSummoned => isSummoned;
        public bool CanAttack => canAttack;

        public virtual void Initialize(string name, string desc, CardType type, Sprite image, int id)
        {
            cardName = name;
            description = desc;
            cardType = type;
            cardImage = image;
            cardId = id;
            isSummoned = false;
            canAttack = false;
        }

        public virtual void Summon()
        {
            isSummoned = true;
            canAttack = false; // Can't attack the turn it's summoned
        }

        public virtual void NewTurn()
        {
            canAttack = true;
        }

        public virtual void OnDestroy()
        {
            // Clean up any resources or references
        }
    }
} 